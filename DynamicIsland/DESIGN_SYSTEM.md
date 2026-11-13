# Sistem Desain — Minimalist Flat UI

## Filosofi Desain
Antarmuka gelap minimalis yang fungsional, bersih, dan profesional untuk Windows 11. Tidak menggunakan gradien warna, efek blur glassmorphism yang berlebihan, pendaran neon, atau ikon emoji.

## Token Warna Solid
Didefinisikan dan dipusatkan pada `Resources/Themes/Colors.xaml`:

| Token | Nilai Hex | Fungsi / Penggunaan |
| :--- | :--- | :--- |
| `PrimaryBackground` | `#09090B` | Latar belakang utama island & jendela |
| `SecondaryBackground` | `#111113` | Latar kartu sekunder & kontainer |
| `Surface` | `#18181B` | Permukaan kartu interaktif & panel |
| `ElevatedSurface` | `#222226` | Status hover tombol & dropdown |
| `Border` | `#303036` | Garis batas tipis 1px yang tegas |
| `PrimaryText` | `#FAFAFA` | Teks utama dengan kontras tinggi |
| `SecondaryText` | `#A1A1AA` | Label sekunder, artis lagu, info perangkat |
| `MutedText` | `#71717A` | Keterangan, waktu, teks petunjuk |
| `Accent` | `#8B9CF7` | Warna aksen utama (dapat disesuaikan pengguna) |
| `Success` | `#34D399` | Indikator baterai normal, proses sukses |
| `Warning` | `#FBBF24` | Peringatan penggunaan daya atau memori tinggi |
| `Danger` | `#F87171` | Baterai kritis, tombol henti/tutup |

## Spesifikasi Geometri Dynamic Island (Phase 3)
1. **Status Compact (Pil Ringkas)**:
   - Lebar: 210 px
   - Tinggi: 34 px
   - Radius Sudut: 17 px (Pil Simetris Sempurna)
   - Border: 1 px solid `#303036`
   - Konten: Ikon status kiri (media) & status kanan (baterai/indikator), dengan titik sentral separator.
2. **Status Expanded (Kartu Modular)**:
   - Lebar: 420 px
   - Tinggi: 175 px
   - Radius Sudut: 12 px (Sudut membulat modern)
   - Border: 1 px solid `#303036`
   - Konten: Tab switcher mode (Musik & Sistem), informasi trek, bilah progres, tombol navigasi, dan metrik perangkat.
3. **Parameter Animasi**:
   - Durasi Ekspansi: 220 ms
   - Durasi Ciutkan (Collapse): 200 ms
   - Easing: `CubicEase` (`EasingMode.EaseOut`)
   - Durasi Transisi Opacity Konten: 100 - 150 ms

## Sistem Tipografi
Font utama: `Segoe UI Variable Display` & `Segoe UI Variable Text` dengan fallback ke `Segoe UI`.
- **Header**: 14pt SemiBold
- **Subheader**: 12pt Medium
- **Body**: 12pt Regular
- **Caption / Muted**: 10pt-11pt Medium/Regular

## Sistem Spasi & Grid
Menggunakan kelipatan ketat 4px: 4px, 8px, 12px, 16px, 20px, 24px, 32px.

## Standar Ikonografi
Menggunakan vektor asli dari **Lucide Icons** (`Resources/Icons/LucideIcons.xaml` & `Controls/LucideIcon.cs`).
- Stroke thickness: 1.5px - 2.0px
- Round line caps & line joins
- Tajam pada setiap skala DPI layar
- Bebas dari emoji untuk memastikan identitas desktop profesional.
