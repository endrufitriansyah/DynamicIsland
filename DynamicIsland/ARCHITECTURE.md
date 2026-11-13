# Ringkasan Arsitektur — Dynamic Island untuk Windows

## Pola Arsitektur
Aplikasi menerapkan pola **Model-View-ViewModel (MVVM)** secara ketat dengan pemisahan tanggung jawab yang jelas melalui Dependency Injection dan layanan berbasis event.

```
DynamicIsland/
├── Controls/           # Custom Control ringan (LucideIcon dengan OnRender)
├── Views/              # Tampilan XAML & Window Shell
│   ├── Island/         # IslandWindow.xaml (Jendela floating utama)
│   ├── MainWindow.xaml # Tampilan showcase sistem desain
│   └── SettingsWindow  # Jendela pengaturan modular
├── ViewModels/         # MVVM ObservableObject & RelayCommand (CommunityToolkit.Mvvm)
│   ├── IslandViewModel.cs # ViewModel pengendali status Dynamic Island, sistem & produktivitas
│   └── MainViewModel.cs   # ViewModel pengujian sistem desain
├── Models/             # Model data (ProductivityModels.cs, IslandState.cs)
├── Services/           # Layanan Windows API, Hardware Monitoring, & Produktivitas
│   ├── MediaService.cs         # Windows GSMTC Media Transport Controls & interpolasi waktu
│   ├── SystemMonitorService.cs # CPU (GetSystemTimes) & RAM (GlobalMemoryStatusEx)
│   ├── BatteryService.cs       # PowerManager (Baterai & Daya AC)
│   ├── NetworkService.cs       # NetworkInterface IPv4 Statistics (Offline)
│   ├── TimerService.cs         # Pomodoro (25m/5m/15m) & Stopwatch
│   ├── ClipboardService.cs     # Riwayat Papan Klip lokal (maks 40 item)
│   ├── LauncherService.cs      # Peluncur Aplikasi lokal dengan persistensi JSON
│   ├── CalendarService.cs      # Kalender & Format Tanggal Bahasa Indonesia
│   └── TrayIconService.cs      # Shell_NotifyIcon P/Invoke handler
├── Helpers/            # Win32 NativeMethods, WindowHelper, AnimationHelper
├── Tests/              # Pengujian mandiri terisolasi (Phase4, Phase5)
├── Resources/          # Token desain: Warna solid, Tipografi, Gaya, Ikon Lucide
└── Data/               # Penyimpanan data lokal (SQLite / Konfigurasi JSON lokal)
```

## Modul Layanan Produktivitas (Phase 5)
1. **TimerService (Pomodoro & Stopwatch)**:
   - Beroperasi 100% offline tanpa koneksi internet.
   - Mode Pomodoro terstruktur: Sesi Kerja (25 menit), Jeda Pendek (5 menit), Jeda Panjang (15 menit).
   - Menghitung waktu mundur detik demi detik, dilengkapi fungsi jeda (*pause*), lanjutkan (*resume*), dan *reset*.
   - Stopwatch mandiri dengan penghitungan waktu presisi (*hours:minutes:seconds*).
2. **ClipboardService (Riwayat Papan Klip)**:
   - Memantau teks baru yang disalin pengguna secara aman tanpa membebani performa memori.
   - Menyimpan hingga 40 entri teks lokal dalam memori aplikasi.
   - Memungkinkan pengguna menyalin kembali teks lama ke clipboard hanya dengan satu kali klik.
   - Menyediakan fitur pembersihan riwayat seketika (*Clear History*).
3. **LauncherService (Peluncur Cepat Aplikasi)**:
   - Menyimpan konfigurasi pintasan aplikasi ke berkas lokal `%APPDATA%\DynamicIsland\launcher.json`.
   - Mengintegrasikan pintasan bawaan sistem: Berkas (*Explorer*), Catatan (*Notepad*), Terminal (*CMD*), Pengelola Tugas (*Taskmgr*), dan Kalkulator (*Calc*).
   - Meluncurkan proses secara aman menggunakan `Process.Start`.
4. **CalendarService (Kalender & Jam Offline)**:
   - Menampilkan tanggal lengkap dalam Bahasa Indonesia (misal: "Jumat, 13 November 2026").
   - Generator sel kalender bulanan dengan penanda visual hari ini (*today indicator*).
