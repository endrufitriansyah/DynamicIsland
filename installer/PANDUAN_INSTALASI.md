# 📖 Panduan Instalasi & Penggunaan Installer Dynamic Island

Panduan ini menjelaskan cara menginstal, mendistribusikan, dan menghapus (uninstall) aplikasi **Dynamic Island** pada sistem operasi Windows 10/11 64-bit.

---

## 1. Spesifikasi Installer

- **Nama File Installer:** `DynamicIsland-Setup.exe`
- **Lokasi File:** `PROJECT/installer-output/DynamicIsland-Setup.exe`
- **Ukuran File:** ± 47.37 MB (Hasil kompresi solid ultra LZMA2 dari total paket sumber 165 MB)
- **Arsitektur Target:** Windows 10 / Windows 11 (64-bit x64)
- **Karakteristik Paket:** **Fully Self-Contained** (Runtime .NET 10, WPF, dan WinRT sudah terintegrasi; komputer pengguna tidak perlu menginstal .NET Desktop Runtime terpisah).

---

## 2. Fitur & Perilaku Installer

1. **Lokasi Instalasi Default:**
   - Direktori default: `C:\Program Files\Dynamic Island` (membutuhkan hak administrator / persetujuan UAC).
   - Pengguna juga dapat memilih mode per-user tanpa hak admin ke folder lokal pengguna (`AppData\Local\Programs\Dynamic Island`).
2. **Shortcut:**
   - **Start Menu:** Shortcut dibuat secara otomatis di bawah folder program `Dynamic Island`.
   - **Desktop Shortcut:** Disediakan opsi centang (default: tidak dicentang sesuai standar Windows UX modern).
3. **Startup:**
   - **Tidak ada auto-start** saat boot Windows secara default.
4. **Uninstaller Terintegrasi:**
   - Terdaftar secara resmi di **Settings > Apps > Installed apps** (atau *Control Panel > Programs and Features*).
   - Dilengkapi dialog konfirmasi pembersihan data:
     > *"Do you also want to delete your Dynamic Island settings and data?"*
     - Klik **No**: Data pengaturan dan riwayat pengguna di folder `%LOCALAPPDATA%\DynamicIsland` **tetap aman tersimpan**.
     - Klik **Yes**: Membersihkan aplikasi beserta seluruh data dan konfigurasi lokal.

---

## 3. Langkah Instalasi (Bagi Pengguna Akhir)

1. Unduh atau salin file `DynamicIsland-Setup.exe`.
2. Klik ganda pada `DynamicIsland-Setup.exe`.
3. Jika muncul jendela peringatan **User Account Control (UAC)**, klik **Yes**.
4. Pilih direktori instalasi (secara default `C:\Program Files\Dynamic Island`) dan klik **Next**.
5. Centang opsi *"Create a desktop shortcut"* jika Anda menginginkan pintasan di Desktop.
6. Klik **Install** dan tunggu hingga proses ekstraksi selesai (memakan waktu beberapa detik).
7. Di halaman akhir, Anda dapat langsung mencentang opsi untuk menjalankan Dynamic Island dan klik **Finish**.

---

## 4. Cara Uninstall (Penghapusan Aplikasi)

Aplikasi dapat dihapus kapan saja dengan cara standar Windows:
1. Buka **Windows Settings** (`Win + I`) -> **Apps** -> **Installed apps**.
2. Cari **Dynamic Island**, klik menu tiga titik (...) lalu pilih **Uninstall**.
3. Pada dialog konfirmasi penyimpanan data pengguna:
   - Pilih **No** jika Anda berencana menginstal kembali di kemudian hari agar konfigurasi tetap ada.
   - Pilih **Yes** jika Anda ingin menghapus seluruh data secara permanen.
