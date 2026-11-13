using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicIsland.Controls;
using DynamicIsland.Models;
using DynamicIsland.Services;

namespace DynamicIsland.ViewModels;

public partial class IslandViewModel : ObservableObject, IDisposable
{
    private readonly MediaService _mediaService;
    private readonly SystemMonitorService _systemService;
    private readonly BatteryService _batteryService;
    private readonly NetworkService _networkService;
    private readonly TimerService _timerService;
    private readonly ClipboardService _clipboardService;
    private readonly LauncherService _launcherService;
    private readonly CalendarService _calendarService;
    private readonly NotificationService _notificationService;
    private readonly SettingsService _settingsService;
    private readonly System.Timers.Timer _clockTimer;
    private bool _isDisposed;

    [ObservableProperty]
    private IslandState _currentState = IslandState.Compact;

    [ObservableProperty]
    private IslandActiveMode _activeMode = IslandActiveMode.Media;

    [ObservableProperty]
    private bool _isLocked = false;

    [ObservableProperty]
    private bool _alwaysOnTop = true;

    // Status ringkas (Compact)
    [ObservableProperty]
    private string _compactLeftText = "Dynamic Island";

    [ObservableProperty]
    private string _compactRightText = "";

    [ObservableProperty]
    private LucideIconKind _compactIconKind = LucideIconKind.Launcher;

    [ObservableProperty]
    private bool _hasActiveMedia = false;

    // Status Media (Expanded)
    [ObservableProperty]
    private string _mediaTitle = "Tidak Ada Media Aktif";

    [ObservableProperty]
    private string _mediaArtist = "Mendukung Spotify & Pemutar Windows";

    [ObservableProperty]
    private BitmapImage? _albumArtwork;

    [ObservableProperty]
    private double _mediaProgress = 0;

    [ObservableProperty]
    private string _mediaCurrentTime = "0:00";

    [ObservableProperty]
    private string _mediaTotalTime = "0:00";

    [ObservableProperty]
    private bool _isPlaying = false;

    [ObservableProperty]
    private string _mediaAppDisplayName = "MUSIK";

    // Metrik Sistem (Expanded)
    [ObservableProperty]
    private string _cpuUsageText = "0%";

    [ObservableProperty]
    private double _cpuUsageValue = 0;

    [ObservableProperty]
    private string _ramUsageText = "0 GB";

    [ObservableProperty]
    private double _ramUsageValue = 0;

    [ObservableProperty]
    private string _batteryStatusText = "Daya Listrik AC";

    [ObservableProperty]
    private int _batteryPercent = 100;

    [ObservableProperty]
    private bool _isBatteryCharging = true;

    [ObservableProperty]
    private bool _hasBattery = false;

    // Metrik Jaringan (Expanded)
    [ObservableProperty]
    private string _networkDownloadText = "0 B/s";

    [ObservableProperty]
    private string _networkUploadText = "0 B/s";

    [ObservableProperty]
    private bool _isNetworkConnected = true;

    // =========================================================================
    // MODUL PRODUKTIVITAS (PHASE 5)
    // =========================================================================

    // Pomodoro
    [ObservableProperty]
    private string _pomodoroTimeText = "25:00";

    [ObservableProperty]
    private double _pomodoroProgress = 0;

    [ObservableProperty]
    private string _pomodoroSessionLabel = "Fokus Kerja (25m)";

    [ObservableProperty]
    private bool _isPomodoroRunning = false;

    // Stopwatch
    [ObservableProperty]
    private string _stopwatchTimeText = "00:00:00";

    [ObservableProperty]
    private bool _isStopwatchRunning = false;

    // Kalender
    [ObservableProperty]
    private string _calendarFormattedDate = string.Empty;

    // Papan Klip
    public ObservableCollection<ClipboardItem> ClipboardHistory => _clipboardService.History;

    // Peluncur Cepat
    public ObservableCollection<LauncherAppItem> LauncherApps => _launcherService.Apps;

    // =========================================================================
    // MODUL NOTIFIKASI (PHASE 6)
    // =========================================================================

    public ObservableCollection<NotificationItem> Notifications => _notificationService.Notifications;

    [ObservableProperty]
    private bool _isNotificationAccessGranted = false;

    [ObservableProperty]
    private string _notificationAccessText = "Belum diminta";

    [ObservableProperty]
    private bool _hasNewNotification = false;

    [ObservableProperty]
    private string _latestNotificationTitle = string.Empty;

    [ObservableProperty]
    private string _latestNotificationApp = string.Empty;

    // Modul aktif dari Pengaturan (Phase 7)
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

    public SettingsService Settings => _settingsService;

    public event Action<IslandState>? StateChanged;
    public event Action<NotificationItem>? NewNotificationArrived;

    public IslandViewModel()
    {
        _mediaService = new MediaService();
        _systemService = new SystemMonitorService(1500);
        _batteryService = new BatteryService();
        _networkService = new NetworkService(1500);
        _timerService = new TimerService();
        _clipboardService = new ClipboardService();
        _launcherService = new LauncherService();
        _calendarService = new CalendarService();
        _notificationService = new NotificationService();
        _settingsService = new SettingsService();

        ApplyLoadedSettings();
        _settingsService.SettingsChanged += OnSettingsChanged;

        _calendarFormattedDate = _calendarService.CurrentFormattedDate;

        _clockTimer = new System.Timers.Timer(1000);
        _clockTimer.Elapsed += (s, e) =>
        {
            App.Current?.Dispatcher.Invoke(() =>
            {
                CompactRightText = DateTime.Now.ToString("HH:mm");
            });
        };
        _clockTimer.Start();
        CompactRightText = DateTime.Now.ToString("HH:mm");

        SubscribeToServices();
    }

    public async Task InitializeServicesAsync()
    {
        await _mediaService.InitializeAsync();
        _systemService.Start();
        _batteryService.Initialize();
        _networkService.Start();
        _clipboardService.Start();

        // Inisialisasi notifikasi — meminta izin pengguna jika belum diberikan
        await _notificationService.InitializeAsync();
        App.Current?.Dispatcher.Invoke(() =>
        {
            IsNotificationAccessGranted = _notificationService.IsAccessGranted;
            NotificationAccessText = _notificationService.AccessStatusText;
        });
    }

    private void ApplyLoadedSettings()
    {
        var s = _settingsService.Current;
        _settingsService.ApplyAccentColor(s.AccentColorHex);
        AlwaysOnTop = s.AlwaysOnTop;

        EnableMediaModule = s.EnableMediaModule;
        EnableSystemModule = s.EnableSystemModule;
        EnableTimerModule = s.EnableTimerModule;
        EnableClipboardModule = s.EnableClipboardModule;
        EnableLauncherModule = s.EnableLauncherModule;
        EnableNotifModule = s.EnableNotifModule;
    }

    private void OnSettingsChanged(AppSettings s)
    {
        App.Current?.Dispatcher.Invoke(() =>
        {
            AlwaysOnTop = s.AlwaysOnTop;
            EnableMediaModule = s.EnableMediaModule;
            EnableSystemModule = s.EnableSystemModule;
            EnableTimerModule = s.EnableTimerModule;
            EnableClipboardModule = s.EnableClipboardModule;
            EnableLauncherModule = s.EnableLauncherModule;
            EnableNotifModule = s.EnableNotifModule;
        });
    }
    private void SubscribeToServices()
    {
        _mediaService.MediaChanged += OnMediaChanged;
        _mediaService.TimelineUpdated += OnTimelineUpdated;
        _systemService.MetricsUpdated += OnSystemMetricsUpdated;
        _batteryService.BatteryChanged += OnBatteryChanged;
        _networkService.NetworkUpdated += OnNetworkUpdated;

        // Pomodoro & Stopwatch
        _timerService.PomodoroTicked += OnPomodoroTicked;
        _timerService.PomodoroFinished += OnPomodoroFinished;
        _timerService.StopwatchTicked += OnStopwatchTicked;

        // Notifikasi (Phase 6)
        _notificationService.NotificationReceived += OnNotificationReceived;
        _notificationService.AccessStatusChanged  += OnNotificationAccessChanged;
    }

    private void OnNotificationReceived(NotificationItem item)
    {
        // Sudah di-dispatch di DispatcherTimer (UI thread) — langsung update state
        LatestNotificationTitle = item.Title;
        LatestNotificationApp   = item.AppLabel;
        HasNewNotification      = true;

        // Infokan ke Window agar bisa ekspansi singkat (opsional)
        NewNotificationArrived?.Invoke(item);
    }

    private void OnNotificationAccessChanged(bool granted)
    {
        App.Current?.Dispatcher.Invoke(() =>
        {
            IsNotificationAccessGranted = granted;
            NotificationAccessText = _notificationService.AccessStatusText;
        });
    }

    private void OnMediaChanged(MediaTrackInfo info)
    {
        App.Current?.Dispatcher.Invoke(() =>
        {
            MediaTitle = info.Title;
            MediaArtist = info.Artist;
            AlbumArtwork = info.Thumbnail;
            IsPlaying = info.IsPlaying;
            HasActiveMedia = info.HasMedia;
            MediaAppDisplayName = info.AppDisplayName;

            MediaCurrentTime = FormatTime(info.Position);
            MediaTotalTime = FormatTime(info.Duration);
            MediaProgress = info.ProgressPercentage;

            UpdateCompactState();
        });
    }

    private void OnTimelineUpdated(TimeSpan position, TimeSpan duration, double progressPercentage)
    {
        App.Current?.Dispatcher.Invoke(() =>
        {
            MediaCurrentTime = FormatTime(position);
            MediaTotalTime = FormatTime(duration);
            MediaProgress = progressPercentage;
        });
    }

    private void OnSystemMetricsUpdated(SystemMetrics metrics)
    {
        App.Current?.Dispatcher.Invoke(() =>
        {
            CpuUsageText = $"{metrics.CpuUsagePercent:F0}%";
            CpuUsageValue = metrics.CpuUsagePercent;
            RamUsageText = $"{metrics.RamUsedGb:F1} / {metrics.RamTotalGb:F0} GB";
            RamUsageValue = metrics.RamUsagePercent;
        });
    }

    private void OnBatteryChanged(BatteryInfo info)
    {
        App.Current?.Dispatcher.Invoke(() =>
        {
            HasBattery = info.HasBattery;
            BatteryPercent = info.Percentage;
            IsBatteryCharging = info.IsCharging;
            BatteryStatusText = info.StatusText;
        });
    }

    private void OnNetworkUpdated(NetworkMetrics metrics)
    {
        App.Current?.Dispatcher.Invoke(() =>
        {
            NetworkDownloadText = metrics.DownloadSpeedFormatted;
            NetworkUploadText = metrics.UploadSpeedFormatted;
            IsNetworkConnected = metrics.IsConnected;
        });
    }

    private void OnPomodoroTicked(TimeSpan remaining, double progress)
    {
        App.Current?.Dispatcher.Invoke(() =>
        {
            PomodoroTimeText = $"{remaining.Minutes:D2}:{remaining.Seconds:D2}";
            PomodoroProgress = progress;
            IsPomodoroRunning = _timerService.PomodoroState == PomodoroState.Running;
        });
    }

    private void OnPomodoroFinished(PomodoroSessionType sessionType)
    {
        App.Current?.Dispatcher.Invoke(() =>
        {
            PomodoroSessionLabel = _timerService.CurrentSession == PomodoroSessionType.Work ? "Fokus Kerja (25m)" : "Jeda Istirahat (5m)";
            IsPomodoroRunning = false;
        });
    }

    private void OnStopwatchTicked(TimeSpan elapsed)
    {
        App.Current?.Dispatcher.Invoke(() =>
        {
            StopwatchTimeText = $"{(int)elapsed.TotalHours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
            IsStopwatchRunning = _timerService.IsStopwatchRunning;
        });
    }

    private void UpdateCompactState()
    {
        if (HasActiveMedia)
        {
            CompactIconKind = LucideIconKind.Music;
            CompactLeftText = MediaTitle.Length > 16 ? MediaTitle[..14] + "…" : MediaTitle;
        }
        else
        {
            CompactIconKind = LucideIconKind.Launcher;
            CompactLeftText = "Dynamic Island";
        }

        CompactRightText = DateTime.Now.ToString("HH:mm");
    }

    private static string FormatTime(TimeSpan time)
    {
        return time.TotalHours >= 1
            ? $"{(int)time.TotalHours}:{time.Minutes:D2}:{time.Seconds:D2}"
            : $"{time.Minutes}:{time.Seconds:D2}";
    }

    // =========================================================================
    // PERINTAH KONTROL (RELAY COMMANDS)
    // =========================================================================

    [RelayCommand]
    public void ToggleState()
    {
        CurrentState = (CurrentState == IslandState.Compact) ? IslandState.Expanded : IslandState.Compact;
        StateChanged?.Invoke(CurrentState);
    }

    [RelayCommand]
    public void Expand()
    {
        if (CurrentState != IslandState.Expanded)
        {
            CurrentState = IslandState.Expanded;
            StateChanged?.Invoke(CurrentState);
        }
    }

    [RelayCommand]
    public void Collapse()
    {
        if (CurrentState != IslandState.Compact)
        {
            CurrentState = IslandState.Compact;
            StateChanged?.Invoke(CurrentState);
        }
    }

    [RelayCommand]
    public void SetMode(IslandActiveMode mode)
    {
        ActiveMode = mode;
    }

    // Kontrol Media
    [RelayCommand]
    public async Task TogglePlayPauseAsync() => await _mediaService.TogglePlayPauseAsync();

    [RelayCommand]
    public async Task SkipNextAsync() => await _mediaService.SkipNextAsync();

    [RelayCommand]
    public async Task SkipPreviousAsync() => await _mediaService.SkipPreviousAsync();

    // Kontrol Pomodoro
    [RelayCommand]
    public void TogglePomodoro()
    {
        if (_timerService.PomodoroState == PomodoroState.Running)
        {
            _timerService.PausePomodoro();
            IsPomodoroRunning = false;
        }
        else
        {
            _timerService.StartPomodoro();
            IsPomodoroRunning = true;
        }
    }

    [RelayCommand]
    public void ResetPomodoro()
    {
        _timerService.ResetPomodoro();
        PomodoroTimeText = $"{_timerService.PomodoroRemaining.Minutes:D2}:{_timerService.PomodoroRemaining.Seconds:D2}";
        PomodoroProgress = 0;
        IsPomodoroRunning = false;
    }

    [RelayCommand]
    public void SetPomodoroSession(string sessionType)
    {
        if (sessionType == "Work")
        {
            _timerService.SetPomodoroSession(PomodoroSessionType.Work);
            PomodoroSessionLabel = "Fokus Kerja (25m)";
        }
        else if (sessionType == "ShortBreak")
        {
            _timerService.SetPomodoroSession(PomodoroSessionType.ShortBreak);
            PomodoroSessionLabel = "Jeda Pendek (5m)";
        }
        else if (sessionType == "LongBreak")
        {
            _timerService.SetPomodoroSession(PomodoroSessionType.LongBreak);
            PomodoroSessionLabel = "Jeda Panjang (15m)";
        }
        PomodoroTimeText = $"{_timerService.PomodoroRemaining.Minutes:D2}:{_timerService.PomodoroRemaining.Seconds:D2}";
        PomodoroProgress = 0;
        IsPomodoroRunning = false;
    }

    // Kontrol Stopwatch
    [RelayCommand]
    public void ToggleStopwatch()
    {
        if (_timerService.IsStopwatchRunning)
        {
            _timerService.PauseStopwatch();
            IsStopwatchRunning = false;
        }
        else
        {
            _timerService.StartStopwatch();
            IsStopwatchRunning = true;
        }
    }

    [RelayCommand]
    public void ResetStopwatch()
    {
        _timerService.ResetStopwatch();
        StopwatchTimeText = "00:00:00";
        IsStopwatchRunning = false;
    }

    // Papan Klip
    [RelayCommand]
    public void CopyClipboardItem(ClipboardItem item)
    {
        if (item != null)
        {
            _clipboardService.CopyToClipboard(item.Text);
        }
    }

    [RelayCommand]
    public void ClearClipboard()
    {
        _clipboardService.ClearHistory();
    }

    // Peluncur Aplikasi
    [RelayCommand]
    public void LaunchApp(LauncherAppItem app)
    {
        if (app != null)
        {
            _launcherService.Launch(app);
        }
    }

    // Notifikasi (Phase 6)
    [RelayCommand]
    public void DismissNotification(NotificationItem item)
    {
        if (item != null)
        {
            _notificationService.DismissNotification(item.Id);
        }
    }

    [RelayCommand]
    public void ClearAllNotifications()
    {
        _notificationService.ClearAllNotifications();
        HasNewNotification = false;
    }

    [RelayCommand]
    public void AcknowledgeNewNotification()
    {
        HasNewNotification = false;
    }

    [RelayCommand]
    public void ExitApplication()
    {
        System.Windows.Application.Current.Shutdown();
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        _clockTimer.Stop();
        _clockTimer.Dispose();

        _mediaService.Dispose();
        _systemService.Dispose();
        _batteryService.Dispose();
        _networkService.Dispose();
        _timerService.Dispose();
        _clipboardService.Dispose();
        _notificationService.Dispose();
    }
}
