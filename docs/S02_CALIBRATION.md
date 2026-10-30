# S02 — Kalibrasi dan data Uranus

Tanggal: 30 Oktober 2026. Status: **alat anotasi tersedia; kalibrasi game dan dataset belum tervalidasi**. Hero pilot: **Uranus**, dipilih pengguna. Branch pengerjaan: `codex/s02-calibration-data`.

## 1. Alur tanpa menghentikan pertandingan

Ada dua cara memulai:

- Pilih jendela melalui pratinjau S01, lalu tekan **Bekukan untuk kalibrasi** ketika shop terlihat. Tombol mengambil salinan gambar terakhir yang diterima maksimal dua detik sebelumnya. Capture tetap berjalan dan permainan tidak dijeda.
- Simpan PNG ketika sedang bermain memakai alat screenshot yang biasa digunakan, lalu tekan **Kalibrasi dari PNG** setelah bermain. Tidak perlu menjaga shop terbuka selama anotasi. Impor belum menerima video.

Pilih PNG dari jendela game yang utuh dan konfigurasi yang konsisten. Gambar yang dipotong atau diubah ukurannya memiliki profil koordinat berbeda. Tidak ada pengiriman input ke game dari editor.

## 2. Menandai dan memberi label

1. Isi **ID sesi** untuk satu pertandingan atau rekaman. ID bawaan hanya penanda awal aplikasi, bukan pendeteksi pertandingan. Gunakan ID yang sama untuk seluruh gambar dari pertandingan tersebut, termasuk setelah aplikasi dimulai ulang. Ganti ID ketika pertandingan berbeda dimulai.
2. Tentukan kelompok **Pengembangan**, **Penyesuaian**, atau **Uji akhir** sebelum menyimpan. Lengkapi versi game, bahasa, mode, skala Windows, dan target Uranus. Nilai yang belum diketahui boleh tetap `Belum dicatat`, tetapi belum memenuhi profil dukungan tervalidasi.
3. Pilih nama area, lalu seret kotak pada gambar. Slot 1–5 dinomori dari kiri ke kanan. Tandai `Shop`, `Hero 1`–`Hero 5`, `Harga 1`–`Harga 5`, `Gold`, `Kapasitas`, dan kandidat `Indikator hasil` sesuai gambar nyata. Area yang belum ditemukan boleh ditinggalkan; kekurangannya dicatat dalam JSON.
4. Area hero harus memuat ciri visual yang akan dibandingkan pada S03. Potongan slot berisi Uranus menjadi kandidat referensi, setelah label dan keragamannya diperiksa. Potongan dari kelompok uji akhir tidak digunakan sebagai referensi pengembangan.
5. Pilih keadaan shop dan kapasitas. Setiap slot dapat berstatus belum dilabeli, terbaca, kosong/terjual, atau tidak terbaca. Isi nama hero dan harga hanya untuk slot terbaca. Harga dan gold kosong disimpan sebagai `null`; angka `0` adalah label nol yang sengaja diberikan.
6. Catat konteks indikator hasil dan kondisi sulit. Satu gambar tidak membuktikan pembelian. Untuk pemetaan hasil, perlukan rangkaian sebelum/sesudah serta keterangan pembelian manual, roll, pergantian fase, dan penggabungan yang terjadi.
7. Tekan **Simpan anotasi lokal**. Revisi pada gambar yang sama dan ID sesi yang sama memperbarui satu sampel. Jangan mengganti profil atau kelompok untuk mengakali pemeriksaan sesi.

Kotak tersimpan sebagai koordinat piksel relatif terhadap gambar sumber, bukan koordinat layar untuk klik. Viewbox menyesuaikan tampilan editor; pemetaan seleksi mengikuti ukuran piksel gambar, termasuk seretan terbalik dan batas gambar. Mengimpor gambar berukuran berbeda menghapus kotak lama. Pada ukuran yang sama, kotak dipertahankan untuk diperiksa ulang; seluruh label gambar dikosongkan.

Editor memperingatkan saat gambar beranotasi belum disimpan akan diganti atau ditutup. Penutupan jendela utama juga memeriksa editor terlebih dahulu. Profil sesi diperiksa terhadap manifest penyimpanan; perubahan kelompok, target, dimensi, atau konfigurasi pada ID yang sudah disimpan ditolak.

## 3. Penyimpanan lokal

Lokasi: `%LOCALAPPDATA%/MCGG-Companion/datasets/s02/<ID-sesi>/`.

```text
<ID-sesi>/
  session.json                  # profil dan kelompok sesi
  <ID-sampel>/
    frame.png                   # gambar sumber yang dibekukan / diimpor
    sample.json                 # label dan rujukan potongan aktif
    <ID-revisi>/
      Shop.png
      Hero-1.png
      ...
```

`sample.json` memakai schema versi 1 dan menyimpan ID sesi, kelompok, hash PNG, nama berkas sumber, `RegionFiles`, daftar area yang belum ditandai, serta anotasi manual. Gunakan hanya berkas yang dirujuk `RegionFiles` untuk membaca potongan aktif. Revisi membuat potongan baru dan mengganti JSON setelah seluruh potongan berhasil ditulis; potongan revisi lama tetap lokal. Waktu anotasi berbeda dari waktu penerimaan gambar. Pada impor, waktu penerimaan dan sequence capture tidak dikarang.

Penulisan diserialkan dengan kunci berkas lokal. Gambar dengan byte PNG identik tidak dapat disalin ke ID sesi lain. Hash tidak mendeteksi semua gambar yang sudah diubah atau frame berdekatan; pengelompokan seluruh pertandingan/rekaman tetap tanggung jawab anotator. Jangan membuat sesi baru hanya karena gambar berdekatan perlu kelompok berbeda.

Data sesi dan PNG tidak masuk GitHub. Gambar sumber mungkin memuat identitas akun; simpan lokal dan periksa serta samarkan sebelum membuat materi portofolio. Fixture sintetis untuk tes dibuat di memori atau folder artefak yang diabaikan Git.

## 4. Pengumpulan data berikutnya

Mulai dari beberapa PNG shop yang menampilkan Uranus, PNG tanpa Uranus, dan hero yang secara visual sulit dibedakan. Periksa bahwa setiap kotak memotong elemen yang benar sebelum memperbanyak data.

Rencana dataset tetap mengikuti PRD: ≥1.000 keadaan shop, ≥300 mengandung target, dari ≥5 sesi; ≥30 cuplikan berurutan untuk transisi dan timing. Pisahkan berdasarkan sesi dengan minimal 30% sesi untuk uji akhir, misalnya dua dari lima sesi. Tetapkan pembagian sebelum menyesuaikan detektor. Anotasi awal yang belum lengkap belum dihitung sebagai dataset evaluasi siap pakai.

Editor ini menangani gambar tunggal. Pengambilan klip, pemutaran rekaman, pemuatan kembali seluruh anotasi lewat UI, dan laporan evaluasi belum tersedia. Pada jendela yang masih terbuka, label dapat diperbaiki dan disimpan ulang; membuka PNG kembali memulai label gambar yang kosong. JSON lokal tetap tersedia untuk pemeriksaan.

## 5. Bukti dan batas

| Pemeriksaan | Hasil |
| --- | --- |
| Restore terkunci dan build Release | Lulus, tanpa perubahan dependensi; build 0 warning / 0 error |
| Tes Vision | 5 lulus: 2 fondasi dan 3 tes kalibrasi |
| Pemetaan area | Seretan terbalik, perubahan skala editor, pembatasan tepi, seleksi kosong, NaN, dan overflow diperiksa |
| Label dan penyimpanan | Nol berbeda dari belum terbaca; harga negatif, slot tidak valid, area keluar gambar, dan ID dengan traversal ditolak; potongan, kunci kelompok, duplikasi, serta revisi diperiksa |
| Publish x64 | Lulus; tahap publish diulang setelah menutup aplikasi uji yang mengunci paket |
| UI sintetis | Impor PNG 960 × 540, seret area, peringatan anotasi belum disimpan, dan penyimpanan PNG/potongan/JSON berhasil diperiksa |
| Tes capture desktop | Dilewati pada jalur default; tidak diulang untuk perubahan S02 ini |
| Bekukan dari capture langsung | Tersambung ke frame S01; alur tombol belum diuji langsung pada shop MCGG |
| Shop asli, referensi Uranus, dan indikator hasil | Belum terkumpul atau tervalidasi |
| Deteksi / input / pembelian | Belum diimplementasikan oleh S02 |

S02 belum selesai. Gate berikutnya adalah profil shop nyata, referensi Uranus berlabel, pemetaan kandidat bukti transaksi dari rangkaian gambar, pembagian sesi, dan pencatatan kondisi yang belum didukung. Keberhasilan alat anotasi tidak membuktikan deteksi atau auto-buy.
