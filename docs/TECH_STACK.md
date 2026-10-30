# Tech Stack — MCGG Companion

Tanggal keputusan: 30 Oktober 2026. Acuan kebutuhan: [PRD](PRD.md).

## 1. Keputusan utama

Dasar aplikasi adalah **C# + .NET 10 LTS + WPF**, dengan pemrosesan gambar melalui **OpenCV / OpenCvSharp** dan integrasi jendela serta input melalui API Windows. Pemrosesan, konfigurasi, dan catatan kejadian berjalan lokal.

Pilihan teknologi tidak berarti seluruh konfigurasi MCGG didukung. [S01](S01_CAPTURE.md) telah memvalidasi capture lobby pada satu konfigurasi Windows serta lifecycle pada fixture sintetis. Capture shop, koordinat tindakan, input, dan indikator pembelian masih mengikuti fase PRD.

| Komponen | Pilihan | Status | Tujuan / bukti sebelum dipakai |
| --- | --- | --- | --- |
| Bahasa dan runtime | C# pada .NET 10 LTS | Ditetapkan | Fondasi aplikasi dan logika transaksi; gunakan rilis stabil serta patch yang masih didukung |
| Antarmuka desktop | WPF + XAML | Ditetapkan | Pemilihan jendela, target, pratinjau, status, dan kontrol sesi |
| Pola antarmuka | MVVM dengan CommunityToolkit.Mvvm | Ditetapkan | Pisahkan tampilan, status UI, dan aturan pembelian agar dapat diuji |
| Pengambilan gambar | Windows.Graphics.Capture | S01: fixture dan satu profil lobby lulus | Capture shop serta variasi mode layar masih perlu diuji |
| Penghubung grafis | Interop WinRT / Direct3D 11 dan SoftwareBitmap | Diimplementasikan pada S01 | Membuat device; menyalin surface ke BGRA milik aplikasi; slot menyimpan frame terbaru |
| Pengenalan gambar | OpenCV melalui OpenCvSharp5 untuk Windows x64 | Ditetapkan; runtime dan gambar sintetis lulus uji S00 | Deteksi hero nyata dan kualitasnya tetap diuji pada S03 |
| Pembacaan gold dan harga | Pemrosesan gambar serta pencocokan digit | Baseline eksperimen | Evaluasi pada font, skala, dan variasi angka dari game asli |
| OCR umum | Kandidat tambahan jika baseline angka gagal | Opsional | Pilih mesin setelah membandingkan akurasi, latency, dan kebutuhan distribusinya |
| Pengiriman klik | Win32 SendInput melalui interop C# | Pilihan awal; perlu uji integrasi | Validasi koordinat, fokus, penerimaan input, dan hasil di game |
| Pintasan berhenti | Win32 RegisterHotKey | Pilihan awal; perlu uji konflik | Harus dapat menjeda saat game fokus; tampilkan konflik pendaftaran pintasan |
| Pengaturan dan profil | JSON dengan System.Text.Json | S01: profil metadata lokal tersedia | Target, batas pembelian, dan referensi kalibrasi mengikuti section berikutnya |
| Catatan transaksi | JSON Lines / JSONL | Ditetapkan | Satu kejadian per baris untuk evaluasi serta pemeriksaan hasil |
| Data gambar | PNG untuk referensi dan screenshot berlabel | Ditetapkan | Bukti visual dan pengujian detektor yang dapat diulang |
| Pengujian logika | xUnit, rilis stabil | Ditetapkan | Uji aturan gold, kuota, keadaan transaksi, pembatalan, dan timeout |
| Simulator | Aplikasi WPF terpisah yang memakai inti keputusan yang sama | Ditetapkan | Menguji transaksi dan kegagalan dengan hasil sebenarnya yang diketahui |
| Distribusi awal | Publish self-contained win-x64 dalam folder / ZIP | Ditetapkan untuk pilot x64 | Sertakan runtime .NET, DLL native, dan aset; lakukan uji pada lingkungan bersih |

Versi paket tercatat pada `Directory.Packages.props`, dan hasil resolusinya dikunci dalam `packages.lock.json` pada setiap proyek. Dependensi fondasi telah diunduh dan diverifikasi pada S00: CommunityToolkit.Mvvm 8.4.2, OpenCvSharp5.Windows 5.0.0.20261018, xUnit 2.9.3, Microsoft.NET.Test.Sdk 17.14.1, dan xunit.runner.visualstudio 3.1.4.

## 2. Alasan pemilihan

### C# dan .NET 10

C# memungkinkan logika aplikasi, antarmuka, pengujian, dan penghubung API Windows dibangun dalam satu ekosistem. .NET 10 merupakan rilis LTS. SDK .NET 10.0.103 dan Windows Desktop Runtime 10.0.3 ditemukan pada PC proyek saat pemeriksaan awal; ini adalah inventaris lokal, bukan penetapan versi patch untuk rilis produk.

### WPF dan MVVM

WPF menyediakan kontrol desktop, data binding, layout, serta penataan visual melalui XAML. Target produk saat ini adalah Windows. MVVM membantu pengaturan target, status transaksi, dan tombol sesi tetap terpisah dari pembacaan gambar serta input game. CommunityToolkit.Mvvm menyediakan komponen untuk properti dan perintah UI.

Tampilan utama menggunakan bahasa pengguna seperti “Target ditemukan” dan “Menunggu hasil pembelian”. Detail teknis tersedia di panel diagnostik.

### OpenCV dan OpenCvSharp

Kebutuhan awal mencakup pemotongan area shop, pengolahan warna, pencocokan gambar, serta pengenalan angka pada tampilan yang dikalibrasi. OpenCvSharp menyediakan akses OpenCV dari C#.

Dokumentasi pengelola saat ini merekomendasikan keluarga OpenCvSharp5 untuk proyek baru. **OpenCvSharp5.Windows 5.0.0.20261018** telah diunduh, dikunci, dan lulus uji pemuatan runtime, pencocokan gambar sintetis, PNG roundtrip, serta publish x64. Paket managed, runtime native, namespace, dan contoh API harus berasal dari keluarga yang sama.

Hero pilot dan metode pengenalannya ditentukan dari screenshot nyata. Pencocokan gambar adalah baseline; model machine learning hanya ditambahkan bila evaluasi menunjukkan kebutuhan yang jelas.

### Pembacaan angka

Gold dan harga diuji dengan pemotongan area, normalisasi ukuran, pemisahan digit, lalu pencocokan referensi angka. Pendekatan ini bergantung pada konsistensi font dan skala game. Nilai nol, area kosong, dan angka tidak terbaca harus menghasilkan keadaan yang berbeda.

Jika hasilnya tidak memenuhi PRD, bandingkan mesin OCR pada data yang sama. Penambahan OCR juga mengevaluasi model bahasa, DLL native, ukuran paket, dan biaya pemrosesan. Mesin OCR belum ditetapkan sebagai dependensi wajib.

## 3. Aturan integrasi Windows

- Pengguna memilih jendela melalui pemilih capture Windows. Dukungan capture diperiksa saat berjalan.
- S01 menggunakan pool free-threaded dengan dua buffer dan polling sekitar 30 kali per detik. `SoftwareBitmap` disalin menjadi `byte[]` BGRA premultiplied yang dimiliki aplikasi. UI mengambil slot frame terbaru setiap 100 ms untuk memperbarui `WriteableBitmap`; ini bukan target FPS game.
- `GraphicsCaptureItem.Closed` memerlukan antrean pesan pada thread pembuat item. Companion memastikan DispatcherQueue tersedia sebelum membuka picker melalui helper CoreMessaging dan menghentikan queue miliknya setelah capture selesai. Fixture membuat item pada dedicated DispatcherQueue yang hidup sampai sesi dibersihkan.
- Proyek desktop menggunakan target framework dengan versi API Windows yang sesuai. Versi Windows minimum dan daftar OS yang didukung ditentukan pada fase 0.
- Frame capture dan sumber daya Direct3D memiliki masa hidup yang terbatas. Salin data yang diperlukan sebelum frame dikembalikan; lepaskan frame, tekstur, dan objek gambar setelah digunakan.
- Pemrosesan gambar dilakukan di luar jalur pembaruan UI. Gunakan gambar terbaru dan buang antrean gambar lama.
- Ukur biaya salinan GPU ke CPU dan konversi warna. Mulai dari CPU untuk deteksi, lalu optimalkan berdasarkan pengukuran.
- Koordinat gambar, area klien jendela, layar fisik, dan skala DPI harus dipetakan serta diuji, termasuk posisi monitor.
- SendInput memasukkan input ke sistem. Pemeriksaan jendela aktif dilakukan sedekat mungkin dengan pengiriman; API ini tidak mengunci input ke satu jendela game.
- Perbedaan tingkat hak akses dapat menghalangi input. Keterbatasan dicatat dari hasil pengujian dan aplikasi tidak otomatis dijalankan dengan hak administrator.
- Hasil API input bukan bukti pembelian. Konfirmasi tetap mengikuti aturan visual pada PRD.
- Timestamp observasi dan tindakan menggunakan sumber waktu monoton yang disejajarkan agar pengukuran latency bermakna.

Jika backend capture atau input awal gagal pada MCGG, simpan hasil investigasi dan evaluasi alternatif melalui antarmuka komponen yang sama. Fondasi UI dan logika transaksi tetap dapat diuji melalui rekaman dan simulator.

## 4. Struktur kode yang direncanakan

Struktur berikut sudah digunakan pada S01. Core dan simulator ditambahkan saat section berikutnya memerlukannya.

```text
Agent-MCGG/
  README.md
  docs/
    PRD.md
    TECH_STACK.md
    S01_CAPTURE.md
  src/
    McggCompanion.App/         # UI WPF dan konfigurasi sesi
    McggCompanion.Windows/     # Capture dan antrean pesan Windows
    McggCompanion.Vision/      # Pengenalan hero, angka, dan indikator hasil
  tests/
    McggCompanion.Vision.Tests/
    McggCompanion.Windows.Tests/
```

Core dapat mengonsumsi observasi gambar dan menghasilkan keputusan tanpa bergantung pada WPF atau input Windows. Sumber gambar langsung, rekaman, dan simulator mengirim format observasi yang sama. Pengendali simulator dan pengendali game tetap berbeda agar keberhasilan simulator tidak dianggap keberhasilan integrasi asli.

Untuk uji alur lengkap, detektor membaca hasil render shop simulasi seperti gambar game. Keadaan sebenarnya dan label target disimpan terpisah oleh simulator untuk menilai hasil, tanpa diberikan langsung ke detektor. Pengujian unit Core dapat memakai observasi buatan, tetapi dilaporkan terpisah dari pengujian alur gambar sampai tindakan.

Profil kalibrasi dan referensi mencantumkan versinya. Data sesi disimpan dalam folder data pengguna saat aplikasi berjalan; dataset pengembangan dan materi yang dibagikan mengikuti aturan privasi pada PRD.

## 5. Pengujian dan distribusi

Pengujian otomatis difokuskan pada logika yang dapat menyebabkan keputusan salah: batas gold, kuota, gambar kedaluwarsa, target berganti, transaksi menunggu hasil, timeout, dan penghentian. Pengenalan gambar diuji pada data berlabel yang dipisahkan berdasarkan sesi. Pemeriksaan manual dipakai untuk capture, fokus, DPI, dan input game asli.

Paket pilot berupa folder hasil publish self-contained untuk Windows x64. Folder dapat dikemas menjadi ZIP dan berisi executable beserta dependensi yang diperlukan. Uji distribusi memeriksa runtime native, aset, hak tulis data pengguna, dan perilaku saat aplikasi dipindahkan ke lokasi lain. Format installer ditentukan setelah paket pilot stabil.

Penyimpanan MVP memakai JSON dan JSONL. Database, layanan web, dan sinkronisasi cloud belum dibutuhkan oleh alur produk saat ini.

## 6. Pekerjaan setelah probe capture

1. Lengkapi versi game dan profil shop pada S02; lobby belum membuktikan keterbacaan hero, harga, atau gold.
2. Perluas pengujian game untuk resize, minimize/restore, DPI, mode layar, dan HDR sesuai daftar dukungan yang ingin ditawarkan.
3. Kumpulkan data untuk hero pilot dan ukur deteksi offline pada S03.
4. Petakan indikator keberhasilan pembelian dari tampilan shop. Rencanakan SendInput pada S06 setelah transaksi simulator lulus, dengan pemeriksaan foreground, DPI, dan gambar terbaru.

Metrik awal dan batas interpretasinya tersedia pada [S01 Capture](S01_CAPTURE.md). Belum ada pengiriman klik pembelian atau validasi transaksi game.

## 7. Rujukan

Rujukan diperiksa pada 30 Oktober 2026:

- [.NET — kebijakan dukungan](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core)
- [Microsoft — WPF overview](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/overview/)
- [Microsoft — CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [Microsoft — Screen capture](https://learn.microsoft.com/en-us/windows/apps/develop/media-authoring-processing/screen-capture)
- [Microsoft — Windows Runtime APIs dalam aplikasi desktop](https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/winrt-apis-desktop-apps)
- [Microsoft — SendInput](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-sendinput)
- [Microsoft — CreateDispatcherQueueController / CoreMessaging](https://learn.microsoft.com/en-us/windows/win32/api/dispatcherqueue/nf-dispatcherqueue-createdispatcherqueuecontroller)
- [Microsoft — QPC dan Stopwatch](https://learn.microsoft.com/en-us/windows/win32/sysinfo/acquiring-high-resolution-time-stamps)
- [Penjelasan maintainer — thread pembuat item dan Closed](https://github.com/robmikh/Win32CaptureSample/issues/17#issuecomment-663772362)
- [OpenCvSharp — dokumentasi pengelola](https://github.com/shimat/opencvsharp)
- [NuGet — OpenCvSharp5.Windows](https://www.nuget.org/packages/OpenCvSharp5.Windows)
- [xUnit — panduan penggunaan](https://xunit.net/docs/getting-started/v3/getting-started)
- [Microsoft — distribusi aplikasi .NET](https://learn.microsoft.com/en-us/dotnet/core/deploying/)
