using System.Diagnostics;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace McggCompanion.Windows;

public sealed class WindowCaptureSession : IAsyncDisposable
{
    private const int BufferCount = 2;
    private readonly CancellationTokenSource cancellation = new();
    private readonly object disposalGate = new();
    private CapturedFrame? latestFrame;
    private Task? disposalTask;
    private bool cancellationDisposed;

    // Keep the item's creator dispatcher alive until this session has been disposed.
    public WindowCaptureSession(GraphicsCaptureItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        SourceName = item.DisplayName;
        Completion = Task.Run(() => CaptureAsync(item));
    }

    public string SourceName { get; }

    public Task Completion { get; }

    public CapturedFrame? TakeLatestFrame() => Interlocked.Exchange(ref latestFrame, null);

    public ValueTask DisposeAsync()
    {
        lock (disposalGate)
        {
            disposalTask ??= StopAsync();
            return new ValueTask(disposalTask);
        }
    }

    private async Task CaptureAsync(GraphicsCaptureItem item)
    {
        var token = cancellation.Token;
        item.Closed += OnSourceClosed;

        try
        {
            token.ThrowIfCancellationRequested();
            var poolSize = item.Size;
            if (poolSize.Width <= 0 || poolSize.Height <= 0)
            {
                throw new InvalidOperationException("Sumber capture tidak memiliki ukuran gambar yang valid.");
            }

            using var device = D3D11Devices.Create();
            using var pool = Direct3D11CaptureFramePool.CreateFreeThreaded(
                device, DirectXPixelFormat.B8G8R8A8UIntNormalized, BufferCount, poolSize);
            using var session = pool.CreateCaptureSession(item);
            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(1000.0 / 30));
            session.StartCapture();

            long sequence = 0;
            var resizeCount = 0;

            while (await timer.WaitForNextTickAsync(token).ConfigureAwait(false))
            {
                var newSize = poolSize;
                using (var frame = TakeNewestFrame(pool))
                {
                    if (frame is null)
                    {
                        continue;
                    }

                    var receivedAt = Stopwatch.GetTimestamp();
                    var contentSize = frame.ContentSize;
                    if (contentSize.Width <= 0 || contentSize.Height <= 0)
                    {
                        continue;
                    }

                    newSize = contentSize;
                    if (contentSize.Width == poolSize.Width && contentSize.Height == poolSize.Height)
                    {
                        var copyStarted = Stopwatch.GetTimestamp();
                        var systemRelativeTime = frame.SystemRelativeTime;

                        // Keep the frame alive until its GPU surface has been deep-copied.
                        using var bitmap = await SoftwareBitmap.CreateCopyFromSurfaceAsync(
                            frame.Surface, BitmapAlphaMode.Premultiplied);
                        if (bitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 ||
                            bitmap.BitmapAlphaMode != BitmapAlphaMode.Premultiplied ||
                            bitmap.PixelWidth != contentSize.Width || bitmap.PixelHeight != contentSize.Height)
                        {
                            throw new InvalidOperationException("Format atau ukuran gambar capture tidak sesuai BGRA yang diharapkan.");
                        }

                        var stride = checked(contentSize.Width * 4);
                        var pixels = new byte[checked(stride * contentSize.Height)];
                        var buffer = new global::Windows.Storage.Streams.Buffer((uint)pixels.Length);
                        bitmap.CopyToBuffer(buffer);
                        using var reader = DataReader.FromBuffer(buffer);
                        reader.ReadBytes(pixels);

                        if (token.IsCancellationRequested)
                        {
                            break;
                        }

                        var captured = new CapturedFrame(
                            contentSize.Width, contentSize.Height, stride, pixels,
                            systemRelativeTime, receivedAt, ++sequence, resizeCount,
                            Stopwatch.GetElapsedTime(copyStarted).TotalMilliseconds);
                        Interlocked.Exchange(ref latestFrame, captured);
                    }
                }

                // Recreate only after returning the old frame to its pool.
                if (newSize.Width != poolSize.Width || newSize.Height != poolSize.Height)
                {
                    token.ThrowIfCancellationRequested();
                    pool.Recreate(device, DirectXPixelFormat.B8G8R8A8UIntNormalized, BufferCount, newSize);
                    poolSize = newSize;
                    resizeCount++;
                }
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
        }
        finally
        {
            item.Closed -= OnSourceClosed;
            Interlocked.Exchange(ref latestFrame, null);
        }
    }

    private static Direct3D11CaptureFrame? TakeNewestFrame(Direct3D11CaptureFramePool pool)
    {
        Direct3D11CaptureFrame? latest = null;
        try
        {
            for (var index = 0; index < BufferCount; index++)
            {
                var candidate = pool.TryGetNextFrame();
                if (candidate is null)
                {
                    break;
                }

                latest?.Dispose();
                latest = candidate;
            }

            return latest;
        }
        catch
        {
            latest?.Dispose();
            throw;
        }
    }

    private void OnSourceClosed(GraphicsCaptureItem sender, object args) => RequestStop();

    private void RequestStop()
    {
        lock (disposalGate)
        {
            if (!cancellationDisposed)
            {
                cancellation.Cancel();
            }
        }
    }

    private async Task StopAsync()
    {
        RequestStop();
        try
        {
            await Completion.ConfigureAwait(false);
        }
        finally
        {
            lock (disposalGate)
            {
                cancellation.Dispose();
                cancellationDisposed = true;
            }
        }
    }
}
