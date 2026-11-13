using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using DynamicIsland.Models;
using Microsoft.Win32;

namespace DynamicIsland.Services;

/// <summary>
/// Layanan pengelolaan pengaturan aplikasi.
/// Menyimpan dan memuat konfigurasi dari %APPDATA%\DynamicIsland\settings.json.
/// Menerapkan perubahan warna aksen secara dinamis ke resource dictionary WPF.
/// </summary>
public class SettingsService
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "DynamicIsland",
        "settings.json"
    );

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private AppSettings _current = new();

    public AppSettings Current => _current;

    /// <summary>Dipicu setelah pengaturan disimpan dan diterapkan.</summary>
    public event Action<AppSettings>? SettingsChanged;

    public SettingsService()
    {
        Load();
    }

    // =========================================================================
    // MUAT & SIMPAN
    // =========================================================================

    public void Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                string json = File.ReadAllText(SettingsPath);
                var loaded = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
                if (loaded != null)
                {
                    _current = loaded;
                    return;
                }
            }
        }
        catch
        {
            // Fallback ke default jika file rusak
        }

        _current = new AppSettings();
        Save(); // simpan default pertama kali
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            string json = JsonSerializer.Serialize(_current, JsonOptions);
            File.WriteAllText(SettingsPath, json);
        }
        catch { /* Simpan gagal — abaikan */ }
    }

    /// <summary>Simpan pengaturan dan terapkan ke seluruh aplikasi.</summary>
    public void Apply(AppSettings settings)
    {
        _current = settings;
        Save();
        ApplyAccentColor(settings.AccentColorHex);
        ApplyAlwaysOnTop(settings.AlwaysOnTop);
        ApplyStartup(settings.RunAtStartup);
        SettingsChanged?.Invoke(_current);
    }

    // =========================================================================
    // TERAPKAN WARNA AKSEN SECARA DINAMIS
    // =========================================================================

    public void ApplyAccentColor(string hexColor)
    {
        try
        {
            var color  = (Color)ColorConverter.ConvertFromString(hexColor);
            var brush  = new SolidColorBrush(color);
            brush.Freeze();

            // Perbarui resource global BrushAccent & ColorAccent
            if (Application.Current?.Resources != null)
            {
                Application.Current.Resources["BrushAccent"]  = brush;
                Application.Current.Resources["ColorAccent"]  = color;
            }
        }
        catch { /* Warna tidak valid — abaikan */ }
    }

    // =========================================================================
    // TERAPKAN ALWAYS ON TOP
    // =========================================================================

    public static void ApplyAlwaysOnTop(bool alwaysOnTop)
    {
        if (Application.Current?.MainWindow is Window win)
        {
            win.Topmost = alwaysOnTop;
        }
    }

    // =========================================================================
    // REGISTRY STARTUP (OPSIONAL, DEFAULT: NONAKTIF)
    // =========================================================================

    private const string StartupRegKey  = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string StartupRegName = "DynamicIsland";

    public void ApplyStartup(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(StartupRegKey, writable: true);
            if (key == null) return;

            if (enable)
            {
                string? exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                if (!string.IsNullOrEmpty(exePath))
                {
                    key.SetValue(StartupRegName, $"\"{exePath}\"");
                }
            }
            else
            {
                // Hapus entri startup jika ada
                if (key.GetValue(StartupRegName) != null)
                {
                    key.DeleteValue(StartupRegName, throwOnMissingValue: false);
                }
            }
        }
        catch { /* Registry gagal — tidak fatal */ }
    }

    public bool IsStartupEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(StartupRegKey, writable: false);
            return key?.GetValue(StartupRegName) != null;
        }
        catch { return false; }
    }

    // =========================================================================
    // WARNA PRESET
    // =========================================================================

    public static readonly string[] AccentPresets =
    [
        "#8B9CF7", // Biru Lavender (default)
        "#34D399", // Hijau Zamrud
        "#FBBF24", // Kuning Amber
        "#F87171", // Merah Koral
        "#60A5FA", // Biru Langit
        "#A78BFA", // Ungu Violet
        "#FB923C", // Oranye
        "#F472B6", // Pink
        "#FAFAFA", // Putih Netral
    ];
}
