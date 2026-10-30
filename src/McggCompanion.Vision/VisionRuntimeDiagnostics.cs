using OpenCvSharp;

namespace McggCompanion.Vision;

public static class VisionRuntimeDiagnostics
{
    public static VisionRuntimeCheckResult Check()
    {
        var version = Cv2.GetVersionString();
        if (string.IsNullOrWhiteSpace(version))
        {
            throw new InvalidOperationException("OpenCV did not report a runtime version.");
        }

        var expectedLocation = new Point(19, 13);
        const int templateRows = 9;
        const int templateColumns = 7;

        using var image = new Mat(48, 64, MatType.CV_8UC1, Scalar.All(0));
        using var template = new Mat(templateRows, templateColumns, MatType.CV_8UC1);
        for (var row = 0; row < templateRows; row++)
        {
            for (var column = 0; column < templateColumns; column++)
            {
                template.Set(row, column, (byte)(40 + ((row * 37 + column * 29 + row * column * 11) % 200)));
            }
        }

        using var patch = new Mat(image, new Rect(expectedLocation.X, expectedLocation.Y, templateColumns, templateRows));
        template.CopyTo(patch);

        using var matches = new Mat();
        Cv2.MatchTemplate(image, template, matches, TemplateMatchModes.SqDiffNormed);
        Cv2.MinMaxLoc(matches, out var minimumDifference, out _, out var actualLocation, out _);

        return new VisionRuntimeCheckResult(
            version,
            actualLocation == expectedLocation && double.IsFinite(minimumDifference) && minimumDifference <= 0.00001);
    }
}

public sealed record VisionRuntimeCheckResult(string OpenCvVersion, bool TemplateMatchPassed);
