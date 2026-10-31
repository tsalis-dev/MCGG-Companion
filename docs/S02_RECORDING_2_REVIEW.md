# S02 — Tinjauan rekaman gameplay kedua

Tanggal analisis: **1 November 2026**, Asia/Jakarta. Status: **anotasi awal dua sesi tersedia; detektor belum dievaluasi**.

Laporan ini tidak memuat identitas akun. Video, screenshot, potongan gambar, hash sumber, dan anotasi lengkap tetap di folder data pengguna. Analisis tidak mengirim input ke game.

## 1. Sumber dan pemeriksaan

Rekaman kedua berdurasi **25 menit 30 detik** (1.530,248 detik), berukuran **1920 × 1200**, dengan metadata sekitar **60 FPS** dan **91.815 frame**. Bahasa UI Indonesia; informasi shop yang diperiksa memakai mode Jumlah. Versi game, mode jendela, dan DPI belum tercatat untuk sesi ini. Ukuran video tidak membuktikan resolusi capture langsung atau FPS game.

Pemeriksaan dilakukan melalui **52 gambar ringkasan** setiap 30 detik dan **139 kandidat shop** hasil sampling setiap dua detik, ditinjau pada 14 lembar gambar. Penyaringan warna hanya membantu memilih gambar dari video; ini bukan detektor produk atau pengukuran akurasi. Sampling tersebut tidak menjamin semua kemunculan singkat ditemukan.

Dipilih **25 screenshot** pada resolusi sumber, lengkap dengan waktu media dan nomor frame. **8 frame positif** memuat **9 penawaran Uranus**, pada slot **1, 2, 4, dan 5**. Frame pada 02:24, 02:30, dan 03:10 memiliki susunan penawaran sama dan tidak dihitung sebagai kejadian independen. Label lengkap semua hero belum tersedia.

## 2. Contoh yang berguna untuk pengembangan

| Waktu video | Pengamatan visual | Kegunaan / batas |
| --- | --- | --- |
| 00:34 | Gold 1, harga hero slot 1 dan 3 adalah 2 berwarna merah | Contoh gold kurang; belum pengujian aturan transaksi |
| 00:50 | Shop kosong dengan efek | Contoh transisi untuk menahan keputusan |
| 00:58 | Uranus slot 1, harga 1 | Posisi target baru dibanding rekaman pertama |
| 01:00 | Uranus slot 2 dan 4 bersamaan, masing-masing harga 1; gold 7 | Contoh target ganda; slot 4 juga posisi baru |
| 01:02 | Kedua target sudah tidak tampak, gold 12 | Target hilang dan gold naik; urutan tindakan belum diverifikasi dari dua gambar |
| 02:24, 02:30, 03:10 | Uranus slot 5 dengan pose dan konteks fase berbeda | Kelompok tampilan penawaran sama; overlay Victory bukan penutupan shop otomatis |
| 03:30 | Popup “Slot Hero penuh.” | Bukti pesan penolakan; aturan bench, slot sementara, dan penggabungan belum tervalidasi |
| 03:50 dan 04:52 | Uranus slot 5 dan slot 2 pada shop berbeda | Variasi pose dan susunan shop |
| 07:30 dan 10:30 | Uranus pada papan dan panel daftar hero | Bukan penawaran shop |
| 11:20 dan 24:24 | Uranus papan terlihat di belakang shop transparan | Contoh sulit tanpa target shop, walaupun model Uranus masuk area gambar shop |
| 16:36 | Popup Shield of Fortune dikonversi menjadi 30 Gold | Gold dapat berubah karena mekanisme lain; bukan indikator pembelian tunggal |
| 20:04 | Uranus slot 2, harga 1, dengan prediksi naik bintang | Prediksi bukan konfirmasi transaksi |
| 21:26 | Justice Sword slot 2, harga 0 | Item, bukan contoh hero gratis |
| 23:26 | Harga hero slot 4 adalah 5, angka pool di bawahnya 0/9 | Nol jumlah pool berbeda dari harga nol |

Gold dilabeli hanya pada gambar yang angkanya diperiksa. Harga yang diperiksa dan label item tersimpan pada manifest review. Kapasitas hanya ditandai Penuh pada gambar popup eksplisit; gambar lain tetap Belum diketahui. Identitas slot yang belum dilabeli tidak dianggap sebagai contoh negatif otomatis.

## 3. Penyimpanan dan pemisahan

Hasil lokal: `%LOCALAPPDATA%/MCGG-Companion/datasets/s02/video-sample-2-20261101/`.

- `selected/` berisi 25 PNG dan indeks waktu/nomor frame.
- `reviewed-frames.json` memuat label target eksplisit, konteks sulit, harga, gold, koordinat, serta rentang sumber untuk pemeriksaan urutan berikutnya.
- `session.json`, folder sampel, dan `dataset-index.json` menyimpan 25 anotasi melalui format dataset S02.
- `uranus-review.png` merangkum contoh slot 1, 2, 4, dan 5 untuk peninjauan lokal.

Seluruh rekaman kedua memakai **satu sesi Penyesuaian**, terpisah dari sesi Pengembangan rekaman pertama. Hash sumber berbeda dan tampilan awal menunjukkan pertandingan berbeda. Gambar kedua tidak ditambahkan ke referensi Pengembangan. Kelompok Penyesuaian boleh dipakai untuk memilih ambang, sehingga tidak menjadi bukti evaluasi akhir yang independen setelah penyesuaian.

Gabungan dua rekaman kini **46 sampel parsial**, termasuk **13 frame positif / 14 posisi target berlabel**. Posisi Uranus mencakup **kelima slot**. Ini tidak sama dengan 46 keadaan shop berlabel lengkap atau peluang transaksi layak. Belum ada sesi Uji akhir; rencana PRD ≥1.000 keadaan shop, ≥300 dengan target, ≥5 sesi, dan ≥30 klip berurutan tetap terbuka. Rentang waktu yang dicatat belum menjadi klip uji timing.

Koordinat awal mengikuti [rekaman pertama](S02_RECORDING_REVIEW.md#3-koordinat-awal) pada ruang video yang sama. Potongan target diperiksa visual, tetapi model papan dapat berada di belakang area slot. Profil ini belum dipetakan ke capture S01 atau koordinat input Windows.

## 4. Pemeriksaan dan langkah berikutnya

Pemeriksaan penyimpanan lulus: **25 PNG** cocok byte dan hash, **268 potongan aktif** cocok persis dengan piksel sumber, dimensi dan kelompok sesi benar, serta label tersimpan memuat 8 frame / 9 posisi Uranus. Data Pengembangan pertama diperiksa kembali: 21 sampel, 205 potongan, dan 5 frame positif tetap pada sesi semula. Data mentah dan keluaran analisis tidak masuk Git.

**Eksperimen baseline offline S03 dapat dimulai** dengan lima referensi Pengembangan pertama dan sesi Penyesuaian kedua. Detektor harus membedakan penawaran target dari hero papan, mengembalikan kedua slot pada target ganda, serta menahan hasil saat transisi atau angka tidak terbaca. Sesi baru yang dialokasikan untuk Uji akhir tetap diperlukan sebelum mengukur kelulusan.

S02 belum dinyatakan selesai: label/profil, capture shop langsung, kapasitas, penggabungan, dan bukti transaksi masih terbuka. Precision, recall, latency, serta keberhasilan auto-buy belum diukur. Kode aplikasi dan dependensi tidak berubah; build dan tes aplikasi tidak diulang untuk analisis ini. Impor MP4 dan pemutar video belum menjadi fitur aplikasi.
