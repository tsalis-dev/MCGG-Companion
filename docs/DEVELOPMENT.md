# Pengembangan — MCGG Companion

Acuan: [PRD](PRD.md), [Tech Stack](TECH_STACK.md), dan [instruksi proyek](../AGENTS.md).

## 1. Lingkungan awal

Inventaris pada 30 Oktober 2026: Windows build `10.0.26200`, CPU x64, SDK .NET `10.0.103`, dan Windows Desktop Runtime `10.0.3`. Inventaris ini tidak membuktikan kompatibilitas MCGG atau menetapkan versi Windows minimum produk.

Fondasi memakai C# / .NET 10, WPF + MVVM, serta OpenCvSharp5 Windows x64. Versi SDK berada di `global.json`, versi paket terpusat pada `Directory.Packages.props`, dan lockfile berada pada setiap proyek. Gunakan patch yang didukung dan perbarui versi secara sengaja beserta hasil verifikasinya.

Struktur yang sudah digunakan:

- `src/McggCompanion.App`: aplikasi WPF dan pemeriksaan lingkungan awal.
- `src/McggCompanion.Vision`: pemrosesan gambar yang sudah dibutuhkan.
- `src/McggCompanion.Windows`: integrasi capture dan antrean pesan Windows pada S01.
- `tests/McggCompanion.Vision.Tests`: pengujian gambar dan pemuatan runtime native.
- `tests/McggCompanion.Windows.Tests`: fixture capture desktop sintetis yang dijalankan secara eksplisit.

Core dan simulator ditambahkan ketika hasil section memerlukannya. Daftar komponen pada Tech Stack merupakan arah pengembangan, bukan kewajiban membuat proyek kosong saat setup.

## 2. Perintah pengembangan

Jalankan dari root repository menggunakan PowerShell pada Windows.

Verifikasi lengkap menjalankan restore terkunci, build Release, pengujian, dan publish aplikasi x64:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\verify.ps1
```

Perintah yang dijalankan oleh script:

```powershell
dotnet restore .\MCGG-Companion.sln --locked-mode
dotnet build .\MCGG-Companion.sln --configuration Release --no-restore
dotnet test .\MCGG-Companion.sln --configuration Release --no-build --no-restore
dotnet publish .\src\McggCompanion.App\McggCompanion.App.csproj --configuration Release --runtime win-x64 --self-contained true --no-restore --output .\artifacts\publish\win-x64
```

Menjalankan aplikasi atau pengujian Vision saja:

```powershell
dotnet run --project .\src\McggCompanion.App\McggCompanion.App.csproj
dotnet test .\tests\McggCompanion.Vision.Tests\McggCompanion.Vision.Tests.csproj --configuration Release
```

Pada penambahan atau perubahan dependensi yang disengaja, jalankan `dotnet restore .\MCGG-Companion.sln` untuk memperbarui lockfile, tinjau perubahannya, lalu jalankan kembali `scripts/verify.ps1`. Restore biasa bukan pengganti pemeriksaan terkunci.

Publish menghasilkan folder `artifacts/publish/win-x64`. Build dan publish membuktikan paket dapat dibuat; dukungan game tetap membutuhkan pengujian section terkait. CI Windows memanggil script verifikasi yang sama. Upload atau rilis paket publik dikerjakan pada S07 setelah penyertaan lisensi/notices dependensi dan pengujian paket distribusi selesai.

Tes Windows desktop secara default di-skip pada discovery xUnit. Menjalankannya memerlukan sesi Windows interaktif dan `MCGG_DESKTOP_TESTS=1`; lihat [perintah dan hasil S01](S01_CAPTURE.md). Skip tidak dihitung sebagai keberhasilan capture.

## 3. Section pengerjaan

S00 **selesai**, dengan verifikasi lokal serta CI GitHub telah lulus pada 30 Oktober 2026. S01 telah memvalidasi implementasi capture dan profil lobby MCGG pada satu konfigurasi; profil shop serta perluasan konfigurasi belum tervalidasi. Input game belum diuji.

| Section | Status | Output | Gate sebelum dinyatakan selesai | Contoh commit logis |
| --- | --- | --- | --- | --- |
| S00 — Fondasi | Selesai | Solution App/Vision/Vision.Tests, dependensi, lockfile, UI awal, script verifikasi, CI, dan skill lokal | Restore terkunci, build Release, pengujian native/gambar, publish x64, dan pembukaan UI awal lulus | `chore: bootstrap Windows development environment and Ponytail skills` |
| S01 — Probe capture | Implementasi + lobby tervalidasi; perluasan profil belum | Pemilih jendela, pratinjau, timestamp, profil metadata lokal, dan fixture desktop | Capture lobby serta kontrol UI lulus; resize/close lulus pada fixture; pengujian shop dan variasi game tetap terbuka sesuai matriks [S01](S01_CAPTURE.md) | `feat(capture): preview Windows sources and validate session lifecycle` |
| S02 — Kalibrasi dan data | Belum dimulai | Area shop, referensi satu hero, label, pemetaan indikator hasil, dan pembagian dataset | Profil koordinat benar; data dipisah per sesi; kondisi yang belum didukung tercatat | `feat(calibration): define the pilot shop profile` |
| S03 — Deteksi offline | Belum dimulai | Pengenalan target, harga, gold, keadaan tidak terbaca, dan evaluasi | Precision ≥99% dan recall ≥95% pada data uji terpisah; jumlah sampel dan abstain dilaporkan | `feat(vision): detect the pilot hero and shop values` |
| S04 — Mode Pantau | Belum dimulai | UI target/sesi, deteksi langsung, jeda, pintasan berhenti, dan catatan lokal | p95 keputusan ≤250 ms pada profil uji; ≥3 sesi masing-masing ≥20 menit; perubahan fokus/tampilan dan capture putus tertangani | `feat(monitor): observe the live shop` |
| S05 — Transaksi simulator | Belum dimulai | Shop simulasi, aturan gold/kuota, pengendali transaksi, dan verifikasi hasil | ≥200 skenario transaksi tanpa salah beli; penghentian, konflik, dan hasil ambigu diuji sesuai PRD | `feat(simulator): validate purchase transactions` |
| S06 — Auto-buy game asli | Belum dimulai | Input Windows, pemeriksaan ulang, indikator hasil tervalidasi, dan laporan pilot | ≥90% dari ≥30 peluang layak terkonfirmasi pada kondisi dukungan yang dibekukan; tidak ada salah beli atau klaim sukses keliru yang belum diselesaikan | `feat(purchase): integrate validated Windows input` |
| S07 — Paket dan portofolio | Belum dimulai | Paket self-contained, panduan, daftar dukungan, laporan, dan video | Paket diuji pada lingkungan bersih dan seluruh definisi selesai PRD terpenuhi | `release: package the validated MVP` |

S00 selesai berarti lingkungan pengembangan siap. Itu tidak menutup fase 0 PRD atau membuktikan bahwa game menerima capture/input. Section S01–S02 merinci investigasi dan persiapan data pada roadmap PRD; target evaluasi tetap mengikuti PRD jika dokumen tersebut berubah.

Pengendali auto-buy pada simulator memakai logika produk yang sama, sedangkan keadaan sebenarnya pada shop simulator dinilai secara independen. Uji alur lengkap membaca hasil render melalui detektor; label simulator tidak diberikan langsung ke detektor. Hasil simulator dan game asli dilaporkan terpisah.

## 4. Siklus perubahan dan commit

1. Periksa status Git, section aktif, dan kode yang terkait. Tentukan satu hasil yang dapat ditinjau.
2. Implementasikan bagian terkecil yang menyelesaikan hasil tersebut, termasuk pemanggil, konfigurasi, dan pengujian yang terdampak.
3. Jalankan pemeriksaan relevan. Untuk fondasi atau dependensi, jalankan verifikasi lengkap; untuk integrasi game, tambahkan pemeriksaan nyata sesuai gate.
4. Tinjau diff dan berkas staging. Pastikan tidak ada data akun, rahasia, data mentah pribadi, atau keluaran build.
5. Commit perubahan logis yang telah diperiksa, lalu push ke branch fitur pada `tsalis-dev/MCGG-Companion`. S01 menggunakan `codex/s01-window-capture`; buka atau perbarui pull request ke `main`.
6. Tinjau hasil CI dan review sebelum integrasi PR ke `main`. Jalur pengerjaan saat ini tidak memakai push langsung ke `main`.
7. Catat bukti dan keterbatasan. Jika pemeriksaan atau push gagal, perbaiki atau laporkan keadaan sebenarnya; jangan menandai section selesai.

Otorisasi commit dan push sudah diberikan pengguna. Satu section dapat memerlukan beberapa commit yang bermakna. Perubahan kode tidak menunggu hingga seluruh MVP selesai untuk disinkronkan.

Integrasi S01 pada 30 Oktober 2026 memakai fast-forward langsung ke `main` setelah CI branch lulus dan pengguna memberikan izin eksplisit. Tidak ada PR untuk integrasi tersebut; seluruh commit pengerjaan dipertahankan. Siklus PR di atas tetap berlaku untuk pengerjaan berikutnya.

## 5. Data dan bukti pemeriksaan

Gunakan fixture sintetis untuk pengujian awal. Data sesi, rekaman MCGG, screenshot pribadi, serta catatan dengan identitas akun disimpan di luar berkas yang dilacak Git. Referensi yang akan dibagikan harus diperiksa hak penggunaan dan informasi pribadinya.

Untuk menutup section, catat tanggal, profil uji, pemeriksaan yang dijalankan, hasil, commit, dan batas yang masih terbuka. Jangan menyalin log mentah berisi data pribadi ke dokumentasi publik. Catat ringkasan atau bukti yang telah disamarkan.

| Pemeriksaan S00 | Hasil awal |
| --- | --- |
| Restore dengan lockfile | Lulus, `--locked-mode`, 30 Oktober 2026 |
| Build Release | Lulus, 0 warning dan 0 error |
| Pengujian native/gambar | Lulus 2 tes: runtime OpenCV 5/template matching dan PNG roundtrip |
| Publish self-contained x64 | Lulus, `artifacts/publish/win-x64` |
| Aplikasi awal dapat dibuka | Lulus pemeriksaan startup tersembunyi: input idle tercapai dan proses tetap hidup; tampilan visual belum ditinjau |
| CI Windows | [Lulus pada commit e7cec2d](https://github.com/tsalis-dev/MCGG-Companion/actions/runs/40987133392): restore terkunci, build, pengujian, dan publish |

Riwayat setup yang sudah dikirim ke GitHub:

1. [`04d6b18`](https://github.com/tsalis-dev/MCGG-Companion/commit/04d6b18): PRD dan keputusan teknologi.
2. [`e229dcc`](https://github.com/tsalis-dev/MCGG-Companion/commit/e229dcc): fondasi kode, dependensi terkunci, skill Ponytail, dan section pengembangan.
3. [`e7cec2d`](https://github.com/tsalis-dev/MCGG-Companion/commit/e7cec2d): workflow CI Windows; pemeriksaan pertama lulus.

Workflow berjalan pada push kode ke `main`, branch `codex/**`, dan pull request. Perubahan Markdown atau skill instruksi saja tidak mengulang build. Verifikasi runtime sintetis dan startup tidak menggantikan uji capture, penerimaan klik, atau pembelian di MCGG asli. Pemeriksaan tampilan visual dan distribusi pada PC bersih masih mengikuti section terkait.

Hasil S01 dicatat terpisah pada [S01 Capture](S01_CAPTURE.md): verifikasi lokal lulus 0 warning/0 error, dua tes Vision lulus, satu tes desktop dilewati pada jalur default, dan tes desktop eksplisit lulus. Kode [`9b9850b`](https://github.com/tsalis-dev/MCGG-Companion/commit/9b9850b431255080a12510ea796377b25323f889) telah digabung ke `main` bersama commit CI dan dokumentasinya. [CI branch S01 lulus](https://github.com/tsalis-dev/MCGG-Companion/actions/runs/41015101843) pada commit 0927875. Bukti tes desktop interaktif lokal dicatat terpisah dari CI.

## 6. Pemeriksaan masalah awal

- Jika restore terkunci gagal setelah perubahan dependensi, tinjau versi paket dan perbarui lockfile melalui restore biasa yang disengaja. Jangan menonaktifkan mode terkunci untuk meloloskan CI.
- Jika DLL OpenCV gagal dimuat, periksa arsitektur x64, paket native, dan isi folder publish. Build managed yang lulus belum membuktikan runtime native tersedia.
- Jika capture tidak tersedia, catat mode layar dan profil Windows. Jangan mengganti pratinjau dengan data contoh lalu menyebut integrasi berhasil.
- Jika pintasan berhenti gagal didaftarkan atau DPI mengubah koordinat, tampilkan keadaan tersebut dan tahan pembelian sampai profil valid.
