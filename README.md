# MCGG Companion

Proyek eksperimen dan portofolio aplikasi pendamping MCGG versi Windows. MVP ditujukan untuk mengenali satu hero pilihan di shop, menjalankan pembelian sesuai batas pengguna, dan memverifikasi hasilnya.

## Dokumentasi

- [PRD dan urutan pengerjaan](docs/PRD.md)
- [Tech stack dan keputusan teknis](docs/TECH_STACK.md)
- [Panduan pengembangan dan section S00–S07](docs/DEVELOPMENT.md)
- [Skill Ponytail dan sumbernya](docs/THIRD_PARTY_SKILLS.md)

Fondasi: C# / .NET 10, WPF + MVVM, OpenCvSharp, serta API Windows untuk capture dan input. Kompatibilitas dengan MCGG asli menjadi pekerjaan pertama pada fase 0.

## Menjalankan fondasi aplikasi

Gunakan Windows x64 dan SDK .NET 10. Dari root repository:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\verify.ps1
dotnet run --project .\src\McggCompanion.App\McggCompanion.App.csproj
```

Verifikasi menjalankan restore terkunci, build Release, dua tes OpenCV, dan publish ke `artifacts/publish/win-x64`. Aplikasi awal menyediakan pemeriksaan dependensi menggunakan gambar sintetis.

## Status

Fondasi lokal sudah lulus build, pengujian OpenCV, publish x64, dan pemeriksaan startup. CI GitHub sedang disiapkan. Pemilihan jendela dan capture MCGG dikerjakan pada **S01**; pengenalan hero dan auto-buy mengikuti section berikutnya.

Setiap perubahan kode dikelompokkan menjadi commit yang utuh, diperiksa, lalu dikirim ke [tsalis-dev/MCGG-Companion](https://github.com/tsalis-dev/MCGG-Companion).
