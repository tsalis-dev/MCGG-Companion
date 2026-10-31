# MCGG Companion

Proyek eksperimen dan portofolio aplikasi pendamping MCGG versi Windows. MVP ditujukan untuk mengenali satu hero pilihan di shop, menjalankan pembelian sesuai batas pengguna, dan memverifikasi hasilnya.

## Dokumentasi

- [PRD dan urutan pengerjaan](docs/PRD.md)
- [Tech stack dan keputusan teknis](docs/TECH_STACK.md)
- [Panduan pengembangan dan section S00–S07](docs/DEVELOPMENT.md)
- [Hasil dan batas pengujian capture S01](docs/S01_CAPTURE.md)
- [Kalibrasi dan pengumpulan data Uranus S02](docs/S02_CALIBRATION.md)
- [Tinjauan rekaman gameplay pertama](docs/S02_RECORDING_REVIEW.md)
- [Skill Ponytail dan sumbernya](docs/THIRD_PARTY_SKILLS.md)

Fondasi: C# / .NET 10, WPF + MVVM, OpenCvSharp, dan Windows Graphics Capture. Integrasi input serta pembelian mengikuti tahap berikutnya.

## Menjalankan pratinjau

Gunakan Windows x64 dan SDK .NET 10. Implementasi S01 tersedia pada branch `main`. Dari root repository:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\verify.ps1
dotnet run --project .\src\McggCompanion.App\McggCompanion.App.csproj
```

1. Buka MCGG, lalu tekan **Pilih jendela** di Companion dan pilih jendela MCGG pada pemilih Windows.
2. Periksa pratinjau, dimensi gambar, dan umur gambar terakhir. Lobby dapat digunakan untuk pemeriksaan awal.
3. Tekan **Hentikan** untuk mengakhiri capture dan membersihkan pratinjau. Membatalkan pilihan sumber baru mempertahankan capture yang sedang berjalan.
4. Buka **Detail pengujian dan profil**, isi versi game serta mode tampilan, lalu tekan **Simpan profil**. Profil JSON disimpan lokal di `%LOCALAPPDATA%/MCGG-Companion/profiles` dan berisi metadata, tanpa piksel gambar.

Untuk S02 pada branch `codex/s02-calibration-data`, gunakan **Bekukan untuk kalibrasi** atau **Kalibrasi dari PNG** untuk membuka editor gambar tetap. Tandai area shop dan beri label hero pilot **Uranus**. PNG dan anotasi disimpan lokal; [panduan S02](docs/S02_CALIBRATION.md) menjelaskan pemisahan sesi dan batas alat.

Verifikasi menjalankan restore terkunci, build Release, lima tes Vision, dan publish ke `artifacts/publish/win-x64`. Tes capture desktop secara default ditandai **dilewati**; cara menjalankannya tersedia pada [catatan S01](docs/S01_CAPTURE.md).

## Status

**S00 selesai:** fondasi lokal dan [CI GitHub S00](https://github.com/tsalis-dev/MCGG-Companion/actions/runs/40987133392) telah lulus. **S01: implementasi dan profil lobby tervalidasi**, dengan [CI S01 lulus](https://github.com/tsalis-dev/MCGG-Companion/actions/runs/41015101843). Fixture sintetis lulus capture, resize, stop, restart, dan penutupan sumber. Capture shop, variasi tampilan game, pengenalan hero, serta auto-buy belum tervalidasi. Rincian bukti dan batas ada pada [S01 Capture](docs/S01_CAPTURE.md).

Setiap perubahan kode dikelompokkan menjadi commit yang utuh, diperiksa, lalu didorong ke branch fitur di [tsalis-dev/MCGG-Companion](https://github.com/tsalis-dev/MCGG-Companion). Integrasi berikutnya ke `main` melalui pull request dan pemeriksaan CI. S01 digabung secara fast-forward dengan izin eksplisit pengguna pada 30 Oktober 2026 setelah CI branch lulus; seluruh commit pengerjaan dipertahankan.

**S02 berjalan:** alat kalibrasi dan anotasi telah diuji dengan gambar sintetis; [CI S02 lulus](https://github.com/tsalis-dev/MCGG-Companion/actions/runs/41050925300). Satu rekaman gameplay telah ditinjau: 21 screenshot dan 5 referensi Uranus tersimpan lokal. Profil capture langsung, aturan hasil, dan dataset evaluasi belum tervalidasi. Perubahan tersedia di branch `codex/s02-calibration-data`, belum digabung ke `main`.
