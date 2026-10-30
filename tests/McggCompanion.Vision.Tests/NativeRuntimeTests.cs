using OpenCvSharp;

namespace McggCompanion.Vision.Tests;

public class NativeRuntimeTests
{
    [Fact]
    public void Check_LoadsNativeRuntimeAndFindsSyntheticTemplate()
    {
        var result = VisionRuntimeDiagnostics.Check();

        Assert.StartsWith("5.", result.OpenCvVersion);
        Assert.True(result.TemplateMatchPassed);
    }

    [Fact]
    public void PngRoundTrip_PreservesSyntheticPixels()
    {
        const int rows = 8;
        const int columns = 11;
        using var original = new Mat(rows, columns, MatType.CV_8UC1);
        for (var row = 0; row < rows; row++)
        {
            for (var column = 0; column < columns; column++)
            {
                original.Set(row, column, (byte)((row * 31 + column * 17) % 256));
            }
        }

        Assert.True(Cv2.ImEncode(".png", original, out var encoded));
        Assert.NotEmpty(encoded);

        using var decoded = Cv2.ImDecode(encoded, ImreadModes.Unchanged);
        Assert.False(decoded.Empty());
        Assert.Equal(original.Size(), decoded.Size());
        Assert.Equal(original.Type(), decoded.Type());
        Assert.Equal(0, Cv2.Norm(original, decoded, NormTypes.INF));
    }
}
