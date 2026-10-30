using System.Runtime.InteropServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using McggCompanion.Vision;
using Windows.Graphics.Capture;

namespace McggCompanion.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string status = "Siap memeriksa lingkungan pengembangan.";

    [ObservableProperty]
    private string openCvVersion = "Belum diperiksa";

    [ObservableProperty]
    private string captureSupport = "Belum diperiksa";

    public string FrameworkVersion => RuntimeInformation.FrameworkDescription;

    public string SystemDescription => $"{RuntimeInformation.OSDescription} · {RuntimeInformation.OSArchitecture}";

    [RelayCommand]
    private async Task CheckEnvironmentAsync()
    {
        Status = "Memeriksa dependensi…";

        try
        {
            var result = await Task.Run(VisionRuntimeDiagnostics.Check);
            OpenCvVersion = result.OpenCvVersion;
            var captureAvailable = GraphicsCaptureSession.IsSupported();
            CaptureSupport = captureAvailable ? "API tersedia; capture game belum diuji" : "API capture tidak tersedia";
            Status = result.TemplateMatchPassed
                ? "OpenCV siap dan uji gambar sintetis lulus. Integrasi MCGG menjadi tahap berikutnya."
                : "Uji gambar sintetis gagal. Periksa dependensi OpenCV sebelum melanjutkan.";
        }
        catch (Exception exception)
        {
            Status = $"Pemeriksaan gagal: {exception.Message}";
        }
    }
}
