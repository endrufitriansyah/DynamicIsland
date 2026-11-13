using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicIsland.Controls;

namespace DynamicIsland.ViewModels;

public class IconShowcaseItem
{
    public LucideIconKind Kind { get; set; }
    public string Name { get; set; } = string.Empty;
}

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "Dynamic Island untuk Windows";

    [ObservableProperty]
    private string _status = "Phase 2 — Sistem Desain & Ikon Vektor Lucide Terverifikasi";

    [ObservableProperty]
    private string _subStatus = "Seluruh warna solid, tipografi Segoe UI Variable, dan ikon vektor Lucide siap digunakan.";

    public ObservableCollection<IconShowcaseItem> Icons { get; } = new()
    {
        new IconShowcaseItem { Kind = LucideIconKind.Music, Name = "Musik" },
        new IconShowcaseItem { Kind = LucideIconKind.Play, Name = "Putar" },
        new IconShowcaseItem { Kind = LucideIconKind.Pause, Name = "Jeda" },
        new IconShowcaseItem { Kind = LucideIconKind.SkipBack, Name = "Sebelumnya" },
        new IconShowcaseItem { Kind = LucideIconKind.SkipForward, Name = "Berikutnya" },
        new IconShowcaseItem { Kind = LucideIconKind.Battery, Name = "Baterai" },
        new IconShowcaseItem { Kind = LucideIconKind.BatteryCharging, Name = "Mengisi Daya" },
        new IconShowcaseItem { Kind = LucideIconKind.Cpu, Name = "Prosesor" },
        new IconShowcaseItem { Kind = LucideIconKind.Wifi, Name = "Jaringan" },
        new IconShowcaseItem { Kind = LucideIconKind.Bell, Name = "Notifikasi" },
        new IconShowcaseItem { Kind = LucideIconKind.Timer, Name = "Penghitung Waktu" },
        new IconShowcaseItem { Kind = LucideIconKind.Clipboard, Name = "Papan Klip" },
        new IconShowcaseItem { Kind = LucideIconKind.Settings, Name = "Pengaturan" },
        new IconShowcaseItem { Kind = LucideIconKind.Launcher, Name = "Peluncur Aplikasi" },
        new IconShowcaseItem { Kind = LucideIconKind.Calendar, Name = "Kalender" },
        new IconShowcaseItem { Kind = LucideIconKind.Download, Name = "Unduhan" },
        new IconShowcaseItem { Kind = LucideIconKind.Volume, Name = "Volume" },
        new IconShowcaseItem { Kind = LucideIconKind.Search, Name = "Pencarian" },
        new IconShowcaseItem { Kind = LucideIconKind.Close, Name = "Tutup" },
        new IconShowcaseItem { Kind = LucideIconKind.ChevronDown, Name = "Ekspansi" },
        new IconShowcaseItem { Kind = LucideIconKind.Refresh, Name = "Segarkan" }
    };

    public MainViewModel()
    {
    }
}
