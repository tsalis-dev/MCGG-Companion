using System.Security.Cryptography;
using System.Text.Json;
using OpenCvSharp;

namespace McggCompanion.Vision;

public static class CalibrationStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static string Save(string root, CalibrationSession session, CalibrationSample sample, byte[] png)
    {
        sample.Validate(session);
        using var image = Cv2.ImDecode(png, ImreadModes.Unchanged);
        if (image.Empty() || image.Width != session.PixelWidth || image.Height != session.PixelHeight)
        {
            throw new ArgumentException("Ukuran PNG tidak sesuai dengan profil kalibrasi.");
        }

        Directory.CreateDirectory(root);
        // A single writer also prevents two app windows assigning the same session to different splits.
        using var writerLock = new FileStream(Path.Combine(root, ".writer.lock"), FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
        var sessionFolder = Path.Combine(root, session.Id);
        var manifestPath = Path.Combine(sessionFolder, "session.json");
        if (File.Exists(manifestPath))
        {
            var existing = JsonSerializer.Deserialize<CalibrationSession>(File.ReadAllText(manifestPath));
            if (existing != session)
            {
                throw new InvalidOperationException("Profil atau kelompok sesi berbeda dari data tersimpan. Gunakan profil sesi semula; buat ID baru hanya untuk sesi permainan berbeda.");
            }
        }

        var digest = Convert.ToHexString(SHA256.HashData(png));
        string? existingSampleFolder = null;
        // Identical source images cannot silently enter another split through a renamed session.
        foreach (var metadataPath in Directory.EnumerateFiles(root, "sample.json", SearchOption.AllDirectories))
        {
            using var metadata = JsonDocument.Parse(File.ReadAllText(metadataPath));
            if (metadata.RootElement.GetProperty("ImageSha256").GetString() == digest)
            {
                if (metadata.RootElement.GetProperty("SessionId").GetString() != session.Id)
                {
                    throw new InvalidOperationException("Gambar ini sudah tersimpan pada sesi lain. Jangan menggandakannya pada sesi atau kelompok lain.");
                }
                existingSampleFolder = Path.GetDirectoryName(metadataPath);
            }
        }

        Directory.CreateDirectory(sessionFolder);
        if (!File.Exists(manifestPath))
        {
            using var manifest = new FileStream(manifestPath, FileMode.CreateNew, FileAccess.Write);
            JsonSerializer.Serialize(manifest, session, JsonOptions);
        }

        var sampleFolder = existingSampleFolder ?? Path.Combine(sessionFolder, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(sampleFolder);
        var revisionId = Guid.NewGuid().ToString("N");
        var cropFolder = Path.Combine(sampleFolder, revisionId);
        Directory.CreateDirectory(cropFolder);
        var pendingMetadata = Path.Combine(sampleFolder, $"{revisionId}.json.tmp");
        try
        {
            if (existingSampleFolder is null)
            {
                File.WriteAllBytes(Path.Combine(sampleFolder, "frame.png"), png);
            }
            var regionFiles = new Dictionary<string, string>();
            foreach (var (name, region) in sample.Regions)
            {
                var fileName = $"{name.Replace(' ', '-')}.png";
                using var crop = new Mat(image, new Rect(region.X, region.Y, region.Width, region.Height));
                if (!Cv2.ImWrite(Path.Combine(cropFolder, fileName), crop))
                {
                    throw new IOException("Potongan area belum dapat disimpan.");
                }
                regionFiles.Add(name, $"{revisionId}/{fileName}");
            }

            var metadata = new
            {
                SchemaVersion = 1,
                SessionId = session.Id,
                session.Split,
                ImageSha256 = digest,
                ImageFile = "frame.png",
                RegionFiles = regionFiles,
                MissingRegions = CalibrationSample.RegionNames.Except(sample.Regions.Keys).ToArray(),
                ValidationStatus = "Anotasi manual; belum tervalidasi untuk deteksi atau pembelian",
                Annotation = sample
            };
            File.WriteAllText(pendingMetadata, JsonSerializer.Serialize(metadata, JsonOptions));
            File.Move(pendingMetadata, Path.Combine(sampleFolder, "sample.json"), overwrite: true);
            return sampleFolder;
        }
        catch
        {
            File.Delete(pendingMetadata);
            Directory.Delete(existingSampleFolder is null ? sampleFolder : cropFolder, recursive: true);
            throw;
        }
    }
}
