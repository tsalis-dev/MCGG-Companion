using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using McggCompanion.Vision;
using McggCompanion.Windows;
using Windows.Graphics.Capture;

namespace McggCompanion.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject, IAsyncDisposable
{
    private readonly nint ownerHandle;
    private readonly DispatcherTimer previewTimer;
    private global::Windows.System.DispatcherQueueController? ownedDispatcherQueue;
    private WindowCaptureSession? capture;
    private CapturedFrame? lastFrame;
    private DateTimeOffset startedAtUtc;
    private long startedAtTimestamp;
    private bool disposed;

    public MainWindowViewModel(nint ownerHandle)
    {
        this.ownerHandle = ownerHandle;
        previewTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        previewTimer.Tick += OnPreviewTick;
    }

    [ObservableProperty]
    private string status = "Pilih jendela MCGG untuk memulai pratinjau.";

    [ObservableProperty]
    private string sourceName = "Belum dipilih";

    [ObservableProperty]
    private WriteableBitmap? preview;

    [ObservableProperty]
    private string previewHint = "Gambar dari jendela yang dipilih akan tampil di sini.";

    [ObservableProperty]
    private string frameDimensions = "Belum ada gambar";

    [ObservableProperty]
    private string frameFreshness = "";

    [ObservableProperty]
    private string captureDetails = "Belum ada sesi capture.";

    [ObservableProperty]
    private string gameVersion = "";

    [ObservableProperty]
    private string displayMode = "Belum dicatat";

    [ObservableProperty]
    private string profileStatus = "Profil menyimpan metadata pengujian lokal, tanpa gambar.";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SelectSourceCommand), nameof(StopCaptureCommand), nameof(SaveProfileCommand))]
    private bool isBusy;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StopCaptureCommand), nameof(SaveProfileCommand))]
    private bool isCapturing;

    [ObservableProperty]
    private string diagnosticsStatus = "Pemeriksaan dependensi belum dijalankan.";

    [ObservableProperty]
    private string openCvVersion = "Belum diperiksa";

    [ObservableProperty]
    private string captureSupport = "Belum diperiksa";

    public string FrameworkVersion => RuntimeInformation.FrameworkDescription;

    public string SystemDescription => $"{RuntimeInformation.OSDescription} · {RuntimeInformation.OSArchitecture}";

    public string[] DisplayModes { get; } = ["Belum dicatat", "Jendela", "Tanpa bingkai", "Layar penuh"];

    private bool CanSelectSource() => !disposed && !IsBusy;

    private bool CanStopCapture() => !disposed && !IsBusy && IsCapturing;

    private bool CanSaveProfile() => !disposed && !IsBusy && IsCapturing && lastFrame is not null;

    [RelayCommand(CanExecute = nameof(CanSelectSource))]
    private async Task SelectSourceAsync()
    {
        IsBusy = true;
        try
        {
            if (!GraphicsCaptureSession.IsSupported())
            {
                Status = "Capture belum tersedia pada lingkungan Windows ini.";
                return;
            }

            ownedDispatcherQueue ??= CaptureDispatcherQueue.CreateForCurrentThreadIfNeeded();
            var picker = new GraphicsCapturePicker();
            WinRT.Interop.InitializeWithWindow.Initialize(picker, ownerHandle);
            var item = await picker.PickSingleItemAsync();
            if (disposed)
            {
                return;
            }

            if (item is null)
            {
                Status = IsCapturing ? "Pilihan dibatalkan. Pratinjau sebelumnya tetap berjalan." : "Pemilihan jendela dibatalkan.";
                return;
            }

            await StopCaptureCoreAsync(null);
            if (disposed)
            {
                return;
            }

            var session = new WindowCaptureSession(item);
            capture = session;
            SourceName = session.SourceName;
            startedAtUtc = DateTimeOffset.UtcNow;
            startedAtTimestamp = Stopwatch.GetTimestamp();
            IsCapturing = true;
            Status = "Mengambil gambar dari sumber yang dipilih…";
            PreviewHint = "Menunggu gambar pertama…";
            previewTimer.Start();
            _ = ObserveCompletionAsync(session);
        }
        catch (Exception exception)
        {
            Status = $"Belum dapat memulai capture: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanStopCapture))]
    private async Task StopCaptureAsync()
    {
        IsBusy = true;
        try
        {
            await StopCaptureCoreAsync("Capture dihentikan.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ObserveCompletionAsync(WindowCaptureSession session)
    {
        var message = "Sumber capture ditutup. Pilih jendela lagi untuk melanjutkan.";
        try
        {
            await session.Completion;
        }
        catch (Exception exception)
        {
            message = $"Capture berhenti: {exception.Message}";
        }

        if (!disposed && ReferenceEquals(capture, session))
        {
            await StopCaptureCoreAsync(message);
        }
    }

    private async Task StopCaptureCoreAsync(string? message)
    {
        var previous = capture;
        capture = null;
        previewTimer.Stop();
        IsCapturing = false;
        lastFrame = null;
        Preview = null;
        PreviewHint = "Pilih jendela untuk menampilkan gambar baru.";
        FrameDimensions = "Belum ada gambar";
        FrameFreshness = "";
        SaveProfileCommand.NotifyCanExecuteChanged();

        if (previous is not null)
        {
            try
            {
                await previous.DisposeAsync();
            }
            catch (Exception exception)
            {
                message = $"Capture sudah dihentikan dengan kendala: {exception.Message}";
            }
        }

        if (message is not null && !disposed && capture is null)
        {
            Status = message;
        }
    }

    private void OnPreviewTick(object? sender, EventArgs args)
    {
        if (capture is null || disposed)
        {
            return;
        }

        try
        {
            var frame = capture.TakeLatestFrame();
            if (frame is not null)
            {
                var firstFrame = lastFrame is null;
                lastFrame = frame;
                if (Preview is null || Preview.PixelWidth != frame.Width || Preview.PixelHeight != frame.Height)
                {
                    Preview = new WriteableBitmap(frame.Width, frame.Height, 96, 96, PixelFormats.Pbgra32, null);
                }

                Preview.WritePixels(new Int32Rect(0, 0, frame.Width, frame.Height), frame.BgraPixels, frame.Stride, 0);
                PreviewHint = "";
                FrameDimensions = $"{frame.Width} × {frame.Height} piksel";
                Status = "Pratinjau berjalan.";
                var seconds = Math.Max(Stopwatch.GetElapsedTime(startedAtTimestamp).TotalSeconds, 0.001);
                CaptureDetails = $"Frame tersalin: {frame.SequenceNumber} · Rata-rata: {frame.SequenceNumber / seconds:F1} fps\n" +
                                 $"Perubahan ukuran: {frame.ResizeCount} · Salinan terakhir: {frame.CopyMilliseconds:F1} ms\n" +
                                 $"Timestamp compositor (QPC): {frame.SystemRelativeTime.TotalMilliseconds:F1} ms";
                if (firstFrame)
                {
                    SaveProfileCommand.NotifyCanExecuteChanged();
                }
            }

            if (lastFrame is not null)
            {
                var age = Stopwatch.GetElapsedTime(lastFrame.ReceivedStopwatchTimestamp).TotalSeconds;
                FrameFreshness = $"Gambar diterima {age:F1} detik lalu";
                if (age > 2)
                {
                    Status = "Menunggu gambar baru. Sumber mungkin diam atau diminimalkan.";
                }
            }
        }
        catch (Exception exception)
        {
            _ = StopCaptureCoreAsync($"Pratinjau dihentikan: {exception.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(CanSaveProfile))]
    private async Task SaveProfileAsync()
    {
        var frame = lastFrame;
        if (frame is null)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MCGG-Companion", "profiles");
            Directory.CreateDirectory(folder);
            var path = Path.Combine(folder, $"capture-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmssfff}-{Guid.NewGuid():N}.json");
            var profile = new
            {
                SchemaVersion = 1,
                SavedAtUtc = DateTimeOffset.UtcNow,
                SessionStartedAtUtc = startedAtUtc,
                SourceName,
                GameVersion = string.IsNullOrWhiteSpace(GameVersion) ? "Belum dicatat" : GameVersion.Trim(),
                DisplayMode,
                SystemDescription,
                FrameworkVersion,
                PixelWidth = frame.Width,
                PixelHeight = frame.Height,
                PixelFormat = "BGRA8 premultiplied",
                frame.SequenceNumber,
                frame.ResizeCount,
                frame.CopyMilliseconds,
                CompositorTimeMilliseconds = frame.SystemRelativeTime.TotalMilliseconds,
                ReceivedStopwatchTimestamp = frame.ReceivedStopwatchTimestamp,
                StopwatchFrequency = Stopwatch.Frequency,
                FrameAgeMilliseconds = Stopwatch.GetElapsedTime(frame.ReceivedStopwatchTimestamp).TotalMilliseconds,
                CaptureValidatedForGame = false
            };
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true }));
            ProfileStatus = $"Profil tersimpan: {path}";
        }
        catch (Exception exception)
        {
            ProfileStatus = $"Profil belum tersimpan: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CheckEnvironmentAsync()
    {
        DiagnosticsStatus = "Memeriksa dependensi…";

        try
        {
            var result = await Task.Run(VisionRuntimeDiagnostics.Check);
            OpenCvVersion = result.OpenCvVersion;
            var captureAvailable = GraphicsCaptureSession.IsSupported();
            CaptureSupport = captureAvailable ? "API tersedia; capture game belum diuji" : "API capture tidak tersedia";
            DiagnosticsStatus = result.TemplateMatchPassed
                ? "OpenCV siap dan uji gambar sintetis lulus."
                : "Uji gambar sintetis gagal. Periksa dependensi OpenCV sebelum melanjutkan.";
        }
        catch (Exception exception)
        {
            DiagnosticsStatus = $"Pemeriksaan gagal: {exception.Message}";
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        previewTimer.Tick -= OnPreviewTick;
        await StopCaptureCoreAsync(null);
        if (ownedDispatcherQueue is { } controller)
        {
            ownedDispatcherQueue = null;
            await controller.ShutdownQueueAsync();
        }
    }
}
