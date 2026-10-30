using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Windows.Graphics.Capture;
using Xunit.Abstractions;

namespace McggCompanion.Windows.Tests;

public sealed class CaptureDesktopProbe(ITestOutputHelper output)
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(15);

    [DesktopFact]
    public async Task Capture_HandlesPixelsResizeRepeatedStopRestartAndSourceClosure()
    {
        Assert.True(Environment.UserInteractive, "Pengujian capture memerlukan sesi Windows interaktif.");
        Assert.True(GraphicsCaptureSession.IsSupported(), "Windows Graphics Capture tidak tersedia pada lingkungan ini.");

        await using var fixture = new FixtureWindow();
        var windowHandle = await fixture.Ready.WaitAsync(Timeout);
        await using var itemCreator = new CaptureItemCreator();
        WindowCaptureSession? first = null;
        WindowCaptureSession? restarted = null;
        try
        {
            first = new WindowCaptureSession(await itemCreator.CreateAsync(windowHandle));
            var blue = await WaitForFrameAsync(first, frame => HasCenterColor(frame, red: false));
            AssertFrameLayout(blue);
            output.WriteLine($"Blue: {blue.Width}x{blue.Height}; stride={blue.Stride}; sequence={blue.SequenceNumber}");

            await fixture.ResizeAndMakeRedAsync();
            var red = await WaitForFrameAsync(first, frame =>
                frame.Width > blue.Width && frame.Height > blue.Height && HasCenterColor(frame, red: true));
            AssertFrameLayout(red);
            Assert.True(red.SequenceNumber > blue.SequenceNumber);
            Assert.True(red.ResizeCount > blue.ResizeCount);
            Assert.True(red.SystemRelativeTime >= blue.SystemRelativeTime);
            Assert.True(HasCenterColor(blue, red: false), "Frame yang sudah dimiliki tidak boleh berubah setelah copy berikutnya.");
            output.WriteLine($"Red: {red.Width}x{red.Height}; resize={red.ResizeCount}; copy={red.CopyMilliseconds:F2} ms");

            await first.DisposeAsync().AsTask().WaitAsync(Timeout);
            await first.DisposeAsync().AsTask().WaitAsync(Timeout);
            Assert.True(first.Completion.IsCompletedSuccessfully);
            Assert.Null(first.TakeLatestFrame());

            restarted = new WindowCaptureSession(await itemCreator.CreateAsync(windowHandle));
            var afterRestart = await WaitForFrameAsync(restarted, frame => HasCenterColor(frame, red: true));
            AssertFrameLayout(afterRestart);
            Assert.Equal(red.Width, afterRestart.Width);
            Assert.Equal(red.Height, afterRestart.Height);
            Assert.Null(first.TakeLatestFrame());

            await fixture.CloseAsync();
            Assert.False(IsWindow(windowHandle), "Fixture CloseAsync selesai tetapi HWND sumber masih hidup.");
            output.WriteLine("HWND fixture sudah dihancurkan; menunggu penyelesaian capture.");
            try
            {
                await restarted.Completion.WaitAsync(Timeout);
            }
            catch (TimeoutException exception)
            {
                throw new TimeoutException(
                    $"Capture belum selesai setelah HWND dihancurkan. Completion={restarted.Completion.Status}; " +
                    $"last={DescribeFrame(restarted.TakeLatestFrame() ?? afterRestart)}.", exception);
            }
            Assert.True(restarted.Completion.IsCompletedSuccessfully);
            Assert.Null(restarted.TakeLatestFrame());
            output.WriteLine("Stop berulang, restart, dan penutupan sumber selesai tanpa frame tertinggal.");
        }
        finally
        {
            try
            {
                if (restarted is not null)
                {
                    await restarted.DisposeAsync().AsTask().WaitAsync(Timeout);
                }
            }
            finally
            {
                if (first is not null)
                {
                    await first.DisposeAsync().AsTask().WaitAsync(Timeout);
                }
            }
        }
    }

    private static async Task<CapturedFrame> WaitForFrameAsync(
        WindowCaptureSession capture, Func<CapturedFrame, bool> matches)
    {
        var started = Stopwatch.GetTimestamp();
        CapturedFrame? lastFrame = null;
        var receivedFrames = 0;
        while (Stopwatch.GetElapsedTime(started) < Timeout)
        {
            var frame = capture.TakeLatestFrame();
            if (frame is not null)
            {
                lastFrame = frame;
                receivedFrames++;
                if (matches(frame))
                {
                    return frame;
                }
            }

            if (capture.Completion.IsCompleted)
            {
                await capture.Completion;
                throw new InvalidOperationException("Capture berhenti sebelum frame yang diharapkan tersedia.");
            }

            await Task.Delay(25);
        }

        throw new TimeoutException(
            $"Tidak menerima frame fixture yang diharapkan dalam 15 detik. " +
            $"Completion={capture.Completion.Status}; frames={receivedFrames}; last={DescribeFrame(lastFrame)}.");
    }

    private static string DescribeFrame(CapturedFrame? frame)
    {
        if (frame is null)
        {
            return "tidak ada";
        }

        var center = (long)(frame.Height / 2) * frame.Stride + (long)(frame.Width / 2) * 4;
        var color = center >= 0 && center <= frame.BgraPixels.Length - 4
            ? $"{frame.BgraPixels[center]},{frame.BgraPixels[center + 1]},{frame.BgraPixels[center + 2]},{frame.BgraPixels[center + 3]}"
            : "tidak valid";
        return $"{frame.Width}x{frame.Height}; stride={frame.Stride}; seq={frame.SequenceNumber}; " +
               $"resize={frame.ResizeCount}; BGRA=({color}); " +
               $"age={Stopwatch.GetElapsedTime(frame.ReceivedStopwatchTimestamp).TotalMilliseconds:F1}ms";
    }

    private static void AssertFrameLayout(CapturedFrame frame)
    {
        Assert.True(frame.Width > 0 && frame.Height > 0);
        Assert.Equal(checked(frame.Width * 4), frame.Stride);
        Assert.Equal(checked(frame.Stride * frame.Height), frame.BgraPixels.Length);
        Assert.True(frame.SequenceNumber > 0);
        Assert.True(frame.ReceivedStopwatchTimestamp > 0);
        Assert.True(double.IsFinite(frame.CopyMilliseconds) && frame.CopyMilliseconds >= 0);
    }

    private static bool HasCenterColor(CapturedFrame frame, bool red)
    {
        AssertFrameLayout(frame);
        var offset = checked((frame.Height / 2) * frame.Stride + (frame.Width / 2) * 4);
        var pixels = frame.BgraPixels;
        return pixels[offset + (red ? 2 : 0)] >= 240 &&
               pixels[offset + (red ? 0 : 2)] <= 15 &&
               pixels[offset + 1] <= 15 && pixels[offset + 3] >= 240;
    }

    private static GraphicsCaptureItem CreateItemForWindow(nint windowHandle)
    {
        var interopId = new Guid("3628E81B-3CAC-4C60-B7F4-23CE0E0C3356");
        var itemId = new Guid("79C3F95B-31F7-4EC2-A464-632EF5D30760");
        using var factory = WinRT.ActivationFactory.Get("Windows.Graphics.Capture.GraphicsCaptureItem", interopId);
        var vtable = Marshal.ReadIntPtr(factory.ThisPtr);
        var create = Marshal.GetDelegateForFunctionPointer<CreateForWindowMethod>(Marshal.ReadIntPtr(vtable, 3 * nint.Size));
        Marshal.ThrowExceptionForHR(create(factory.ThisPtr, windowHandle, in itemId, out var itemPointer));
        try
        {
            return WinRT.MarshalInterface<GraphicsCaptureItem>.FromAbi(itemPointer);
        }
        finally
        {
            Marshal.Release(itemPointer);
        }
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int CreateForWindowMethod(nint factory, nint window, in Guid interfaceId, out nint item);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindow(nint windowHandle);

    private sealed class CaptureItemCreator : IAsyncDisposable
    {
        private readonly global::Windows.System.DispatcherQueueController controller =
            global::Windows.System.DispatcherQueueController.CreateOnDedicatedThread();

        public async Task<GraphicsCaptureItem> CreateAsync(nint windowHandle)
        {
            var created = new TaskCompletionSource<GraphicsCaptureItem>(TaskCreationOptions.RunContinuationsAsynchronously);
            // Closed needs a DispatcherQueue on the item's creator thread, independently of the frame pool.
            if (!controller.DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    created.TrySetResult(CreateItemForWindow(windowHandle));
                }
                catch (Exception exception)
                {
                    created.TrySetException(exception);
                }
            }))
            {
                throw new InvalidOperationException("DispatcherQueue menolak pembuatan capture item.");
            }

            return await created.Task.WaitAsync(Timeout);
        }

        public async ValueTask DisposeAsync() =>
            await controller.ShutdownQueueAsync().AsTask().WaitAsync(Timeout);
    }

    private sealed class FixtureWindow : IAsyncDisposable
    {
        private readonly TaskCompletionSource<nint> ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource exited = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private Dispatcher? dispatcher;
        private Window? window;

        public FixtureWindow()
        {
            var thread = new Thread(Run) { IsBackground = true, Name = "Capture fixture STA" };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        public Task<nint> Ready => ready.Task;

        public async Task ResizeAndMakeRedAsync()
        {
            await Ready.WaitAsync(Timeout);
            await dispatcher!.InvokeAsync(() =>
            {
                window!.Width = 481;
                window.Height = 321;
                window.Background = Brushes.Red;
            }).Task.WaitAsync(Timeout);
        }

        public async Task CloseAsync()
        {
            var currentDispatcher = dispatcher;
            if (currentDispatcher is not null && !currentDispatcher.HasShutdownStarted)
            {
                await currentDispatcher.InvokeAsync(() =>
                {
                    window?.Close();
                }).Task.WaitAsync(Timeout);
            }
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                await CloseAsync();
            }
            finally
            {
                var currentDispatcher = dispatcher;
                if (currentDispatcher is not null && !currentDispatcher.HasShutdownStarted)
                {
                    await currentDispatcher.InvokeAsync(() =>
                        currentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background)).Task.WaitAsync(Timeout);
                }

                await exited.Task.WaitAsync(Timeout);
            }
        }

        private void Run()
        {
            try
            {
                dispatcher = Dispatcher.CurrentDispatcher;
                window = new Window
                {
                    Title = "MCGG Companion — Fixture capture sintetis",
                    Width = 321,
                    Height = 241,
                    Left = 64,
                    Top = 64,
                    WindowStyle = WindowStyle.None,
                    ResizeMode = ResizeMode.NoResize,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    Background = Brushes.Blue
                };
                window.Show();
                ready.TrySetResult(new WindowInteropHelper(window).Handle);
                Dispatcher.Run();
            }
            catch (Exception exception)
            {
                ready.TrySetException(exception);
                exited.TrySetException(exception);
            }
            finally
            {
                try
                {
                    window?.Close();
                }
                catch (Exception exception)
                {
                    exited.TrySetException(exception);
                }
                finally
                {
                    exited.TrySetResult();
                }
            }
        }
    }
}
