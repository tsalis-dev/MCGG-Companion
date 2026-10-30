using System.Text.RegularExpressions;

namespace McggCompanion.Vision;

public sealed record PixelRegion(int X, int Y, int Width, int Height)
{
    public void Validate(int imageWidth, int imageHeight)
    {
        if (X < 0 || Y < 0 || Width <= 0 || Height <= 0 ||
            (long)X + Width > imageWidth || (long)Y + Height > imageHeight)
        {
            throw new ArgumentException("Area harus berada di dalam gambar dan memiliki ukuran positif.");
        }
    }

    public static PixelRegion FromDrag(double x1, double y1, double x2, double y2,
        double viewWidth, double viewHeight, int imageWidth, int imageHeight)
    {
        if (!double.IsFinite(x1) || !double.IsFinite(y1) || !double.IsFinite(x2) || !double.IsFinite(y2) ||
            !double.IsFinite(viewWidth) || !double.IsFinite(viewHeight) || viewWidth <= 0 || viewHeight <= 0 ||
            imageWidth <= 0 || imageHeight <= 0 || x1 == x2 || y1 == y2)
        {
            throw new ArgumentException("Ukuran gambar atau posisi pilihan tidak valid.");
        }

        var left = (int)Math.Floor(Math.Clamp(Math.Min(x1, x2) / viewWidth, 0, 1) * imageWidth);
        var top = (int)Math.Floor(Math.Clamp(Math.Min(y1, y2) / viewHeight, 0, 1) * imageHeight);
        var right = (int)Math.Ceiling(Math.Clamp(Math.Max(x1, x2) / viewWidth, 0, 1) * imageWidth);
        var bottom = (int)Math.Ceiling(Math.Clamp(Math.Max(y1, y2) / viewHeight, 0, 1) * imageHeight);
        var region = new PixelRegion(left, top, right - left, bottom - top);
        region.Validate(imageWidth, imageHeight);
        return region;
    }
}

public sealed record CalibrationSession(string Id, string Split, string TargetHero,
    string GameVersion, string GameLanguage, string DisplayMode, string WindowsScale,
    int PixelWidth, int PixelHeight)
{
    public void Validate()
    {
        if (Id is null || !Regex.IsMatch(Id, "\\A[a-zA-Z0-9_-]{1,80}\\z"))
        {
            throw new ArgumentException("ID sesi harus berisi 1–80 huruf, angka, tanda minus, atau garis bawah.");
        }

        if (Split is not ("Pengembangan" or "Penyesuaian" or "Uji akhir") ||
            string.IsNullOrWhiteSpace(TargetHero) || string.IsNullOrWhiteSpace(GameVersion) ||
            string.IsNullOrWhiteSpace(GameLanguage) || string.IsNullOrWhiteSpace(DisplayMode) ||
            string.IsNullOrWhiteSpace(WindowsScale) || PixelWidth <= 0 || PixelHeight <= 0)
        {
            throw new ArgumentException("Lengkapi profil sesi dan kelompok data. Nilai yang belum diketahui dapat ditulis 'Belum dicatat'.");
        }
    }
}

public sealed record SlotAnnotation(int Slot, string HeroName, string State, int? Price);

public sealed record CalibrationSample(DateTimeOffset AnnotatedAtUtc, string Origin,
    DateTimeOffset? ObservationReceivedAtUtc, long? FrameSequence, double? FrameAgeMilliseconds,
    string ShopState, int? Gold, string Capacity, string OutcomeNotes,
    Dictionary<string, PixelRegion> Regions, SlotAnnotation[] Slots)
{
    public static string[] RegionNames { get; } =
    ["Shop", "Gold", "Kapasitas", "Indikator hasil",
     "Hero 1", "Harga 1", "Hero 2", "Harga 2", "Hero 3", "Harga 3", "Hero 4", "Harga 4", "Hero 5", "Harga 5"];

    public void Validate(CalibrationSession session)
    {
        session.Validate();
        if (Gold < 0 || FrameAgeMilliseconds is < 0 ||
            (FrameAgeMilliseconds.HasValue && !double.IsFinite(FrameAgeMilliseconds.Value)) ||
            ShopState is not ("Stabil" or "Transisi" or "Tertutup" or "Tidak terbaca") ||
            Capacity is not ("Belum diketahui" or "Tersedia" or "Penuh" or "Penggabungan") ||
            Origin is not ("Capture" or "Impor PNG"))
        {
            throw new ArgumentException("Label keadaan atau angka tidak valid.");
        }

        if (Slots.Length != 5 || Slots.Select(slot => slot.Slot).Order().SequenceEqual(new[] { 1, 2, 3, 4, 5 }) is false)
        {
            throw new ArgumentException("Label harus memuat tepat lima slot berbeda (1–5).");
        }

        foreach (var slot in Slots)
        {
            if (slot.Price < 0 || slot.State is not ("Belum dilabeli" or "Terbaca" or "Kosong/terjual" or "Tidak terbaca") ||
                (slot.State == "Terbaca" && string.IsNullOrWhiteSpace(slot.HeroName)) ||
                (slot.State != "Terbaca" && (!string.IsNullOrWhiteSpace(slot.HeroName) || slot.Price.HasValue)))
            {
                throw new ArgumentException($"Periksa label slot {slot.Slot}. Nama dan harga hanya diisi untuk slot terbaca; harga kosong berarti belum terbaca.");
            }
        }

        foreach (var (name, region) in Regions)
        {
            if (!RegionNames.Contains(name))
            {
                throw new ArgumentException("Nama area tidak dikenal.");
            }
            region.Validate(session.PixelWidth, session.PixelHeight);
        }
    }
}
