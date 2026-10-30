# PRD — MCGG Companion untuk Windows

| Atribut | Nilai |
| --- | --- |
| Versi dokumen | 0.1 — draf awal |
| Tanggal | 30 Oktober 2026, Asia/Jakarta |
| Nama kerja | MCGG Companion |
| Platform target | Aplikasi MCGG versi Windows pada PC pengguna |
| Tujuan proyek | Eksperimen pribadi dan portofolio pengembangan aplikasi pendamping game |
| Fitur utama | Mendeteksi satu hero target di shop dan membelinya otomatis ketika syarat terpenuhi |
| Status implementasi | Belum dimulai; kompatibilitas dengan aplikasi game perlu dibuktikan |

## 1. Ringkasan produk

MCGG Companion adalah aplikasi desktop Windows yang membaca tampilan shop Magic Chess: Go Go, mengenali hero pilihan pengguna, dan menjalankan pembelian otomatis sesuai batas yang ditentukan pengguna. Setiap tindakan diikuti pemeriksaan hasil agar aplikasi dapat membedakan klik yang dikirim dari pembelian yang benar-benar berhasil.

Proyek berangkat dari pengalaman pengguna saat bermain: hero target terlewat ketika roll, serta klik pengambilan hero yang tampaknya tidak menghasilkan tindakan. Tujuan awalnya adalah menjawab apakah masalah tersebut dapat dibantu melalui pengenalan layar dan interaksi Windows yang andal, sekaligus menghasilkan portofolio dengan bukti pengujian.

Pembelian otomatis tetap menjadi hasil akhir MVP. Mode pemantauan, pemutaran rekaman, dan shop simulasi digunakan untuk membangun serta menguji kemampuan tersebut. Keberhasilan simulasi dan keberhasilan integrasi game asli dilaporkan secara terpisah.

## 2. Konteks dan masalah

### 2.1 Pengguna utama

Pemain MCGG yang sudah memahami hero dan strategi permainan, serta ingin mengeksplorasi aplikasi pendamping untuk mengurangi hero terlewat dan mengetahui apakah pembelian berhasil. Pemilik proyek adalah pengguna awal sekaligus pengembang portofolio.

### 2.2 Masalah yang ingin diselesaikan

| Masalah | Dampak | Respons produk |
| --- | --- | --- |
| Hero target muncul singkat ketika roll | Kesempatan pembelian terlewat | Mengenali target pada shop yang terlihat dan mengukur waktu respons |
| Klik tidak menghasilkan pembelian | Pemain mengira hero sudah diambil | Memeriksa hasil dan menampilkan status yang sesuai |
| Alat bertindak berdasarkan tampilan lama | Berpotensi membeli hero lain | Membatalkan keputusan ketika shop berubah dan memeriksa ulang sebelum klik |
| Gold, harga, atau kondisi tempat hero tidak jelas | Pembelian gagal atau melampaui anggaran | Menahan tindakan sampai syarat dapat dibaca dengan cukup yakin |
| Perubahan posisi atau ukuran jendela | Koordinat klik menjadi keliru | Memvalidasi jendela dan pemetaan area sebelum bertindak |

Penyebab masalah pada game belum diketahui. PRD ini tidak mengasumsikan bahwa klik gagal berasal dari bug tertentu, koneksi, atau performa PC. Aplikasi hanya dapat mengamati tampilan dan hasil interaksi yang tersedia.

### 2.3 Batas kemampuan yang harus diakui

- Kesesuaian hero, jumlah gold, dan hasil pembelian merupakan hasil pembacaan layar yang dapat keliru atau tidak terbaca.
- Jika shop berganti lebih cepat daripada proses membaca dan mengklik, sebagian target dapat terlewat.
- Pemeriksaan ulang dan klik eksternal bukan satu operasi yang tak terpisahkan. Roll manual masih dapat terjadi di sela keduanya.
- Input yang sudah dikirim tidak dapat ditarik kembali. Aplikasi tidak dapat menjamin game atau server memproses input tersebut.
- Tujuan eksperimen tidak menjamin izin penggunaan oleh penerbit. Kebijakan Moonton tentang bot dan skrip tetap menjadi batas eksternal proyek; lihat bagian 13.

## 3. Tujuan dan ukuran keberhasilan

### 3.1 Tujuan produk

1. Mengenali satu hero target pada konfigurasi tampilan MCGG Windows yang telah diuji.
2. Membeli target ketika shop dan syarat pembelian valid.
3. Memisahkan status deteksi, pengiriman klik, dan konfirmasi pembelian.
4. Menghentikan tindakan ketika keadaan meragukan atau konteks berubah.
5. Menyediakan bukti kualitas berupa dataset berlabel, hasil evaluasi, catatan transaksi, dan demonstrasi.

### 3.2 Target evaluasi awal

Angka di bawah adalah usulan target pengembangan, belum merupakan hasil pengujian atau janji performa. Profil PC, versi game, resolusi, skala Windows, dan mode layar harus ditetapkan pada fase 0. Setiap perubahan target dicatat beserta alasannya.

| Ukuran | Target awal | Cara mengukur |
| --- | --- | --- |
| Ketepatan deteksi hero / precision | ≥99% | Deteksi benar dibagi seluruh deteksi target pada data uji terpisah; lokasi slot harus benar |
| Target yang berhasil ditemukan / recall | ≥95% | Kejadian target yang ditemukan dibagi seluruh kejadian target berlabel |
| Waktu sampai keputusan | p95 ≤250 ms pada profil uji | Dari gambar pertama saat shop stabil sampai keputusan beli atau tahan; waktu pengambilan gambar ikut dihitung |
| Transaksi simulator | ≥200 skenario; tidak ada pembelian hero yang salah dalam pengujian | Mencakup shop berganti, klik gagal, gold kurang, harga nol, dan penghentian |
| Verifikasi pembelian | Semua klaim berhasil pada pengujian memiliki bukti yang valid | Cocokkan catatan alat dengan keadaan sebenarnya di simulator atau anotasi game |
| Batas gold dan jumlah pembelian | Tidak ada pelampauan oleh alat dalam pengujian batas | Uji nilai di bawah, tepat pada, dan di atas batas |
| Pemantauan langsung | ≥3 sesi, masing-masing ≥20 menit, tanpa crash | Catat gangguan capture, kehilangan fokus, dan perubahan jendela |
| Pembelian di game asli | ≥90% peluang layak menghasilkan pembelian terkonfirmasi pada pilot dengan ≥30 peluang layak | Peluang ditentukan dari anotasi independen; laporkan hasil terkonfirmasi, terlewat, ditahan, gagal, dibatalkan, dan ambigu |

Waktu dari kemunculan target sampai klik dan sampai konfirmasi juga dilaporkan. Keduanya berbeda dari waktu sampai keputusan dan dapat dipengaruhi animasi atau respons game.

Satu peluang layak adalah satu hero target pada satu slot dalam satu versi shop, ketika alat aktif dan kondisi sebenarnya memenuhi syarat harga, gold, kapasitas, serta profil tampilan yang didukung. Target harus terlihat stabil selama durasi minimum dukungan yang ditetapkan dari baseline dan dibekukan sebelum pilot. Gambar berulang dari peluang yang sama tidak menambah jumlah peluang. Penilaian memakai rekaman berlabel, bukan hanya deteksi yang berhasil dicatat alat; target yang terlewat atau ditahan oleh alat padahal sebenarnya layak tetap masuk penyebut rasio keberhasilan. Konflik manual yang membuat peluang tidak lagi memenuhi kondisi dukungan dilaporkan terpisah beserta jumlah dan waktunya. Durasi minimum dan aturan pengecualian harus tercantum dalam laporan.

Hasil yang ambigu bukan pembelian berhasil. Apabila pilot menemukan pembelian hero yang salah atau klaim sukses yang keliru, penyebabnya harus ditangani dan pengujian terkait diulang sebelum MVP dinyatakan selesai. Jumlah sampel selalu ditampilkan; nol kesalahan pada sampel terbatas bukan jaminan nol kesalahan pada semua kondisi.

## 4. Lingkup MVP

### 4.1 Termasuk

- Satu jendela MCGG versi Windows yang dipilih pengguna.
- Satu bahasa UI game dan satu konfigurasi resolusi, skala, serta mode jendela yang tervalidasi.
- Satu hero target aktif dari referensi visual yang sudah diuji.
- Mode **Pantau** dan **Beli otomatis**.
- Pembacaan identitas target, harga, dan gold untuk transaksi berbayar.
- Pemeriksaan kelayakan pembelian pada kondisi tempat hero yang sudah dikenali.
- Batas sisa gold minimum dan batas pembelian terkonfirmasi per sesi.
- Satu tindakan pembelian pada satu waktu, pemeriksaan ulang, dan verifikasi hasil.
- Jeda, berhenti cepat, dan catatan alasan tindakan atau pembatalan.
- Pengujian offline menggunakan screenshot, rekaman, dan shop simulasi.

Hero target pilot dipilih ketika data dikumpulkan. Daftar hero lengkap dan dukungan semua variasi visual bukan syarat MVP.

### 4.2 Harga nol dan hero gratis

Ada dua alur yang harus dibedakan:

1. **Hero berharga nol pada shop biasa.** Dapat masuk MVP jika tampilan harga nol dan bukti keberhasilannya telah divalidasi. Gold nol tidak menghalangi transaksi gratis; gold berkurang bukan syarat konfirmasi. Harga yang gagal dibaca tidak boleh dianggap nol.
2. **Hero gratis pada panel hadiah atau pilihan khusus.** Dicatat sebagai fitur lanjutan sampai tersedia contoh tampilan, cara pengambilan, dan indikator hasil. Dukungan pada shop biasa tidak otomatis mencakup panel ini.

Jika penawaran harga nol pada shop biasa tidak ditemukan atau tidak dapat diuji, statusnya ditulis **belum tervalidasi** dalam daftar dukungan. Simulasi harga nol tetap diuji.

### 4.3 Pengembangan setelah MVP

- Beberapa hero dengan prioritas dan batas masing-masing.
- Dukungan tambahan untuk resolusi, bahasa, atau variasi tampilan hero.
- Pengambilan hadiah gratis melalui panel khusus.
- Alur roll terkoordinasi agar pembacaan dan pembelian selesai sebelum roll berikutnya.
- Analisis rekaman dan perluasan katalog referensi.

Rekomendasi strategi, pengaturan formasi, penjualan hero, pengelolaan seluruh pertandingan, dan sistem yang bermain tanpa pengawasan berada di luar lingkup PRD ini.

## 5. Pengalaman pengguna

### 5.1 Layar utama

Layar utama menyediakan:

- Pilihan jendela MCGG dan pratinjau area shop.
- Hero target yang aktif.
- Pilihan **Pantau** atau **Beli otomatis**; aplikasi dibuka dalam keadaan dijeda.
- Sisa gold minimum, dalam bilangan bulat ≥0.
- Batas jumlah pembelian per sesi, dalam bilangan bulat positif.
- Tombol **Mulai**, **Jeda**, dan **Akhiri sesi**.
- Status saat ini, hasil terakhir, dan jumlah pembelian yang terkonfirmasi.
- Akses terpisah ke detail pengujian dan catatan kejadian.

Sebelum memulai Beli otomatis, pengguna mengisi batas pembelian. Contoh nilai awal untuk pilot adalah satu pembelian dengan sisa gold minimum nol; nilai ini merupakan konfigurasi pengujian, bukan rekomendasi strategi bermain.

### 5.2 Alur penggunaan

1. Pengguna membuka MCGG dan memilih jendelanya di aplikasi pendamping.
2. Aplikasi menampilkan pratinjau, lalu memeriksa kesesuaian konfigurasi tampilan.
3. Pengguna memilih target dan mode, serta menetapkan batas pembelian.
4. Pengguna menekan Mulai dan kembali memfokuskan jendela game.
5. Dalam Pantau, aplikasi menandai target pada pratinjau dan mencatat deteksi.
6. Dalam Beli otomatis, aplikasi memeriksa syarat, membaca ulang target, mengirim satu klik, dan menunggu hasil.
7. Pengguna melihat hasil atau alasan jeda. Pintasan berhenti cepat dapat digunakan saat game sedang fokus.

### 5.3 Bahasa status

| Status yang terlihat | Artinya bagi pengguna |
| --- | --- |
| Dijeda | Pemantauan atau pembelian belum berjalan |
| Siap memantau | Jendela dan konfigurasi valid |
| Target ditemukan | Hero cocok dengan target; belum berarti telah dibeli |
| Gold belum cukup | Harga atau batas sisa gold mencegah pembelian |
| Tempat hero belum tersedia | Kondisi kapasitas yang didukung belum terpenuhi |
| Tampilan belum terbaca | Pembacaan belum cukup yakin |
| Menunggu hasil pembelian | Klik sudah dikirim dan sedang diperiksa |
| Pembelian terkonfirmasi | Bukti keberhasilan memenuhi aturan verifikasi |
| Shop berubah sebelum pembelian | Keputusan dibatalkan karena tampilan berganti |
| Pembelian belum terkonfirmasi | Hasil ambigu atau waktu tunggu habis; pembelian otomatis dijeda |
| Tampilan perlu disesuaikan | Ukuran, skala, atau area shop tidak lagi cocok |
| Fokuskan jendela MCGG | Pengiriman klik ditahan sampai game kembali aktif |
| Batas pembelian tercapai | Sesi pembelian selesai sesuai batas pengguna |

Persentase kecocokan gambar dan istilah teknis ditampilkan di panel diagnostik. Skor kecocokan tidak disebut sebagai probabilitas kebenaran kecuali memang telah dikalibrasi.

## 6. Kebutuhan fungsional

| ID | Kebutuhan | Kriteria penerimaan |
| --- | --- | --- |
| FR-01 | Memilih jendela game | Pengguna dapat memilih satu jendela; pratinjau sesuai; jendela tertutup menghentikan sesi |
| FR-02 | Memvalidasi tampilan | Area shop dan pemetaan koordinat valid pada profil uji; perubahan yang tidak didukung menahan tindakan |
| FR-03 | Memilih satu target | Target memiliki nama dan referensi teruji; perubahan target membatalkan keputusan yang belum dikirim |
| FR-04 | Menjalankan Pantau | Deteksi dan catatan tersedia; mode ini tidak mengirim input pembelian |
| FR-05 | Mengenali target | Deteksi menunjukkan slot dan skor; hasil di bawah ambang menghasilkan abstain atau tidak yakin |
| FR-06 | Membaca harga dan gold | Nilai berbayar harus terbaca valid; harga tidak diketahui berbeda dari nol; nilai ambigu menahan tindakan |
| FR-07 | Menjaga sisa gold | Transaksi berbayar hanya dilanjutkan jika gold terbaru dikurangi harga ≥ batas sisa gold |
| FR-08 | Memeriksa kapasitas | Auto-buy hanya berjalan pada keadaan tempat hero yang telah divalidasi; bench penuh atau kemungkinan penggabungan yang belum dipahami menahan tindakan |
| FR-09 | Membaca ulang sebelum klik | Shop, target, slot, fokus, dan koordinat diperiksa kembali memakai gambar baru; perubahan membatalkan keputusan lama |
| FR-10 | Mengirim satu tindakan | Maksimal satu transaksi menunggu hasil; tidak ada klik berulang selama status tersebut |
| FR-11 | Memverifikasi hasil | Klik tidak langsung dihitung berhasil; indikator visual yang tervalidasi menentukan hasil; gold berubah atau target hilang saja tidak cukup |
| FR-12 | Menangani kegagalan | Klik gagal atau hasil ambigu menampilkan alasan; retry memerlukan observasi dan keputusan baru, bukan pengulangan buta |
| FR-13 | Membatasi jumlah pembelian | Penghitung bertambah hanya setelah konfirmasi; transaksi ambigu menahan pembelian berikutnya agar batas tidak terlampaui |
| FR-14 | Menangani target ganda | Jika target muncul di beberapa slot, proses satu per satu; baca ulang harga, gold, dan slot setelah tiap hasil |
| FR-15 | Menjeda dan menghentikan | Jeda dan pintasan berhenti membatalkan semua tindakan yang belum dikirim; memulai kembali membutuhkan gambar baru |
| FR-16 | Menjaga fokus dan pemetaan | Kehilangan fokus, jendela diminimalkan, atau pemetaan tidak valid menahan input; pengujian wajib mencakup perpindahan fokus saat transaksi |
| FR-17 | Mencatat kejadian | Setiap tindakan memiliki waktu, target, slot, alasan, nilai yang dibaca, dan hasil; gambar pendukung mengikuti pengaturan diagnostik |
| FR-18 | Memutar data uji | Screenshot dan rekaman dapat diproses melalui detektor yang sama; laporan memuat hasil dan profil pengujian |
| FR-19 | Memulihkan sesi | Setelah capture putus, game ditutup, atau aplikasi dimulai ulang, antrean lama dibuang dan auto-buy kembali dijeda |
| FR-20 | Menangani harga nol | Pada tampilan yang tervalidasi, harga nol dapat diproses dengan gold nol; bukti keberhasilan tidak bergantung pada gold berkurang |

Jumlah pembelian per sesi berarti transaksi yang dikonfirmasi oleh alat. Angka ini bukan jumlah salinan hero yang dimiliki pemain dan tidak memasukkan pembelian manual yang tidak dapat dibuktikan.

## 7. Aturan keputusan dan transaksi

### 7.1 Syarat pembelian

Pembelian berbayar hanya dapat dicoba ketika seluruh syarat berikut valid:

- Mode Beli otomatis aktif dan sesi tidak dijeda.
- Jendela game, fokus, serta pemetaan area masih sesuai.
- Shop terbaca stabil dan dapat diinteraksikan pada fase yang dikenali.
- Target teridentifikasi pada slot yang sama setelah pemeriksaan ulang.
- Harga dan gold terbaca; gold setelah pembelian memenuhi batas pengguna.
- Kapasitas berada pada kondisi yang telah didukung.
- Batas pembelian belum tercapai dan tidak ada transaksi menunggu hasil.

Harga nol yang tervalidasi tidak memerlukan saldo untuk membayar atau pengurangan gold sebagai bukti. Pemeriksaan target, kapasitas, batas jumlah, dan hasil tetap berlaku.

Ambang kecocokan, definisi shop stabil, batas umur gambar, dan waktu tunggu hasil ditentukan dari pengukuran fase 0–1. Parameter tersebut disimpan bersama profil pengujian dan tidak dipilih untuk sekadar memperbagus skor pada data uji.

### 7.2 Siklus transaksi

**Membaca → target ditemukan → memeriksa syarat → membaca ulang → klik dikirim → menunggu hasil → terkonfirmasi / gagal / belum terkonfirmasi.**

Setiap keputusan terkait dengan satu identitas transaksi dan versi tampilan shop. Shop baru, perubahan target, jeda, atau hilangnya konteks membatalkan tindakan lama yang belum dikirim. Pemrosesan memakai gambar terbaru; antrean gambar lama dibuang untuk mencegah tindakan tertinggal.

Jika konteks berubah setelah klik dikirim, aplikasi menyimpan kejadian itu sebagai transaksi yang perlu diperiksa. Aplikasi tidak menganggap target hilang karena roll sebagai bukti pembelian.

### 7.3 Konfirmasi keberhasilan

Indikator spesifik harus dipetakan dari MCGG asli. Kandidat bukti meliputi perubahan status slot yang menandakan terjual, perubahan hero pada tempat yang sesuai, serta perubahan gold yang konsisten untuk transaksi berbayar. Penggabungan hero dapat mengubah tampilan sehingga pola keberhasilannya perlu validasi tersendiri.

Aturan konfirmasi memakai kombinasi indikator yang telah dibuktikan. Ketika roll, pembelian manual, atau pergantian fase membuat penyebab perubahan tidak dapat dibedakan, hasilnya **belum terkonfirmasi** dan auto-buy dijeda. Pengguna dapat meninjau catatan dan memulai sesi baru; hasil ambigu tidak diam-diam diubah menjadi sukses atau dihapus dari laporan.

### 7.4 Konflik dengan input pemain

Pergerakan mouse dan roll manual dapat terjadi bersamaan dengan alat. Implementasi awal memeriksa konteks sedekat mungkin dengan pengiriman klik, menghindari antrean tindakan, dan menahan tindakan ketika konflik dapat dikenali. Ambang waktu yang dapat didukung ditentukan dari uji nyata.

MVP hanya dinyatakan mendukung kondisi operasi yang lulus pengujian. Jika auto-buy tidak andal selama roll terus-menerus, kondisi tersebut dinyatakan belum didukung dan diperlihatkan dalam laporan. Alur roll terkoordinasi menjadi pengembangan berikutnya; PRD ini tidak menjanjikan bahwa pemantauan pasif dapat menghilangkan seluruh race condition.

## 8. Kebutuhan nonfungsional

- **Respons:** gunakan target latency bagian 3 dan ukur seluruh tahapan dengan waktu yang konsisten.
- **Stabilitas:** proses gambar berjalan terpisah dari pembaruan antarmuka agar tombol jeda tetap merespons.
- **Kontrol:** pintasan berhenti harus bekerja ketika game fokus. Target penghentian keputusan baru adalah p95 ≤150 ms pada profil uji, diukur sampai pengendali tindakan dibatalkan. Input yang sudah dikirim dicatat sebagai batas kendali.
- **Pemakaian sumber daya:** ukur CPU, memori, GPU bila dipakai, dan dampaknya pada kelancaran game. Anggaran numerik ditetapkan setelah baseline; belum ada klaim bahwa alat ringan.
- **Kompatibilitas:** publikasikan daftar versi Windows, versi game, mode layar, resolusi, bahasa, serta skala yang benar-benar diuji.
- **Data:** pemrosesan dan penyimpanan MVP dilakukan lokal. Pengambilan gambar dibatasi pada jendela yang dipilih. Penyimpanan cuplikan diagnostik dapat dimatikan dan berkasnya dapat dihapus pengguna.
- **Portofolio:** identitas akun dan informasi pribadi disamarkan pada dataset, screenshot, atau rekaman yang dibagikan.
- **Pemeliharaan:** referensi hero dan konfigurasi tampilan memiliki versi; perubahan UI game memicu validasi ulang.

## 9. Rancangan teknis awal

Fondasi aplikasi ditetapkan menggunakan C# / .NET 10 dan WPF + MVVM. Rincian keputusan, kandidat pustaka pengenalan gambar, serta komponen yang masih memerlukan uji kompatibilitas dicatat pada [Tech Stack](TECH_STACK.md). Rancangan integrasi berikut memandu investigasi pada fase 0.

| Komponen | Tanggung jawab |
| --- | --- |
| Pengambil gambar | Mengambil gambar jendela yang dipilih beserta waktu dan ukurannya |
| Pembaca tampilan | Menemukan shop, slot, hero, harga, gold, dan indikator hasil yang tersedia |
| Pengambil keputusan | Menilai syarat, batas, kesegaran observasi, dan keadaan transaksi |
| Pengendali tindakan | Memvalidasi konteks lalu mengirim satu input yang ditentukan |
| Pemeriksa hasil | Menghubungkan perubahan tampilan dengan transaksi yang sedang menunggu |
| Antarmuka desktop | Menyediakan konfigurasi, status, jeda, serta catatan |
| Pemutar rekaman dan simulator | Menguji komponen secara berulang menggunakan sumber gambar dan tindakan yang terkontrol |

Kandidat pengambilan gambar adalah Windows Graphics Capture, yang menyediakan gambar dari display atau jendela aplikasi. Kandidat pengiriman input adalah SendInput. SendInput memiliki batas tingkat hak akses Windows; keberadaannya tidak membuktikan bahwa MCGG akan menerima input. Kompatibilitas keduanya menjadi bagian fase 0. Sumber: [Microsoft — Screen capture](https://learn.microsoft.com/en-us/windows/apps/develop/media-authoring-processing/screen-capture), [Microsoft — SendInput](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-sendinput).

Pencocokan gambar menjadi kandidat awal jika ciri hero cukup stabil. Pembacaan teks digunakan jika nama atau angka tersedia dan dapat dibaca. Pemilihan metode mengikuti data game, termasuk perbandingan pada hero yang mirip. Sumber konsep: [OpenCV — Template Matching](https://docs.opencv.org/4.13.0/d4/dc6/tutorial_py_template_matching.html).

Seluruh komponen di atas menggunakan pengamatan layar dan input Windows. Akses internal proses game, perubahan berkas game, dan komunikasi langsung dengan server tidak dibutuhkan oleh rancangan MVP ini.

## 10. Data dan rencana pengujian

### 10.1 Data yang dikumpulkan

Target awal dataset adalah ≥1.000 keadaan shop berlabel, dengan ≥300 keadaan yang mengandung hero target, dari ≥5 sesi terpisah. Angka ini merupakan rencana pengumpulan dan dapat disesuaikan setelah menilai keragaman tampilan. Tambahkan ≥30 cuplikan berurutan yang mencakup roll, animasi, target ganda, dan perubahan fase.

Label mencakup identitas sesi, versi game dan profil tampilan, posisi target, harga, gold, keterbacaan, keadaan shop, serta hasil transaksi jika tersedia. Data sulit seperti hero mirip, panel menutup shop, dan tampilan belum stabil harus disertakan.

Pisahkan data pengembangan, penyesuaian ambang, dan pengujian berdasarkan sesi; minimal 30% sesi dialokasikan untuk uji akhir. Gambar berdekatan dari cuplikan yang sama tidak disebar ke kelompok berbeda. Evaluasi juga melaporkan hasil per kondisi, jumlah sampel, tingkat abstain, dan kesalahan yang ditemukan.

### 10.2 Jenis bukti

| Bukti | Yang dapat dibuktikan | Batasnya |
| --- | --- | --- |
| Screenshot berlabel | Ketepatan pengenalan hero, lokasi, dan angka | Tidak membuktikan timing atau pembelian |
| Rekaman berurutan | Deteksi saat transisi, target singkat, dan latency pemrosesan | Tidak membuktikan input diterima game |
| Simulator interaktif | Aturan transaksi, kegagalan, penghentian, dan kondisi bersamaan | Tidak membuktikan kompatibilitas game asli |
| Pilot game asli | Capture, input, dan indikator pembelian pada konfigurasi uji | Hanya mendukung kondisi dan ukuran sampel yang dilaporkan |

### 10.3 Skenario wajib

| ID | Skenario | Hasil yang diharapkan |
| --- | --- | --- |
| AT-01 | Target ada, gold cukup, kapasitas valid | Satu klik; jumlah pembelian naik hanya setelah terkonfirmasi |
| AT-02 | Hero mirip target | Tidak membeli hero lain; hasil meragukan ditahan |
| AT-03 | Gold < harga | Tidak mengklik |
| AT-04 | Gold setelah pembelian tepat pada batas | Pembelian diperbolehkan jika syarat lain terpenuhi |
| AT-05 | Gold setelah pembelian kurang satu dari batas | Tidak mengklik |
| AT-06 | Gold atau harga tidak terbaca | Tidak menganggap nilainya nol; tahan tindakan |
| AT-07 | Harga nol, gold nol | Jalur gratis dapat berjalan pada profil yang tervalidasi; konfirmasi tanpa syarat gold berkurang |
| AT-08 | Tempat hero penuh atau penggabungan belum didukung | Tahan tindakan dan tampilkan alasan |
| AT-09 | Dua target di shop | Proses berurutan; baca ulang keadaan setelah transaksi pertama |
| AT-10 | Klik tidak diterima | Tidak mengklaim berhasil atau mengulang klik tanpa keputusan baru |
| AT-11 | Shop berubah sebelum pemeriksaan ulang | Batalkan keputusan dari shop lama |
| AT-12 | Shop berubah antara pemeriksaan ulang dan klik | Uji batas kondisi bersamaan; catat setiap salah beli dan jadikan penghalang kelulusan kondisi tersebut |
| AT-13 | Shop berubah saat menunggu hasil | Tidak menghitung perubahan karena roll sebagai pembelian |
| AT-14 | Pemain melakukan pembelian manual bersamaan | Hasil yang tidak dapat diatribusikan tidak dihitung sebagai sukses alat |
| AT-15 | Jendela dipindah, diubah ukurannya, atau skala berubah | Pemetaan diperbarui dan divalidasi atau tindakan ditahan |
| AT-16 | Fokus berpindah, game diminimalkan, atau panel menutup shop | Tahan input; uji perpindahan fokus tepat sebelum pengiriman |
| AT-17 | Jeda atau pintasan berhenti saat transaksi dipersiapkan | Semua tindakan yang belum dikirim dibatalkan |
| AT-18 | Capture putus, game ditutup, aplikasi dimulai ulang | Antrean lama hilang; auto-buy dijeda |
| AT-19 | Batas pembelian tercapai atau transaksi terakhir ambigu | Tidak memulai pembelian berikutnya |
| AT-20 | Panel hadiah gratis khusus muncul | Tidak menjalankan logika klik shop pada panel yang belum didukung |

Pengujian simulator wajib mencakup variasi waktu perubahan shop dan fokus, bukan hanya urutan yang selalu sama. Perubahan algoritme yang memengaruhi transaksi memerlukan pengujian ulang skenario terkait.

## 11. Urutan pengerjaan dan syarat lulus

| Fase | Pekerjaan | Hasil yang diserahkan | Syarat melanjutkan |
| --- | --- | --- | --- |
| 0. Buktikan integrasi | Catat profil PC/game; uji capture pada mode layar yang dipakai; petakan UI shop dan indikator hasil; selidiki kompatibilitas input | Catatan kelayakan, contoh gambar, daftar kondisi didukung dan kendala | Capture menghasilkan gambar yang dapat digunakan; jalur input dan bukti transaksi memiliki rencana validasi yang konkret |
| 1. Deteksi offline | Pilih hero pilot; kumpulkan dan labeli data; bandingkan pengenalan gambar/teks; tentukan ambang | Dataset terpisah per sesi, detektor, laporan evaluasi | Precision dan recall memenuhi target awal pada data uji; kesalahan diketahui |
| 2. Aplikasi Pantau | Buat pemilihan jendela, target, pratinjau, status, jeda, dan pengukuran waktu | Aplikasi desktop yang memantau game langsung | Uji stabilitas dan latency lulus; perubahan fokus/tampilan tertangani |
| 3. Transaksi simulator | Bangun shop simulasi; terapkan syarat gold, kapasitas, kuota, klik, verifikasi, dan recovery | Auto-buy berjalan pada simulator; laporan skenario wajib | ≥200 skenario lulus tanpa salah beli; penghentian dan hasil ambigu tertangani |
| 4. Integrasi auto-buy | Hubungkan pengendali tindakan ke game Windows; validasi input dan bukti hasil; bekukan kondisi dukungan lalu jalankan pilot terbatas | Auto-buy pada kondisi game yang didukung dan laporan hasil nyata | ≥90% dari ≥30 peluang layak terkonfirmasi; tidak ada salah beli atau klaim sukses keliru yang belum diselesaikan |
| 5. Penyelesaian portofolio | Rapikan UX, paket instalasi/eksekusi, panduan, diagram, video, dan keterbatasan | Rilis MVP, dokumentasi, demonstrasi, dan hasil evaluasi | Daftar selesai bagian 12 terpenuhi |

Pekerjaan data dan rancangan antarmuka dapat berjalan paralel setelah capture terbukti. Pengembangan auto-buy pada game asli bergantung pada detektor, aturan transaksi, serta indikator hasil yang tervalidasi.

Durasi dan estimasi biaya disusun setelah fase 0, karena kendala capture, kualitas data, dan kemampuan verifikasi dapat mengubah usaha secara signifikan. Setiap fase menghasilkan artefak yang dapat ditinjau sebelum melanjutkan.

Jika capture, input, atau bukti keberhasilan ternyata tidak tersedia secara andal, catat hasil investigasinya dan revisi lingkup secara eksplisit. Mode Pantau atau simulator yang selesai tetap merupakan hasil proyek yang berguna, tetapi tidak disebut sebagai MVP auto-buy game asli yang selesai.

## 12. Definisi selesai MVP

- [ ] Profil kompatibilitas dan hero pilot terdokumentasi.
- [ ] Pengguna dapat memilih jendela, target, mode, dan batas pembelian melalui UI.
- [ ] Deteksi memenuhi target pada data uji dari sesi terpisah.
- [ ] Mode Pantau lulus uji stabilitas dan latency.
- [ ] Aturan gold, kuota, harga nol, dan kapasitas yang didukung lulus pengujian.
- [ ] Simulator lulus seluruh skenario kritis dan minimal 200 skenario transaksi.
- [ ] Auto-buy telah bekerja pada aplikasi MCGG Windows pada kondisi yang dilaporkan.
- [ ] Pilot game asli mencakup ≥30 peluang layak dengan ≥90% pembelian terkonfirmasi; bukti keberhasilan, kegagalan, dan hasil ambigu dapat diperiksa.
- [ ] Klik terkirim tidak dihitung sebagai pembelian terkonfirmasi.
- [ ] Jeda, berhenti cepat, kehilangan fokus, dan pemulihan sesi telah diuji.
- [ ] Pengambilan hadiah gratis khusus dan kondisi lain yang belum didukung diberi status yang jelas.
- [ ] Paket aplikasi, panduan penggunaan, laporan pengujian, dan video demonstrasi tersedia.
- [ ] Materi portofolio menyamarkan informasi akun dan membedakan hasil simulator dari game asli.

## 13. Risiko dan asumsi terbuka

| Risiko / asumsi | Dampak | Cara membuktikan atau menangani |
| --- | --- | --- |
| Jendela game dapat ditangkap | Tanpa gambar yang valid, pengenalan tidak berjalan | Uji pada aplikasi Windows dan mode layar pengguna pada fase 0 |
| Input Windows diterima | Klik dapat ditolak meski pemanggilan input berhasil | Uji terpisah dan cocokkan dengan hasil di game; jangan menyamakan input terkirim dengan sukses |
| Ciri target cukup stabil | Skin, animasi, musim, atau update dapat mengubah deteksi | Tetapkan profil referensi dan uji kondisi sulit; validasi ulang saat berubah |
| Gold dan harga terbaca | Salah baca dapat menghasilkan keputusan keliru | Evaluasi angka secara terpisah dan tahan transaksi saat meragukan |
| Kapasitas dan penggabungan dapat dipahami | Pembelian mungkin gagal atau bukti hasil berubah | Petakan aturan dari tampilan nyata; batasi dukungan pada kondisi yang terbukti |
| Hasil pembelian memiliki indikator visual | Klaim berhasil dapat salah | Validasi kombinasi bukti; jeda ketika ambigu |
| Roll atau input manual bersamaan | Salah klik atau target terlewat masih mungkin | Uji batas waktu, batalkan keputusan lama, dan dokumentasikan kondisi yang belum didukung |
| Fokus berubah tepat saat input dikirim | Input dapat menuju jendela lain | Uji kondisi bersamaan dan validasi tepat sebelum kirim; kegagalan menjadi penghalang kelulusan |
| Kebijakan penerbit | Otomasi gameplay dapat melanggar aturan dan menimbulkan sanksi akun | Catat batas eksternal ini pada dokumentasi; jangan mengklaim alat diizinkan atau bebas sanksi |

[Kebijakan resmi Moonton](https://mlbbesports.com/legal/terms) memuat larangan bot atau skrip tanpa izin untuk memperoleh keuntungan atau memanipulasi gameplay, serta kemungkinan sanksi akun. Rujukan diperiksa pada 30 Oktober 2026. Proyek ini tidak mencakup pengelakan sistem perlindungan game.

Keputusan berikut ditutup selama fase 0–1: versi Windows/game yang dipakai, mode layar, resolusi dan skala, hero pilot, bahasa UI, letak elemen shop, indikator pembelian, aturan kapasitas, metode capture/input yang bekerja, dan parameter pembacaan. Tidak satu pun dari detail UI tersebut dianggap sudah diketahui hanya dari percakapan awal.

## 14. Materi portofolio

Portofolio akhir menunjukkan masalah yang diangkat, alur pengguna, pemisahan komponen, dan bukti hasil. Paket demonstrasi mencakup:

1. Video deteksi dari rekaman serta pemantauan langsung.
2. Demonstrasi pembelian simulator pada kondisi normal dan gagal.
3. Demonstrasi integrasi MCGG Windows pada konfigurasi yang didukung.
4. Tabel precision, recall, abstain, latency, dan hasil transaksi dengan jumlah sampelnya.
5. Contoh kejadian target terlewat atau hasil ambigu beserta respons aplikasi.
6. Panduan menjalankan aplikasi, menghentikannya, serta memahami batas dukungan.

Klaim portofolio mengikuti bukti yang tersedia. Tujuan akhirnya adalah memperlihatkan sistem yang dapat diuji dan dijelaskan, termasuk saat kondisi permainan tidak memungkinkan tindakan yang andal.
