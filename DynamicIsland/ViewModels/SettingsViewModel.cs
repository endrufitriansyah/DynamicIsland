using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicIsland.Models;
using DynamicIsland.Services;

namespace DynamicIsland.ViewModels;

public class AccentColorOption
{
    public string Name { get; set; } = string.Empty;
    public string Hex { get; set; } = string.Empty;
}

public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settingsService;

    [ObservableProperty]
    private string _accentColorHex = "#8B9CF7";

    [ObservableProperty]
    private bool _alwaysOnTop = true;

    [ObservableProperty]
    private bool _autoCollapse = true;

    [ObservableProperty]
    private double _topMargin = 12.0;

    [ObservableProperty]
    private bool _enableMediaModule = true;

    [ObservableProperty]
    private bool _enableSystemModule = true;

    [ObservableProperty]
    private bool _enableTimerModule = true;

    [ObservableProperty]
    private bool _enableClipboardModule = true;

    [ObservableProperty]
    private bool _enableLauncherModule = true;

    [ObservableProperty]
    private bool _enableNotifModule = true;

    [ObservableProperty]
    private bool _runAtStartup = false;

    public ObservableCollection<AccentColorOption> ColorOptions { get; } = new()
    {
        new AccentColorOption { Name = "Lavender (Default)", Hex = "#8B9CF7" },
        new AccentColorOption { Name = "Zamrud", Hex = "#34D399" },
        new AccentColorOption { Name = "Amber", Hex = "#FBBF24" },
        new AccentColorOption { Name = "Koral", Hex = "#F87171" },
        new AccentColorOption { Name = "Langit", Hex = "#60A5FA" },
        new AccentColorOption { Name = "Violet", Hex = "#A78BFA" },
        new AccentColorOption { Name = "Oranye", Hex = "#FB923C" },
        new AccentColorOption { Name = "Pink", Hex = "#F472B6" },
        new AccentColorOption { Name = "Putih Netral", Hex = "#FAFAFA" }
    };

    public SettingsViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;
        LoadFromCurrentSettings();
    }

    private void LoadFromCurrentSettings()
    {
        var s = _settingsService.Current;
        AccentColorHex = s.AccentColorHex;
        AlwaysOnTop = s.AlwaysOnTop;
        AutoCollapse = s.AutoCollapse;
        TopMargin = s.TopMargin;

        EnableMediaModule = s.EnableMediaModule;
        EnableSystemModule = s.EnableSystemModule;
        EnableTimerModule = s.EnableTimerModule;
        EnableClipboardModule = s.EnableClipboardModule;
        EnableLauncherModule = s.EnableLauncherModule;
        EnableNotifModule = s.EnableNotifModule;

        RunAtStartup = _settingsService.IsStartupEnabled();
    }

    [RelayCommand]
    public void SelectColor(string hex)
    {
        if (!string.IsNullOrEmpty(hex))
        {
            AccentColorHex = hex;
            _settingsService.ApplyAccentColor(hex);
            SaveCurrent();
        }
    }

    [RelayCommand]
    public void SaveCurrent()
    {
        var s = _settingsService.Current;
        s.AccentColorHex = AccentColorHex;
        s.AlwaysOnTop = AlwaysOnTop;
        s.AutoCollapse = AutoCollapse;
        s.TopMargin = TopMargin;

        s.EnableMediaModule = EnableMediaModule;
        s.EnableSystemModule = EnableSystemModule;
        s.EnableTimerModule = EnableTimerModule;
        s.EnableClipboardModule = EnableClipboardModule;
        s.EnableLauncherModule = EnableLauncherModule;
        s.EnableNotifModule = EnableNotifModule;

        s.RunAtStartup = RunAtStartup;

        _settingsService.Apply(s);
    }

    [RelayCommand]
    public void ResetDefaults()
    {
        AccentColorHex = "#8B9CF7";
        AlwaysOnTop = true;
        AutoCollapse = true;
        TopMargin = 12.0;

        EnableMediaModule = true;
        EnableSystemModule = true;
        EnableTimerModule = true;
        EnableClipboardModule = true;
        EnableLauncherModule = true;
        EnableNotifModule = true;

        RunAtStartup = false;

        SaveCurrent();
    }
}
