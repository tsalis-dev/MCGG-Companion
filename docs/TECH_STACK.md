# Tech Stack — MCGG Companion

Tanggal keputusan: 30 Oktober 2026. Acuan kebutuhan: [PRD](PRD.md).

## 1. Keputusan utama

Dasar aplikasi adalah **C# + .NET 10 LTS + WPF**, dengan pemrosesan gambar melalui **OpenCV / OpenCvSharp** dan integrasi jendela serta input melalui API Windows. Pemrosesan, konfigurasi, dan catatan kejadian berjalan lokal.

Pilihan teknologi tidak berarti kompatibilitas dengan MCGG telah terbukti. Fase 0 PRD menguji capture, koordinat, input, dan indikator hasil pada aplikasi MCGG Windows yang digunakan.

| Komponen | Pilihan | Status | Tujuan / bukti sebelum dipakai |
| --- | --- | --- | --- |
| Bahasa dan runtime | C# pada .NET 10 LTS | Ditetapkan | Fondasi aplikasi dan logika transaksi; gunakan rilis stabil serta patch yang masih didukung |
| Antarmuka desktop | WPF + XAML | Ditetapkan | Pemilihan jendela, target, pratinjau, status, dan kontrol sesi |
| Pola antarmuka | MVVM dengan CommunityToolkit.Mvvm | Ditetapkan | Pisahkan tampilan, status UI, dan aturan pembelian agar dapat diuji |
| Pengambilan gambar | Windows.Graphics.Capture | Pilihan awal; perlu uji integrasi | Harus menghasilkan gambar shop yang benar dan stabil pada mode layar pengguna |
| Penghubung grafis | Interop WinRT / Direct3D 11 dari C# | Pilihan awal; detail pustaka ditentukan pada fase 0 | Menyiapkan device, frame pool, dan akses piksel; ukur konversi dan salinan gambar |
| Pengenalan gambar | OpenCV melalui OpenCvSharp5 untuk Windows x64 | Pilihan awal; paket dikunci setelah uji build dan runtime | Uji pemuatan DLL, pembacaan gambar, pencocokan target, dan pengelolaan memori |
| Pembacaan gold dan harga | Pemrosesan gambar serta pencocokan digit | Baseline eksperimen | Evaluasi pada font, skala, dan variasi angka dari game asli |
| OCR umum | Kandidat tambahan jika baseline angka gagal | Opsional | Pilih mesin setelah membandingkan akurasi, latency, dan kebutuhan distribusinya |
| Pengiriman klik | Win32 SendInput melalui interop C# | Pilihan awal; perlu uji integrasi | Validasi koordinat, fokus, penerimaan input, dan hasil di game |
| Pintasan berhenti | Win32 RegisterHotKey | Pilihan awal; perlu uji konflik | Harus dapat menjeda saat game fokus; tampilkan konflik pendaftaran pintasan |
| Pengaturan dan profil | JSON dengan System.Text.Json | Ditetapkan | Simpan target, batas pembelian, referensi, dan versi kalibrasi |
| Catatan transaksi | JSON Lines / JSONL | Ditetapkan | Satu kejadian per baris untuk evaluasi serta pemeriksaan hasil |
| Data gambar | PNG untuk referensi dan screenshot berlabel | Ditetapkan | Bukti visual dan pengujian detektor yang dapat diulang |
| Pengujian logika | xUnit, rilis stabil | Ditetapkan | Uji aturan gold, kuota, keadaan transaksi, pembatalan, dan timeout |
| Simulator | Aplikasi WPF terpisah yang memakai inti keputusan yang sama | Ditetapkan | Menguji transaksi dan kegagalan dengan hasil sebenarnya yang diketahui |
| Distribusi awal | Publish self-contained win-x64 dalam folder / ZIP | Ditetapkan untuk pilot x64 | Sertakan runtime .NET, DLL native, dan aset; lakukan uji pada lingkungan bersih |

Versi library yang tepat ditulis dalam berkas dependensi saat implementasi. Tidak ada dependensi yang diunduh atau dipasang hanya dengan penulisan dokumen ini.

## 2. Alasan pemilihan

### C# dan .NET 10

C# memungkinkan logika aplikasi, antarmuka, pengujian, dan penghubung API Windows dibangun dalam satu ekosistem. .NET 10 merupakan rilis LTS. SDK .NET 10.0.103 dan Windows Desktop Runtime 10.0.3 ditemukan pada PC proyek saat pemeriksaan awal; ini adalah inventaris lokal, bukan penetapan versi patch untuk rilis produk.

### WPF dan MVVM

WPF menyediakan kontrol desktop, data binding, layout, serta penataan visual melalui XAML. Target produk saat ini adalah Windows. MVVM membantu pengaturan target, status transaksi, dan tombol sesi tetap terpisah dari pembacaan gambar serta input game. CommunityToolkit.Mvvm menyediakan komponen untuk properti dan perintah UI.

Tampilan utama menggunakan bahasa pengguna seperti “Target ditemukan” dan “Menunggu hasil pembelian”. Detail teknis tersedia di panel diagnostik.

### OpenCV dan OpenCvSharp

Kebutuhan awal mencakup pemotongan area shop, pengolahan warna, pencocokan gambar, serta pengenalan angka pada tampilan yang dikalibrasi. OpenCvSharp menyediakan akses OpenCV dari C#.

Dokumentasi pengelola saat ini merekomendasikan keluarga OpenCvSharp5 untuk proyek baru. Paket Windows yang terverifikasi tersedia saat penulisan adalah **OpenCvSharp5.Windows 5.0.0.20261018**. Versi tersebut menjadi kandidat baseline dan baru dikunci setelah uji pemuatan runtime, pengolahan gambar, serta publish x64 berhasil. Paket managed, runtime native, namespace, dan contoh API harus berasal dari keluarga yang sama.

Hero pilot dan metode pengenalannya ditentukan dari screenshot nyata. Pencocokan gambar adalah baseline; model machine learning hanya ditambahkan bila evaluasi menunjukkan kebutuhan yang jelas.

### Pembacaan angka

Gold dan harga diuji dengan pemotongan area, normalisasi ukuran, pemisahan digit, lalu pencocokan referensi angka. Pendekatan ini bergantung pada konsistensi font dan skala game. Nilai nol, area kosong, dan angka tidak terbaca harus menghasilkan keadaan yang berbeda.

Jika hasilnya tidak memenuhi PRD, bandingkan mesin OCR pada data yang sama. Penambahan OCR juga mengevaluasi model bahasa, DLL native, ukuran paket, dan biaya pemrosesan. Mesin OCR belum ditetapkan sebagai dependensi wajib.

## 3. Aturan integrasi Windows

- Pengguna memilih jendela melalui pemilih capture Windows. Dukungan capture diperiksa saat berjalan.
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

Struktur berikut adalah rencana; folder implementasi belum dibuat.

```text
Agent-MCGG/
  README.md
  docs/
    PRD.md
    TECH_STACK.md
  src/
    McggCompanion.App/         # UI WPF dan konfigurasi sesi
    McggCompanion.Core/        # Keadaan transaksi dan aturan pembelian
    McggCompanion.Windows/     # Capture, jendela, input, dan pintasan
    McggCompanion.Vision/      # Pengenalan hero, angka, dan indikator hasil
    McggCompanion.Simulator/   # Shop pengujian dengan hasil yang diketahui
  tests/
    McggCompanion.Core.Tests/
    McggCompanion.Vision.Tests/
  assets/
    references/               # Referensi yang sudah dikalibrasi
```

Core dapat mengonsumsi observasi gambar dan menghasilkan keputusan tanpa bergantung pada WPF atau input Windows. Sumber gambar langsung, rekaman, dan simulator mengirim format observasi yang sama. Pengendali simulator dan pengendali game tetap berbeda agar keberhasilan simulator tidak dianggap keberhasilan integrasi asli.

Untuk uji alur lengkap, detektor membaca hasil render shop simulasi seperti gambar game. Keadaan sebenarnya dan label target disimpan terpisah oleh simulator untuk menilai hasil, tanpa diberikan langsung ke detektor. Pengujian unit Core dapat memakai observasi buatan, tetapi dilaporkan terpisah dari pengujian alur gambar sampai tindakan.

Profil kalibrasi dan referensi mencantumkan versinya. Data sesi disimpan dalam folder data pengguna saat aplikasi berjalan; dataset pengembangan dan materi yang dibagikan mengikuti aturan privasi pada PRD.

## 5. Pengujian dan distribusi

Pengujian otomatis difokuskan pada logika yang dapat menyebabkan keputusan salah: batas gold, kuota, gambar kedaluwarsa, target berganti, transaksi menunggu hasil, timeout, dan penghentian. Pengenalan gambar diuji pada data berlabel yang dipisahkan berdasarkan sesi. Pemeriksaan manual dipakai untuk capture, fokus, DPI, dan input game asli.

Paket pilot berupa folder hasil publish self-contained untuk Windows x64. Folder dapat dikemas menjadi ZIP dan berisi executable beserta dependensi yang diperlukan. Uji distribusi memeriksa runtime native, aset, hak tulis data pengguna, dan perilaku saat aplikasi dipindahkan ke lokasi lain. Format installer ditentukan setelah paket pilot stabil.

Penyimpanan MVP memakai JSON dan JSONL. Database, layanan web, dan sinkronisasi cloud belum dibutuhkan oleh alur produk saat ini.

## 6. Pekerjaan teknis pertama

1. Buat aplikasi percobaan kecil berbasis .NET/WPF untuk memilih jendela dan menampilkan gambar shop.
2. Validasi jalur WinRT/Direct3D, ketepatan warna, perubahan ukuran, dan pengelolaan memori.
3. Uji OpenCvSharp pada gambar yang diambil, kemudian tetapkan versi paket dan runtime.
4. Ukur capture dan pemrosesan awal pada PC pengguna; catat profil uji.
5. Petakan bukti pembelian dari game dan susun pengujian input terpisah.
6. Lanjutkan fase deteksi offline pada roadmap PRD setelah kelayakan awal terbukti.

## 7. Rujukan

Rujukan diperiksa pada 30 Oktober 2026:

- [.NET — kebijakan dukungan](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core)
- [Microsoft — WPF overview](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/overview/)
- [Microsoft — CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [Microsoft — Screen capture](https://learn.microsoft.com/en-us/windows/apps/develop/media-authoring-processing/screen-capture)
- [Microsoft — Windows Runtime APIs dalam aplikasi desktop](https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/winrt-apis-desktop-apps)
- [Microsoft — SendInput](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-sendinput)
- [OpenCvSharp — dokumentasi pengelola](https://github.com/shimat/opencvsharp)
- [NuGet — OpenCvSharp5.Windows](https://www.nuget.org/packages/OpenCvSharp5.Windows)
- [xUnit — panduan penggunaan](https://xunit.net/docs/getting-started/v3/getting-started)
- [Microsoft — distribusi aplikasi .NET](https://learn.microsoft.com/en-us/dotnet/core/deploying/)
