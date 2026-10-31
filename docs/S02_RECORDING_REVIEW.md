# S02 — Tinjauan rekaman gameplay pertama

Tanggal analisis: **1 November 2026**, Asia/Jakarta. Status: **kalibrasi awal dari satu rekaman tersedia; deteksi dan profil capture langsung belum tervalidasi**.

Laporan ini berisi ringkasan tanpa identitas akun. Rekaman, screenshot, potongan referensi, dan anotasi lengkap tetap lokal di folder data pengguna. Tidak ada pengiriman input ke game selama analisis.

## 1. Sumber dan cara pemeriksaan

| Atribut | Hasil |
| --- | --- |
| Sumber | Satu rekaman gameplay yang diberikan pengguna |
| Durasi media | 1.577,165 detik, sekitar 26 menit 17 detik |
| Ukuran video | 1920 × 1200 piksel |
| Frame rate metadata | Sekitar 60 FPS; bukan pengukuran FPS game atau capture Companion |
| Jumlah frame metadata | 94.630 |
| Bahasa UI | Indonesia |
| Mode game yang tampak pada pengaturan | 1920 × 1080 (Window) |
| Geometri rekaman | Ada bidang hitam atas/bawah; koordinat tetap memakai video utuh |
| Versi game dan skala Windows | Belum tercatat |

Pemeriksaan awal memakai 53 gambar ringkasan setiap 30 detik. Penyaringan lokal berikutnya mengambil sampel setiap dua detik sepanjang video dan menghasilkan **142 kandidat**, yang ditinjau melalui 15 lembar gambar. Penyaringan warna ini khusus rekaman tersebut dan menghasilkan contoh non-shop juga; **bukan detektor produk atau hasil evaluasi akurasi**. Sepuluh waktu kandidat ditandai non-shop pada indeks review lokal. Frame berulang tidak dihitung sebagai keadaan shop independen.

Rentang 01:30–01:42 dan 01:40–01:52 juga diperiksa setiap 0,25 detik. Pemeriksaan ini menemukan penawaran Uranus singkat yang terlewat oleh sampling dua detik. Ini tidak membuktikan bahwa semua kemunculan atau transaksi sepanjang rekaman telah ditemukan.

## 2. Hasil yang disimpan

- **21 screenshot** pada resolusi sumber, dengan timestamp media dan nomor frame sumber.
- **21 sampel** melalui penyimpanan dataset S02, dengan manifest sesi, PNG, JSON, dan potongan area.
- **5 frame positif Uranus**, dari **4 kejadian penawaran shop berbeda**, mencakup slot **2, 3, dan 5**.
- Contoh shop tanpa target, slot kosong, transisi pembelian manual, hero di papan ketika shop tertutup, panel daftar hero, pengaturan, item di shop, serta panel hasil.

Identitas seluruh hero belum dilabeli lengkap pada sebagian besar sampel. Label Uranus, konteks shop, beberapa harga/gold, dan satu gambar dengan lima nama hero terbaca telah ditandai secara manual. Gold yang belum diperiksa tetap tidak diketahui. Kapasitas tetap **belum diketahui**. Anotasi ini merupakan awal pengembangan, bukan dataset evaluasi yang sudah lengkap.

| Waktu relatif video | Pengamatan visual | Label awal |
| --- | --- | --- |
| 01:38.25 dan 01:38.50 | Uranus di slot 3, harga 1, dengan pose berbeda | Dua frame dari kejadian penawaran yang sama |
| 01:38.75 | Efek pengambilan menggantikan model Uranus pada sampel rapat | Transisi; bukan referensi hero stabil |
| 02:34.00 | Uranus di slot 2, harga 1 | Referensi positif |
| 02:34.50–02:40.00 | Efek di slot/bench, kemudian slot 2 kosong; gold pada screenshot terpilih tetap 34 | Kandidat rangkaian hasil manual; perubahan gold saja tidak cukup |
| 06:18.00 dan 07:00.00 | Uranus di papan atau potret bernama Uranus pada panel daftar hero | Konteks non-shop; harus ditahan |
| 16:20.00 | Justice Sword, harga 0, di slot 4 | Item; bukan hero gratis |
| 17:44.00 | Uranus di slot 5, harga 1, gold 4; label prediksi naik bintang | Referensi positif; label prediksi bukan konfirmasi pembelian |
| 17:46.00 | Slot 5 kosong, efek model di bench, gold 3 | Kandidat kombinasi bukti manual; penggabungan perlu diperiksa lebih lanjut |
| 20:48.00 | Justice Sword, harga 0, di slot 1 | Item dengan variasi tampilan |
| 23:32.00 | Uranus di slot 5, harga 1, pose menyamping | Referensi positif |
| 25:10.00 | Pengaturan gameplay menunjukkan mode Model, info hero Jumlah, dan Slot Sementara aktif | Konfigurasi dari rekaman, bukan perubahan oleh Companion |
| 25:22.00 | Nama hero menggantikan angka jumlah pool; lima nama dan harga dapat dilabeli | Varian UI berbeda dari bagian awal |

Waktu ditulis sebagai menit:detik dengan pecahan detik, relatif terhadap awal video. Urutan screenshot terpilih tidak merekam setiap frame transisi. Rentang sumber untuk pemeriksaan hasil berikutnya dicatat pada indeks lokal; belum berupa pemutar rekaman atau kumpulan klip uji timing.

## 3. Koordinat awal

Koordinat berikut memakai titik asal kiri atas **gambar video 1920 × 1200**, dengan format `(X, Y, lebar, tinggi)`.

| Area | Koordinat awal |
| --- | --- |
| Shop, termasuk model dan penawaran | `(15, 145, 1890, 580)` |
| Hero slot 1–5 | `X = 265 + 285 × (slot − 1)`, `Y = 145`, lebar `245`, tinggi `345` |
| Harga slot 1–5 | `X = 390 + 285 × (slot − 1)`, `Y = 488`, lebar `42`, tinggi `40` |
| Angka gold | `(1790, 966, 85, 64)` |
| Kandidat area bench/kapasitas | `(330, 875, 1290, 230)` |
| Kandidat indikator hasil untuk slot yang diamati | X sama dengan area hero, `Y = 145`, lebar `245`, tinggi `555` |

Area model mencakup ciri Uranus pada lima referensi yang diperiksa. Animasi atau bagian hero lain dapat melintasi batas slot; area ini tidak menjamin isolasi semua model pada semua pose. Area bench hanya lokasi pengamatan, bukan bukti bahwa aturan kapasitas sudah dipahami. Area indikator hasil hanya kandidat pengamatan visual.

Profil ini **tidak langsung menjadi koordinat input Windows**. Capture S01 sebelumnya memiliki ukuran 1922 × 1112, berbeda dari video ini. Bidang hitam, bingkai jendela, dan skala harus dipetakan ulang dari frame Companion sebelum dukungan langsung atau input dapat dinyatakan valid.

## 4. Temuan untuk tahap deteksi

1. **Model Uranus beranimasi.** Dua frame berdekatan memperlihatkan sudut yang berbeda. Satu gambar referensi saja belum membuktikan pengenalan andal.
2. **Kemunculan singkat dapat terlewat saat sampling.** Uranus terlihat pada sampel 01:38.25/01:38.50 dan sudah digantikan efek pada 01:38.75. Durasi minimum dukungan dan latency tetap harus diukur; angka ini bukan hasil p95.
3. **Identitas target harus terkait shop dan slot.** Model/potret Uranus pada papan atau panel daftar hero bukan penawaran untuk dibeli.
4. **Angka di bawah harga memiliki dua mode.** Pengaturan `Info Hero di Toko` memuat pilihan `Nama` dan `Jumlah`. Angka seperti `19/25` pada mode Jumlah bukan harga atau gold. Mode Nama pada akhir rekaman memberi jalur pembacaan teks untuk dibandingkan pada S03; belum ada OCR yang diimplementasikan atau diuji.
5. **Shop dapat memuat item harga nol.** Justice Sword tidak memvalidasi alur hero gratis. Penawaran harus dibedakan berdasarkan jenis dan identitas, bukan angka nol saja.
6. **Kapasitas tidak dapat diasumsikan dari bench biasa.** Pengaturan memperlihatkan Slot Sementara aktif, dan ada konteks penggabungan serta penyeretan. Dukungan kapasitas tetap ditahan sampai aturan tampilannya dipetakan.
7. **Konfirmasi pembelian memerlukan rangkaian bukti.** Model hilang, efek muncul, label prediksi naik bintang, atau gold berubah masing-masing tidak berdiri sendiri sebagai aturan sukses. Urutan pembelian manual dalam rekaman belum memvalidasi transaksi otomatis Companion.

## 5. Penyimpanan dan pemisahan data

Seluruh hasil berada di `%LOCALAPPDATA%/MCGG-Companion/datasets/s02/video-sample-20261101/`:

- `selected/`: 21 screenshot dan indeks waktu/nomor frame.
- `reviewed-frames.json`: konteks, label target eksplisit, harga hero/item yang diperiksa, mode Nama/Jumlah, koordinat, keterbatasan, serta rentang urutan sumber.
- `session.json` dan folder ID sampel: format dataset S02. Label target/item yang lebih rinci tetap dirujuk melalui manifest review; identitas slot yang belum dilabeli tidak dianggap negatif otomatis.
- `dataset-index.json`: hubungan screenshot terpilih dengan sampel dataset.
- `uranus-references.png`: lembar potongan lima referensi beserta waktu dan slot.

Hash sumber dan hash PNG disimpan lokal. Semua gambar dari rekaman ini berada pada **satu sesi Pengembangan**. Tidak ada pemisahan frame berdekatan ke kelompok penyesuaian/uji akhir. Rekaman tambahan dari sesi berbeda tetap diperlukan untuk evaluasi independen sesuai PRD.

Ekstraksi video pada pekerjaan ini dilakukan melalui analisis lokal di luar UI. Editor aplikasi masih menerima frame tetap/PNG; impor MP4 dan pemutar video belum menjadi fitur aplikasi.

## 6. Pemeriksaan dan pekerjaan terbuka

Pemeriksaan data lulus: **21 PNG** cocok dengan sumber terpilih dan hash metadata, **205 potongan area** cocok persis dengan piksel sumber, dimensi sampel benar, serta seluruh sampel memakai sesi/kelompok yang sama. Lima sampel yang berlabel Uranus sesuai dengan lima referensi hasil review visual. Penyaringan kandidat juga melewati pemeriksaan awal pada contoh shop dan non-shop.

Kode aplikasi dan dependensi tidak diubah pada pekerjaan analisis ini; build dan tes aplikasi tidak diulang. Pemeriksaan Git memastikan MP4 diabaikan dan data mentah tidak masuk staging.

S02 tetap berjalan. Yang belum selesai: identitas hero/angka/kapasitas yang masih kosong, versi game dan DPI, pemetaan ke capture langsung, validasi aturan hasil/penggabungan/harga nol hero, variasi sesi, serta dataset evaluasi independen. Precision, recall, latency, dan keberhasilan auto-buy belum diukur atau diklaim.
