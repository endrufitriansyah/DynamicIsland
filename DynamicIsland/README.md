# Dynamic Island untuk Windows

Aplikasi pendamping desktop Windows 11 yang ringan, elegan, dan fungsional, terinspirasi oleh paradigma interaksi Dynamic Island. Dibangun khusus untuk ekosistem desktop Windows menggunakan C#, WPF, dan .NET 10 LTS.

## Fitur Utama & Keunggulan
- **Arsitektur Native WPF**: Pemisahan MVVM yang bersih ditenagai oleh `CommunityToolkit.Mvvm`.
- **Target Platform**: Windows 11 x64 (.NET 10 LTS dengan integrasi SDK Windows 11 `net10.0-windows10.0.22621.0`).
- **Sistem Desain Minimalis Flat**: Warna solid tanpa gradien, tipografi bersih (Segoe UI Variable), grid kelipatan 4px, dan ikon vektor Lucide asli.
- **Filosofi Offline-First**: Seluruh fitur pemantauan sistem, kontrol media, papan klip, peluncur aplikasi, dan produktivitas beroperasi 100% lokal tanpa memerlukan koneksi internet maupun server eksternal.

## Kebutuhan Sistem
- **Sistem Operasi**: Windows 11 (build 22000+) / Windows 10 (build 19041+)
- **Runtime / SDK**: .NET 10.0 SDK (versi 10.0.404 atau yang lebih baru)
- **Arsitektur**: x64

## Panduan Build & Menjalankan Aplikasi
```powershell
# Masuk ke direktori proyek
cd DynamicIsland

# Kompilasi proyek
dotnet build -c Release

# Jalankan aplikasi
dotnet run --project DynamicIsland.csproj
```

## Dokumentasi Teknis
- [Arsitektur Sistem (ARCHITECTURE.md)](file:///c:/Users/ASUS%20Vivobook%20Go/Documents/PROJECT/DynamicIsland/ARCHITECTURE.md)
- [Panduan Sistem Desain (DESIGN_SYSTEM.md)](file:///c:/Users/ASUS%20Vivobook%20Go/Documents/PROJECT/DynamicIsland/DESIGN_SYSTEM.md)
- [Status Implementasi & Roadmap (IMPLEMENTATION_STATUS.md)](file:///c:/Users/ASUS%20Vivobook%20Go/Documents/PROJECT/DynamicIsland/IMPLEMENTATION_STATUS.md)
