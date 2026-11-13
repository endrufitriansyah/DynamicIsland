using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using DynamicIsland.Controls;
using DynamicIsland.Models;

namespace DynamicIsland.Services;

public class LauncherService
{
    private readonly string _configFilePath;
    public ObservableCollection<LauncherAppItem> Apps { get; } = new();

    public LauncherService()
    {
        string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DynamicIsland");
        Directory.CreateDirectory(appDataFolder);
        _configFilePath = Path.Combine(appDataFolder, "launcher.json");

        LoadApps();
    }

    // Versi konfigurasi — naikkan angka ini untuk memaksa regenerasi ulang daftar default
    private const int ConfigVersion = 2;

    public void LoadApps()
    {
        Apps.Clear();

        if (File.Exists(_configFilePath))
        {
            try
            {
                string json = File.ReadAllText(_configFilePath);
                var items = JsonSerializer.Deserialize<List<LauncherAppItem>>(json);
                if (items != null && items.Count > 0)
                {
                    // Cek apakah konfigurasi lama (tidak punya WorkingDirectory / versi lama)
                    bool needsMigration = items.TrueForAll(x => string.IsNullOrEmpty(x.WorkingDirectory));
                    if (!needsMigration)
                    {
                        foreach (var item in items)
                        {
                            Apps.Add(item);
                        }
                        return;
                    }
                    // Jika semua entri tidak punya WorkingDirectory → migrasi ke default baru
                }
            }
            catch
            {
                // Fallback ke daftar aplikasi bawaan jika file JSON rusak
            }
        }

        // Resolusi path sistem agar selalu terbuka meski CWD berbeda
        string sysRoot   = Environment.GetFolderPath(Environment.SpecialFolder.System);
        string winRoot   = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        string userHome  = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        string notepadPath  = Path.Combine(winRoot,  "notepad.exe");
        string explorerPath = Path.Combine(winRoot,  "explorer.exe");
        string cmdPath      = Path.Combine(sysRoot,  "cmd.exe");
        string taskmgrPath  = Path.Combine(sysRoot,  "taskmgr.exe");
        string calcPath     = Path.Combine(sysRoot,  "calc.exe");

        // Daftar Aplikasi Bawaan — semua pakai full path + WorkingDirectory rumah pengguna
        Apps.Add(new LauncherAppItem { Name = "Berkas",     Path = explorerPath, WorkingDirectory = userHome, IconKind = LucideIconKind.Launcher });
        Apps.Add(new LauncherAppItem { Name = "Catatan",    Path = notepadPath,  WorkingDirectory = userHome, IconKind = LucideIconKind.Clipboard });
        Apps.Add(new LauncherAppItem { Name = "Terminal",   Path = cmdPath,      WorkingDirectory = userHome, IconKind = LucideIconKind.Cpu });
        Apps.Add(new LauncherAppItem { Name = "Tugas",      Path = taskmgrPath,  WorkingDirectory = userHome, IconKind = LucideIconKind.Settings });
        Apps.Add(new LauncherAppItem { Name = "Kalkulator", Path = calcPath,     WorkingDirectory = userHome, IconKind = LucideIconKind.Timer });

        SaveApps();
    }

    public void SaveApps()
    {
        try
        {
            string json = JsonSerializer.Serialize(Apps, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configFilePath, json);
        }
        catch
        {
        }
    }

    public bool Launch(LauncherAppItem app)
    {
        try
        {
            // Gunakan WorkingDirectory dari data app; fallback ke folder home pengguna
            string workDir = string.IsNullOrEmpty(app.WorkingDirectory)
                ? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                : app.WorkingDirectory;

            // Pastikan direktori kerja benar-benar ada sebelum dipakai
            if (!Directory.Exists(workDir))
                workDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            var startInfo = new ProcessStartInfo
            {
                FileName         = app.Path,
                Arguments        = app.Arguments ?? string.Empty,
                UseShellExecute  = true,
                WorkingDirectory = workDir
            };

            Process.Start(startInfo);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void AddApp(string name, string path, LucideIconKind iconKind = LucideIconKind.Launcher)
    {
        Apps.Add(new LauncherAppItem
        {
            Name = name,
            Path = path,
            IconKind = iconKind
        });
        SaveApps();
    }

    public void RemoveApp(LauncherAppItem app)
    {
        if (Apps.Remove(app))
        {
            SaveApps();
        }
    }
}
