using System.Text.Json.Serialization;

namespace DynamicIsland.Models;

/// <summary>
/// Model data untuk seluruh pengaturan aplikasi.
/// Disimpan di %APPDATA%\DynamicIsland\settings.json.
/// </summary>
public class AppSettings
{
    // =========================================================================
    // TAMPILAN (APPEARANCE)
    // =========================================================================

    /// <summary>Warna aksen dalam format hex (#RRGGBB). Default: #8B9CF7</summary>
    public string AccentColorHex { get; set; } = "#8B9CF7";

    // =========================================================================
    // PERILAKU ISLAND (BEHAVIOR)
    // =========================================================================

    /// <summary>Apakah island selalu di atas semua jendela. Default: true</summary>
    public bool AlwaysOnTop { get; set; } = true;

    /// <summary>Auto-collapse island saat kehilangan fokus. Default: true</summary>
    public bool AutoCollapse { get; set; } = true;

    /// <summary>Jarak dari tepi atas layar (piksel). Default: 12</summary>
    public double TopMargin { get; set; } = 12.0;

    // =========================================================================
    // MODUL AKTIF (MODULES)
    // =========================================================================

    public bool EnableMediaModule      { get; set; } = true;
    public bool EnableSystemModule     { get; set; } = true;
    public bool EnableTimerModule      { get; set; } = true;
    public bool EnableClipboardModule  { get; set; } = true;
    public bool EnableLauncherModule   { get; set; } = true;
    public bool EnableNotifModule      { get; set; } = true;

    // =========================================================================
    // PERFORMA (PERFORMANCE)
    // =========================================================================

    /// <summary>Interval pembaruan pemantau sistem (milidetik). Default: 1500</summary>
    public int SystemMonitorIntervalMs { get; set; } = 1500;

    /// <summary>Interval pembaruan jaringan (milidetik). Default: 1500</summary>
    public int NetworkMonitorIntervalMs { get; set; } = 1500;

    /// <summary>Interval polling papan klip (milidetik). Default: 1200</summary>
    public int ClipboardPollIntervalMs { get; set; } = 1200;

    // =========================================================================
    // STARTUP
    // =========================================================================

    /// <summary>
    /// Jalankan otomatis saat Windows startup. Default: false.
    /// Diimplementasikan via registry HKCU\Software\Microsoft\Windows\CurrentVersion\Run.
    /// </summary>
    public bool RunAtStartup { get; set; } = false;

    // =========================================================================
    // METADATA
    // =========================================================================

    /// <summary>Versi skema pengaturan — untuk migrasi di masa depan.</summary>
    public int SchemaVersion { get; set; } = 1;
}
