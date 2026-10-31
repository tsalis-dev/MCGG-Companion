using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Rectangle = System.Windows.Shapes.Rectangle;
using McggCompanion.Vision;
using Microsoft.Win32;

namespace McggCompanion.App;

public partial class CalibrationWindow : Window
{
    private readonly Dictionary<string, PixelRegion> regions = [];
    private readonly SlotLabelInput[] slots = Enumerable.Range(1, 5).Select(slot => new SlotLabelInput(slot)).ToArray();
    private BitmapSource? image;
    private Point? dragStart;
    private Rectangle? dragOutline;
    private string origin = "Impor PNG";
    private DateTimeOffset? receivedAtUtc;
    private long? frameSequence;
    private double? frameAgeMilliseconds;
    private bool saving;
    private bool dirty;

    public CalibrationWindow(string sessionId, string gameVersion, string displayMode)
    {
        InitializeComponent();
        SessionId.Text = sessionId;
        GameVersion.Text = string.IsNullOrWhiteSpace(gameVersion) ? "Belum dicatat" : gameVersion;
        DisplayMode.Text = displayMode;
        RegionChoice.ItemsSource = CalibrationSample.RegionNames;
        RegionChoice.SelectedIndex = 0;
        SplitChoice.ItemsSource = new[] { "Pengembangan", "Penyesuaian", "Uji akhir" };
        SplitChoice.SelectedIndex = 0;
        ShopState.ItemsSource = new[] { "Tidak terbaca", "Stabil", "Transisi", "Tertutup" };
        ShopState.SelectedIndex = 0;
        Capacity.ItemsSource = new[] { "Belum diketahui", "Tersedia", "Penuh", "Penggabungan" };
        Capacity.SelectedIndex = 0;
        SlotLabels.ItemsSource = slots;
        // Text and selection edits must also protect a partially annotated image from replacement.
        AddHandler(TextBox.TextChangedEvent, new TextChangedEventHandler((_, _) => dirty = true));
        AddHandler(ComboBox.SelectionChangedEvent, new SelectionChangedEventHandler((_, _) => dirty = true));
        Closing += (_, args) => args.Cancel = saving || !CanReplaceImage();
    }

    private bool CanReplaceImage() => !dirty || image is null || MessageBox.Show(this,
        "Anotasi gambar ini belum disimpan. Lanjutkan dan tinggalkan perubahan?", "Anotasi belum tersimpan",
        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

    public void SetImage(BitmapSource bitmap, string imageOrigin, DateTimeOffset? observationReceivedAtUtc = null,
        long? sequence = null, double? ageMilliseconds = null)
    {
        if (saving || !CanReplaceImage())
        {
            return;
        }

        var sameSize = image?.PixelWidth == bitmap.PixelWidth && image.PixelHeight == bitmap.PixelHeight;
        if (!sameSize)
        {
            regions.Clear();
        }
        image = bitmap;
        origin = imageOrigin;
        receivedAtUtc = observationReceivedAtUtc;
        frameSequence = sequence;
        frameAgeMilliseconds = ageMilliseconds;
        FrozenImage.Source = bitmap;
        ImageSurface.Width = bitmap.PixelWidth;
        ImageSurface.Height = bitmap.PixelHeight;
        Gold.Clear();
        OutcomeNotes.Clear();
        ShopState.SelectedIndex = 0;
        Capacity.SelectedIndex = 0;
        foreach (var slot in slots)
        {
            slot.HeroName = "";
            slot.Price = "";
            slot.State = "Belum dilabeli";
        }
        SlotLabels.Items.Refresh();
        SaveButton.IsEnabled = true;
        dirty = true;
        DrawRegions();
        SaveStatus.Text = $"Gambar tetap {bitmap.PixelWidth} × {bitmap.PixelHeight}. " +
                          (sameSize ? "Kotak sebelumnya dipertahankan; periksa ulang letaknya. Label gambar dikosongkan." : "Tandai area pada gambar ini.");
    }

    private void OnOpenImage(object sender, RoutedEventArgs args) => OpenImage();

    public void OpenImage()
    {
        if (saving)
        {
            return;
        }
        var dialog = new OpenFileDialog { Filter = "Gambar PNG (*.png)|*.png", Title = "Buka gambar shop untuk kalibrasi" };
        if (dialog.ShowDialog(this) != true)
        {
            return;
        }
        try
        {
            using var file = File.OpenRead(dialog.FileName);
            var bitmap = BitmapFrame.Create(file, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            bitmap.Freeze();
            SetImage(bitmap, "Impor PNG");
        }
        catch (Exception exception)
        {
            SaveStatus.Text = $"Gambar belum dapat dibuka: {exception.Message}";
        }
    }

    private void OnDragStart(object sender, MouseButtonEventArgs args)
    {
        if (image is null || saving)
        {
            return;
        }
        dragStart = args.GetPosition(RegionCanvas);
        RegionCanvas.CaptureMouse();
        dragOutline = new Rectangle { Stroke = Brushes.Yellow, StrokeThickness = 3, IsHitTestVisible = false };
        RegionCanvas.Children.Add(dragOutline);
        args.Handled = true;
    }

    private void OnDragMove(object sender, MouseEventArgs args)
    {
        if (dragStart is not { } start || dragOutline is null)
        {
            return;
        }
        var end = args.GetPosition(RegionCanvas);
        Canvas.SetLeft(dragOutline, Math.Min(start.X, end.X));
        Canvas.SetTop(dragOutline, Math.Min(start.Y, end.Y));
        dragOutline.Width = Math.Abs(end.X - start.X);
        dragOutline.Height = Math.Abs(end.Y - start.Y);
    }

    private void OnDragEnd(object sender, MouseButtonEventArgs args)
    {
        if (dragStart is not { } start || image is null)
        {
            return;
        }
        var end = args.GetPosition(RegionCanvas);
        try
        {
            if (Math.Abs(start.X - end.X) >= 2 && Math.Abs(start.Y - end.Y) >= 2)
            {
                regions[(string)RegionChoice.SelectedItem] = PixelRegion.FromDrag(start.X, start.Y, end.X, end.Y,
                    image.PixelWidth, image.PixelHeight, image.PixelWidth, image.PixelHeight);
                dirty = true;
            }
        }
        catch (ArgumentException exception)
        {
            SaveStatus.Text = exception.Message;
        }
        finally
        {
            dragStart = null;
            dragOutline = null;
            RegionCanvas.ReleaseMouseCapture();
            DrawRegions();
        }
    }

    private void OnDragCancelled(object sender, MouseEventArgs args)
    {
        dragStart = null;
        dragOutline = null;
        DrawRegions();
    }

    private void OnRegionChanged(object sender, SelectionChangedEventArgs args)
    {
        if (RegionCanvas is not null)
        {
            DrawRegions();
        }
    }

    private void OnRemoveRegion(object sender, RoutedEventArgs args)
    {
        if (!saving && RegionChoice.SelectedItem is string name)
        {
            dirty |= regions.Remove(name);
            DrawRegions();
        }
    }

    private void DrawRegions()
    {
        RegionCanvas.Children.Clear();
        foreach (var (name, region) in regions)
        {
            var outline = new Rectangle
            {
                Width = region.Width, Height = region.Height, IsHitTestVisible = false,
                Stroke = name == (string?)RegionChoice.SelectedItem ? Brushes.Yellow : Brushes.DeepSkyBlue,
                StrokeThickness = 3
            };
            Canvas.SetLeft(outline, region.X);
            Canvas.SetTop(outline, region.Y);
            RegionCanvas.Children.Add(outline);
        }
        var selected = RegionChoice.SelectedItem as string;
        RegionSummary.Text = selected is not null && regions.TryGetValue(selected, out var area)
            ? $"{selected}: X={area.X}, Y={area.Y}, lebar={area.Width}, tinggi={area.Height} piksel. Area ditandai: {regions.Count}/{CalibrationSample.RegionNames.Length}."
            : $"Seret untuk menandai {selected}. Area ditandai: {regions.Count}/{CalibrationSample.RegionNames.Length}.";
    }

    private async void OnSave(object sender, RoutedEventArgs args)
    {
        if (image is null || saving)
        {
            return;
        }
        try
        {
            var session = new CalibrationSession(SessionId.Text.Trim(), (string)SplitChoice.SelectedItem,
                TargetHero.Text.Trim(), GameVersion.Text.Trim(), GameLanguage.Text.Trim(), DisplayMode.Text.Trim(),
                WindowsScale.Text.Trim(), image.PixelWidth, image.PixelHeight);
            var sample = new CalibrationSample(DateTimeOffset.UtcNow, origin, receivedAtUtc, frameSequence, frameAgeMilliseconds,
                (string)ShopState.SelectedItem, ParseNumber(Gold.Text, "Gold"), (string)Capacity.SelectedItem,
                OutcomeNotes.Text.Trim(), new Dictionary<string, PixelRegion>(regions),
                slots.Select(slot => new SlotAnnotation(slot.Slot, slot.HeroName.Trim(), slot.State, ParseNumber(slot.Price, $"Harga slot {slot.Slot}"))).ToArray());
            sample.Validate(session);
            var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MCGG-Companion", "datasets", "s02");
            saving = true;
            IsEnabled = false;
            SaveStatus.Text = "Menyimpan gambar dan anotasi lokal…";
            // Encode on the WPF thread; only owned bytes cross into the file/crop worker.
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            using var stream = new MemoryStream();
            encoder.Save(stream);
            var png = stream.ToArray();
            var folder = await Task.Run(() => CalibrationStore.Save(root, session, sample, png));
            dirty = false;
            SaveStatus.Text = $"Anotasi lokal tersimpan: {folder}. Belum merupakan validasi deteksi atau pembelian.";
        }
        catch (Exception exception)
        {
            SaveStatus.Text = $"Anotasi belum tersimpan: {exception.Message}";
        }
        finally
        {
            saving = false;
            IsEnabled = true;
        }
    }

    private static int? ParseNumber(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }
        if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var number))
        {
            throw new ArgumentException($"{field} harus berupa angka bulat ≥0, atau kosong jika belum terbaca.");
        }
        return number;
    }
}

public sealed class SlotLabelInput(int slot)
{
    public int Slot { get; } = slot;
    public string Title => $"Slot {Slot}";
    public string[] States { get; } = ["Belum dilabeli", "Terbaca", "Kosong/terjual", "Tidak terbaca"];
    public string State { get; set; } = "Belum dilabeli";
    public string HeroName { get; set; } = "";
    public string Price { get; set; } = "";
}
