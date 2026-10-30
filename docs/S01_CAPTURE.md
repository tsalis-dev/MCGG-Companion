# S01 — Probe capture Windows

Tanggal: 30 Oktober 2026. Status: **implementasi dan profil lobby tervalidasi; perluasan profil belum**.

Kode: [`9b9850b`](https://github.com/tsalis-dev/MCGG-Companion/commit/9b9850b431255080a12510ea796377b25323f889), branch `codex/s01-window-capture`. Branch belum digabung ke `main`; [perbandingan perubahan](https://github.com/tsalis-dev/MCGG-Companion/compare/main...codex%2Fs01-window-capture) tersedia untuk peninjauan. [CI S01 lulus pada commit 0927875](https://github.com/tsalis-dev/MCGG-Companion/actions/runs/41015101843).

## 1. Yang tersedia

Pengguna dapat memilih sumber melalui pemilih Windows, melihat pratinjau, menghentikan capture, dan menyimpan profil metadata lokal. Membatalkan pilihan sumber baru mempertahankan capture yang sedang berjalan. Hentikan membersihkan gambar pratinjau.

Alur gambar: Windows Graphics Capture → SoftwareBitmap → salinan `byte[]` BGRA premultiplied → slot frame terbaru → WriteableBitmap WPF. Backend melakukan polling sekitar 30 kali per detik; UI memeriksa frame terbaru setiap 100 ms. Frame dan surface lama dilepas setelah data yang diperlukan disalin.

Frame menyertakan dimensi, stride, sequence, jumlah resize, timestamp compositor, waktu penerimaan Stopwatch, dan waktu copy. Waktu QPC/Stopwatch dipakai untuk interval; waktu UTC dicatat terpisah untuk tanggal sesi.

## 2. Profil game yang diperiksa

| Atribut | Bukti yang dicatat |
| --- | --- |
| Aplikasi | MCGG versi Windows |
| Tampilan | Lobby; belum shop |
| Versi game | Belum dicatat |
| Mode | Jendela |
| Ukuran gambar | 1922 × 1112 piksel |
| Sistem | Windows build 10.0.26200, x64 |
| Runtime / SDK | .NET 10.0.3 / SDK 10.0.103 |
| Kontrol UI | Pilih sumber, Hentikan, batal pilih ulang, dan simpan profil diperiksa |

UI menunjukkan sekitar **27,2–27,4 frame tersalin per detik** pada sesi yang diamati dan **13,1 ms untuk copy terakhir**. Angka ini bukan p95, FPS game, atau jaminan performa. UI diperbarui lebih jarang daripada proses copy; belum ada hasil uji stabilitas 3 × 20 menit maupun pengukuran dampak pada game.

## 3. Matriks bukti

| Pemeriksaan | Hasil | Lingkup |
| --- | --- | --- |
| Restore terkunci, build Release, publish x64 | Lulus; 0 warning dan 0 error | Verifikasi lokal terbaru |
| Dua tes Vision | Lulus | Runtime OpenCV dan gambar sintetis |
| CI branch S01 | Lulus: restore, build, tes, dan publish | Runner Windows GitHub; tes desktop eksplisit tetap dibuktikan terpisah di PC lokal |
| Tes desktop pada jalur default | Dilewati secara eksplisit | Tidak dihitung sebagai capture lulus |
| Fixture desktop eksplisit | Lulus; sekitar 12 detik tes, 14,37 detik total | Windows interaktif lokal |
| BGRA dan resize | Biru 321 × 241 → merah 481 × 321; warna dan layout benar | Fixture sintetis |
| Kepemilikan piksel | Salinan biru tetap sama setelah frame merah tersedia | Fixture sintetis |
| Stop dua kali, restart, penutupan sumber | Lulus; HWND hancur dan sesi selesai | Fixture sintetis |
| Pratinjau lobby MCGG | Lulus pada ukuran 1922 × 1112 | Satu profil game |
| Hentikan dan batal pilih ulang | Gambar dibersihkan saat stop; batal mempertahankan sesi | UI Companion dengan lobby MCGG |
| Simpan profil | Metadata JSON tersedia lokal; pemeriksaan tidak menemukan data piksel | UI dan profil lokal |
| Shop, hero, harga, gold | Belum diuji | S02–S03 |
| Resize dan penutupan jendela game asli | Belum diuji | Bukti fixture tidak menggantikannya |
| Minimize/restore, mixed DPI, HDR, mode lain | Belum diuji | Perluasan profil |
| Klik atau pembelian game | Belum dijalankan | S06 setelah simulator |

Tidak ada gambar mentah, UID, atau log akun yang dimasukkan ke repository.

## 4. Menjalankan tes desktop

Gunakan sesi Windows interaktif dan jalankan dari root repository:

```powershell
$env:MCGG_DESKTOP_TESTS = '1'
dotnet test .\tests\McggCompanion.Windows.Tests\McggCompanion.Windows.Tests.csproj --configuration Release --no-restore
Remove-Item Env:\MCGG_DESKTOP_TESTS
```

Tanpa variabel tersebut, atribut xUnit menandai tes sebagai dilewati saat discovery. Saat diaktifkan, lingkungan yang tidak mendukung capture atau frame yang tidak datang menjadi kegagalan. Batas tunggu 15 detik per tahap tidak diperpanjang untuk menyembunyikan kegagalan.

Fixture menggunakan jendela WPF milik pengujian. Capture item dibuat pada dedicated DispatcherQueue, sementara jendela sumber memiliki thread UI terpisah. Menutup HWND sumber tidak langsung mematikan antrean pembuat item; queue baru dihentikan sesudah sesi capture dibersihkan.

## 5. Lifecycle dan profil lokal

`GraphicsCaptureItem.Closed` memerlukan message pump pada thread pembuat item. WPF Dispatcher saja belum cukup membuktikan bahwa DispatcherQueue WinRT tersedia. Companion membuat queue current-thread melalui CoreMessaging bila diperlukan sebelum picker dibuka, kemudian menghentikan queue miliknya setelah capture berhenti. Penutupan akhir jendela Companion dijadwalkan melalui dispatcher setelah cleanup, sehingga tidak memanggil Close secara reentrant.

**Simpan profil** menulis metadata ke `%LOCALAPPDATA%/MCGG-Companion/profiles`. Data mencakup konfigurasi yang diisi pengguna, dimensi, timestamp, dan metrik sesi. Piksel tidak disimpan. Kolom `CaptureValidatedForGame` tetap `false`: menyimpan profil bukan tindakan sertifikasi dukungan game. Profil lokal tidak otomatis ditambahkan ke Git.

Cara memakai kontrol UI tersedia pada [README](../README.md). Isi versi game saat melakukan pengujian berikutnya agar profil dapat direproduksi.

## 6. Langkah berikutnya

S02 memetakan shop dan indikator keberhasilan pembelian dari game asli, termasuk keadaan yang tidak terbaca. Perluasan profil menguji resize/minimize/DPI/HDR game sesuai dukungan yang ingin dinyatakan. S03 menguji deteksi pada data terpisah.

Rencana input memakai SendInput pada S06 setelah transaksi simulator lulus. Pemeriksaan mencakup jendela foreground, pemetaan DPI, gambar terbaru, serta bukti transaksi dari shop; hasil API input tidak dianggap pembelian berhasil. S01 hanya mengamati gambar dan belum mengirim klik pembelian.

## 7. Sumber teknis

- [Microsoft — Windows Graphics Capture dan lifetime frame](https://learn.microsoft.com/en-us/windows/apps/develop/media-authoring-processing/screen-capture)
- [Microsoft — QPC dan Stopwatch](https://learn.microsoft.com/en-us/windows/win32/sysinfo/acquiring-high-resolution-time-stamps)
- [Penjelasan maintainer — DispatcherQueue pada thread pembuat item untuk Closed](https://github.com/robmikh/Win32CaptureSample/issues/17#issuecomment-663772362)
- [Microsoft — CoreMessaging CreateDispatcherQueueController dan shutdown current-thread](https://learn.microsoft.com/en-us/windows/win32/api/dispatcherqueue/nf-dispatcherqueue-createdispatcherqueuecontroller)
