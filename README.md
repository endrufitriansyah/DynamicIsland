# Dynamic Island untuk Windows

Aplikasi pendamping desktop Windows yang ringan, elegan, dan kaya fitur, terinspirasi oleh konsep interaksi **Dynamic Island**. Dikembangkan secara native menggunakan **C#**, **WPF**, dan **.NET 10 LTS** dengan integrasi mendalam ke Windows SDK.

---

##  Tujuan & Konsep

Dynamic Island di Windows dirancang sebagai pusat kendali interaktif yang melayang di bagian atas layar desktop. Tujuannya adalah menghadirkan informasi penting serta kontrol produktivitas secara instan tanpa mengganggu fokus kerja pengguna:

- **Minimalis & Non-Intrusif**: Tampil dalam mode *Compact* (kapsul kecil elegan di tengah atas layar) saat diam, dan bertransformasi secara mulus ke mode *Expanded* ketika diklik.
- **Transisi Animasi Halus**: Didukung oleh sistem animasi easing native WPF yang responsif untuk transisi ukuran dan transparansi.
- **Filosofi Offline-First & Privasi Penuh**: 100% fungsionalitas berjalan secara lokal di komputer Anda tanpa memerlukan akun, koneksi internet, maupun dependensi server pihak ketiga.

---

## Fitur yang Sudah Diimplementasikan

Semua fitur di bawah ini telah selesai dibangun dan teruji secara menyeluruh:

### 1. Core Shell & Interaksi
- **Mode Compact & Expanded**: Tampilan kapsul mini yang dapat diekspansi menjadi dasbor kartu interaktif berbasis tab.
- **Auto-Collapse**: Otomatis kembali ke bentuk kapsul saat pengguna mengklik area luar pulau (*click-outside detection*).
- **Positioning & Draggable**: Posisi default di tengah atas layar dengan dukungan penyesuaian posisi.
- **System Tray Integration**: Ikon di area notifikasi Windows (Tray) dengan menu cepat untuk membuka, mengatur, dan menutup aplikasi.

### 2. Integrasi Sistem Windows
- **Now Playing / Media Controller**: Terintegrasi langsung dengan Windows Media Session (GSMTC) untuk menampilkan judul lagu, artis, aplikasi pemutar musik aktif, serta kontrol navigasi (*Play/Pause/Next/Previous*) dan progress bar.
- **Hardware Monitor**: Pemantauan beban CPU (%) dan penggunaan memori RAM secara real-time via Win32 API.
- **Status Baterai & Daya**: Indikator persentase daya baterai dan status pengisian laptop/desktop secara akurat.
- **Traffic Jaringan**: Pemantauan kecepatan unduh (*download*) dan unggah (*upload*) jaringan lokal secara langsung.

### 3. Alat Produktivitas
- **Timer Pomodoro & Stopwatch**: Mode kerja fokus 25 menit, jeda singkat 5 menit, dan stopwatch presisi dengan notifikasi visual.
- **Clipboard History Manager**: Menyimpan riwayat teks yang disalin secara lokal untuk disalin ulang kapan saja dengan sekali klik.
- **Quick App Launcher**: Peluncur pintasan aplikasi favorit dengan persistensi data lokal.
- **Kalender & Jam Interaktif**: Tampilan kalender bulanan dan waktu digital berformat Bahasa Indonesia.

### 4. Pusat Notifikasi Windows
- **Notification Feed**: Membaca notifikasi sistem menggunakan Windows `UserNotificationListener` (WinRT).
- **Badge Indikator**: Titik merah penanda notifikasi aktif pada kapsul Compact.
- **Manajemen Notifikasi**: Kemampuan menutup notifikasi per-item atau membersihkan seluruh riwayat notifikasi.

### 5. Personalisasi & Pengaturan
- **Palet Aksen Warna**: 9 pilihan warna aksen solid yang dapat diganti secara instan.
- **Toggle Konfigurasi**: Pengaturan opsi *Always On Top*, *Auto-Collapse*, dan visibilitas masing-masing modul.
- **Simpan Otomatis**: Konfigurasi pengguna disimpan secara aman di folder `%LOCALAPPDATA%\DynamicIsland`.

---

## Teknologi yang Digunakan

| Komponen | Spesifikasi / Pustaka |
|---|---|
| **Bahasa Pemrograman** | C# (C# 13+) |
| **Framework GUI** | WPF (Windows Presentation Foundation) |
| **Runtime Target** | .NET 10 LTS (`net10.0-windows10.0.22621.0`) |
| **Windows SDK** | Windows SDK Build 22621 (Windows 11 22H2 / WinRT API) |
| **Pola Arsitektur** | MVVM (Model-View-ViewModel) |
| **MVVM Toolkit** | `CommunityToolkit.Mvvm` (8.4.2) |
| **Ikonografi** | Vektor Lucide Icons native (Geometry XAML) |
| **Installer** | Inno Setup 6 (LZMA2 Solid Ultra Compression) |

---

## 📂 Arsitektur & Struktur Folder

```text
PROJECT/
├── .gitignore                      # Konfigurasi file yang diabaikan Git
├── README.md                       # Dokumentasi utama proyek
├── DynamicIsland/                  # Source code utama aplikasi
│   ├── Controls/                   # Kontrol custom (LucideIcon, dll.)
│   ├── Helpers/                    # Animasi, interop Win32, window positioning
│   ├── Models/                     # Data model, entitas notifikasi & setting
│   ├── Resources/                  # Resource XAML (Icon, Gaya, Warna, Tipografi)
│   ├── Services/                   # Integrasi sistem (Media, Monitor, Tray, Baterai, dll.)
│   ├── Tests/                      # Unit test internal modul aplikasi
│   ├── ViewModels/                 # MVVM ViewModel logika antarmuka
│   ├── Views/                      # Window XAML (IslandWindow, SettingsWindow, MainWindow)
│   ├── App.xaml                    # Entry point aplikasi WPF
│   ├── app.manifest                # Deklarasi PerMonitorV2 High-DPI Awareness
│   ├── DynamicIsland.csproj        # Spesifikasi kompilasi proyek .NET
│   ├── ARCHITECTURE.md             # Dokumentasi teknis arsitektur
│   ├── DESIGN_SYSTEM.md            # Panduan token warna & tipografi desain
│   └── IMPLEMENTATION_STATUS.md    # Lembar status fase implementasi
└── installer/                      # Paket dan script instalasi
    ├── DynamicIsland-Setup.iss     # Script compiler Inno Setup
    └── PANDUAN_INSTALASI.md        # Panduan instalasi dan distribusi
```

---

## Persyaratan Sistem

- **Sistem Operasi**: Windows 11 64-bit (Build 22000 ke atas) atau Windows 10 64-bit (Build 19041 ke atas).
- **Arsitektur**: x64 (AMD64 / Intel 64-bit).
- **Untuk Menjalankan Installer (Self-Contained)**: Tidak membutuhkan runtime terpisah; seluruh dependensi .NET sudah terintegrasi.
- **Untuk Mengembangkan / Build dari Source**: .NET 10.0 SDK (versi 10.0.404 atau yang lebih baru).

---

## Cara Menjalankan dari Source Code

Pastikan .NET 10.0 SDK sudah terpasang di komputer Anda.

1. Buka terminal (PowerShell atau Command Prompt) di root folder proyek:
   ```powershell
   cd DynamicIsland
   ```
2. Jalankan perintah restore dan jalankan aplikasi:
   ```powershell
   dotnet run
   ```

---

##  Cara Build & Publish

Untuk menghasilkan rilis mandiri (**Self-Contained**) yang siap dijalankan di komputer mana saja tanpa perlu menginstal .NET Runtime:

```powershell
cd DynamicIsland
dotnet publish -c Release -r win-x64 --self-contained true -o "..\publish-self-contained"
```

Hasil publish akan berada di folder `PROJECT/publish-self-contained` dengan seluruh file dependensi WPF, WinRT, dan CoreCLR yang lengkap.

---

##  Cara Membuat Installer Windows

Proyek ini telah dilengkapi dengan script Inno Setup untuk membuat file setup instalasi mandiri:

1. Pastikan **Inno Setup 6** sudah terpasang.
2. Pastikan Anda telah melakukan publish self-contained ke folder `PROJECT/publish-self-contained`.
3. Kompilasi installer menggunakan compiler CLI Inno Setup (`ISCC`):
   ```powershell
   & "ISCC.exe" "installer\DynamicIsland-Setup.iss"
   ```
4. File installer `DynamicIsland-Setup.exe` akan otomatis terbentuk di dalam direktori `PROJECT/installer-output/`.
5. Panduan penggunaan installer selengkapnya dapat dibaca di [installer/PANDUAN_INSTALASI.md](file:///c:/Users/ASUS%20Vivobook%20Go/Documents/PROJECT/installer/PANDUAN_INSTALASI.md).

---

##  Kontribusi & Lisensi

- **Kontribusi**: Kontribusi terbuka bagi siapa saja yang ingin memperbaiki bug, mengoptimalkan animasi, maupun menambahkan modul produktivitas baru. Silakan buat *Issue* atau kirimkan *Pull Request*.
- **Lisensi**: Proyek ini dirilis di bawah lisensi open source [MIT License](LICENSE) (atau lisensi default pengembang). Bebas digunakan, dipelajari, dan dimodifikasi untuk kebutuhan pribadi maupun komersial.
