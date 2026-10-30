# MCGG Companion

Proyek eksperimen dan portofolio aplikasi pendamping MCGG versi Windows. MVP ditujukan untuk mengenali satu hero pilihan di shop, menjalankan pembelian sesuai batas pengguna, dan memverifikasi hasilnya.

## Dokumentasi

- [PRD dan urutan pengerjaan](docs/PRD.md)
- [Tech stack dan keputusan teknis](docs/TECH_STACK.md)
- [Panduan pengembangan dan section S00–S07](docs/DEVELOPMENT.md)
- [Hasil dan batas pengujian capture S01](docs/S01_CAPTURE.md)
- [Skill Ponytail dan sumbernya](docs/THIRD_PARTY_SKILLS.md)

Fondasi: C# / .NET 10, WPF + MVVM, OpenCvSharp, dan Windows Graphics Capture. Integrasi input serta pembelian mengikuti tahap berikutnya.

## Menjalankan pratinjau

Gunakan Windows x64 dan SDK .NET 10. Implementasi S01 tersedia pada branch `codex/s01-window-capture` sebelum integrasi ke `main`. Dari root repository:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\verify.ps1
dotnet run --project .\src\McggCompanion.App\McggCompanion.App.csproj
```

1. Buka MCGG, lalu tekan **Pilih jendela** di Companion dan pilih jendela MCGG pada pemilih Windows.
2. Periksa pratinjau, dimensi gambar, dan umur gambar terakhir. Lobby dapat digunakan untuk pemeriksaan awal.
3. Tekan **Hentikan** untuk mengakhiri capture dan membersihkan pratinjau. Membatalkan pilihan sumber baru mempertahankan capture yang sedang berjalan.
4. Buka **Detail pengujian dan profil**, isi versi game serta mode tampilan, lalu tekan **Simpan profil**. Profil JSON disimpan lokal di `%LOCALAPPDATA%/MCGG-Companion/profiles` dan berisi metadata, tanpa piksel gambar.

Verifikasi menjalankan restore terkunci, build Release, dua tes OpenCV, dan publish ke `artifacts/publish/win-x64`. Tes capture desktop secara default ditandai **dilewati**; cara menjalankannya tersedia pada [catatan S01](docs/S01_CAPTURE.md).

## Status

**S00 selesai:** fondasi lokal dan [CI GitHub S00](https://github.com/tsalis-dev/MCGG-Companion/actions/runs/40987133392) telah lulus. **S01: implementasi dan profil lobby tervalidasi** pada satu konfigurasi Windows; fixture sintetis lulus capture, resize, stop, restart, dan penutupan sumber. Capture shop, variasi tampilan game, pengenalan hero, serta auto-buy belum tervalidasi. Rincian bukti dan batas ada pada [S01 Capture](docs/S01_CAPTURE.md).

Setiap perubahan kode dikelompokkan menjadi commit yang utuh, diperiksa, lalu didorong ke branch fitur di [tsalis-dev/MCGG-Companion](https://github.com/tsalis-dev/MCGG-Companion). Integrasi ke `main` melalui pull request dan pemeriksaan CI.
