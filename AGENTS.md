# Instruksi proyek

- Baca [README](README.md), [PRD](docs/PRD.md), [Tech Stack](docs/TECH_STACK.md), dan [Development](docs/DEVELOPMENT.md) sebelum mengubah kode.
- Ikuti skill lokal [Ponytail](.agents/skills/ponytail/SKILL.md): selesaikan kebutuhan dengan perubahan terkecil yang utuh. Gunakan [Ponytail Review](.agents/skills/ponytail-review/SKILL.md) untuk review perubahan. Skill dipakai sebagai instruksi; tidak ada lifecycle hook yang dipasang.
- UI dan dokumentasi pengguna memakai Bahasa Indonesia. Detail diagnostik dipisahkan dari alur utama.
- Mulai dari App, Vision, dan Vision.Tests. Buat Core, Windows, atau Simulator ketika section pengerjaan benar-benar memerlukannya; jangan menambah proyek atau abstraksi kosong.
- Periksa status Git dan kode terkait sebelum mengedit. Pertahankan perubahan pengguna; jangan melakukan reset, force-push, atau perubahan riwayat tanpa instruksi.
- Pengguna sudah mengizinkan commit dan push perubahan kode ke `tsalis-dev/MCGG-Companion`. Kerjakan satu perubahan logis, jalankan pemeriksaan relevan, tinjau diff, lalu commit dan push ke branch fitur. S01 memakai `codex/s01-window-capture`; integrasi ke `main` disiapkan melalui PR. Tidak perlu meminta izin ulang untuk commit dan push rutin pada branch fitur.
- Gunakan `scripts/verify.ps1` sebelum menyelesaikan perubahan kode yang memengaruhi build atau dependensi. Untuk perubahan lanjutan, tambahkan pengujian kecil pada logika berisiko seperti gold, kuota, transaksi, pembatalan, dan verifikasi hasil.
- Tambahkan berkas ke staging secara terarah. Jangan commit kredensial, token, informasi akun, dataset pribadi, screenshot/rekaman mentah, log sesi, atau keluaran build.
- Simpan data sesi di folder data pengguna. Fixture sintetis yang diperlukan pengujian boleh masuk repository.
- Versi dependensi dan lockfile harus sesuai. Setelah sengaja mengubah dependensi, perbarui lockfile lalu jalankan verifikasi terkunci.
- Perbarui status section dan batas dukungan sesuai bukti. Jangan mengklaim capture, input, deteksi, atau auto-buy MCGG berhasil hanya karena build atau simulator lulus.
- Auto-buy mengikuti gate PRD, mulai dalam keadaan dijeda, dan memverifikasi pembelian. Input terkirim tidak sama dengan transaksi berhasil.
- Laporkan pemeriksaan yang benar-benar dijalankan dan yang belum dapat dilakukan. Jika push gagal, pertahankan commit lokal dan laporkan; jangan menandai sinkronisasi selesai.
