using System.Text.Json;
using OpenCvSharp;

namespace McggCompanion.Vision.Tests;

public class CalibrationTests
{
    [Fact]
    public void Drag_ReversedAndScaled_MapsToImagePixels()
    {
        var region = PixelRegion.FromDrag(75, 40, 10, 5, 100, 50, 1000, 500);
        Assert.Equal(new PixelRegion(100, 50, 650, 350), region);
        Assert.Equal(new PixelRegion(0, 0, 1000, 500), PixelRegion.FromDrag(-10, -5, 110, 55, 100, 50, 1000, 500));
        Assert.Throws<ArgumentException>(() => PixelRegion.FromDrag(1, 1, 1, 1, 100, 50, 1000, 500));
        Assert.Throws<ArgumentException>(() => PixelRegion.FromDrag(1.1, 2.1, 1.1, 2.1, 100, 50, 1000, 500));
        Assert.Throws<ArgumentException>(() => PixelRegion.FromDrag(double.NaN, 0, 1, 1, 100, 50, 1000, 500));
        Assert.Throws<ArgumentException>(() => new PixelRegion(int.MaxValue, 0, 3, 1).Validate(1000, 500));
    }

    [Fact]
    public void Labels_KeepZeroSeparateFromUnknownAndRejectInvalidAreas()
    {
        var session = Session();
        var sample = Sample();
        sample.Validate(session);
        var restored = JsonSerializer.Deserialize<CalibrationSample>(JsonSerializer.Serialize(sample))!;
        Assert.Null(restored.Gold);
        Assert.Equal(0, restored.Slots[0].Price);
        Assert.Null(restored.Slots[1].Price);
        Assert.Throws<ArgumentException>(() => (sample with { Gold = -1 }).Validate(session));
        Assert.Throws<ArgumentException>(() => (sample with { Regions = new() { ["Shop"] = new(9, 0, 2, 1) } }).Validate(session));
        Assert.Throws<ArgumentException>(() => (session with { Id = "../escape" }).Validate());
        Assert.Throws<ArgumentException>(() => (sample with { Slots = [sample.Slots[0]] }).Validate(session));
        var invalid = sample.Slots.ToArray();
        invalid[1] = new SlotAnnotation(2, "Uranus", "Tidak terbaca", 0);
        Assert.Throws<ArgumentException>(() => (sample with { Slots = invalid }).Validate(session));
        invalid[0] = invalid[0] with { Price = -1 };
        invalid[1] = sample.Slots[1];
        Assert.Throws<ArgumentException>(() => (sample with { Slots = invalid }).Validate(session));
    }

    [Fact]
    public void Store_WritesCropsAndLocksSessionSplit_WithoutDuplicateImages()
    {
        var root = Path.Combine(Path.GetTempPath(), $"mcgg-calibration-test-{Guid.NewGuid():N}");
        try
        {
            using var image = new Mat(8, 10, MatType.CV_8UC3, new Scalar(10, 30, 50));
            Assert.True(Cv2.ImEncode(".png", image, out var png));
            var path = CalibrationStore.Save(root, Session(), Sample(), png);
            using var metadata = JsonDocument.Parse(File.ReadAllText(Path.Combine(path, "sample.json")));
            var cropFile = metadata.RootElement.GetProperty("RegionFiles").GetProperty("Hero 1").GetString()!;
            using var crop = Cv2.ImRead(Path.Combine(path, cropFile));
            Assert.Equal(new Size(3, 2), crop.Size());
            Assert.Equal(new Vec3b(10, 30, 50), crop.At<Vec3b>(0, 0));
            Assert.Equal("Pengembangan", metadata.RootElement.GetProperty("Split").GetString());
            Assert.Contains("Gold", metadata.RootElement.GetProperty("MissingRegions").EnumerateArray().Select(value => value.GetString()));
            Assert.Throws<InvalidOperationException>(() => CalibrationStore.Save(root, Session() with { Split = "Uji akhir" }, Sample(), png));
            Assert.Throws<InvalidOperationException>(() => CalibrationStore.Save(root, Session() with { Id = "another-session", Split = "Uji akhir" }, Sample(), png));
            Assert.False(Directory.Exists(Path.Combine(root, "another-session")));
            Assert.Single(Directory.EnumerateFiles(root, "sample.json", SearchOption.AllDirectories));
            Assert.Throws<ArgumentException>(() => CalibrationStore.Save(root, Session() with { PixelWidth = 11 }, Sample(), png));
            var revised = Sample() with { Gold = 0, Regions = new() { ["Hero 1"] = new(0, 0, 1, 1) } };
            Assert.Equal(path, CalibrationStore.Save(root, Session(), revised, png));
            using var updated = JsonDocument.Parse(File.ReadAllText(Path.Combine(path, "sample.json")));
            Assert.Equal(0, updated.RootElement.GetProperty("Annotation").GetProperty("Gold").GetInt32());
            using var updatedCrop = Cv2.ImRead(Path.Combine(path, updated.RootElement.GetProperty("RegionFiles").GetProperty("Hero 1").GetString()!));
            Assert.Equal(new Size(1, 1), updatedCrop.Size());
            Assert.Single(Directory.EnumerateFiles(root, "sample.json", SearchOption.AllDirectories));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    private static CalibrationSession Session() => new("test-session", "Pengembangan", "Uranus", "Belum dicatat",
        "Indonesia", "Jendela", "100%", 10, 8);

    private static CalibrationSample Sample() => new(DateTimeOffset.UtcNow, "Impor PNG", null, null, null,
        "Stabil", null, "Belum diketahui", "Indikator hasil belum dipetakan",
        new() { ["Shop"] = new(0, 0, 10, 8), ["Hero 1"] = new(2, 3, 3, 2) },
        [new(1, "Uranus", "Terbaca", 0), new(2, "", "Tidak terbaca", null),
         new(3, "", "Belum dilabeli", null), new(4, "", "Kosong/terjual", null), new(5, "", "Belum dilabeli", null)]);
}
