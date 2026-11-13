using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DynamicIsland.Helpers;
using DynamicIsland.Models;
using DynamicIsland.Services;
using DynamicIsland.ViewModels;

namespace DynamicIsland.Views.Island;

public partial class IslandWindow : Window
{
    private const double CompactWidth = 210.0;
    private const double CompactHeight = 34.0;
    private const double CompactCornerRadius = 17.0;

    private const double ExpandedWidth = 430.0;
    private const double ExpandedHeight = 205.0;
    private const double ExpandedCornerRadius = 12.0;

    private TrayIconService? _trayIconService;
    private Point _mouseDownPosition;
    private bool _isMouseDown;
    private bool _hasDragged;

    public IslandWindow()
    {
        InitializeComponent();

        Loaded += OnWindowLoaded;
        SourceInitialized += OnSourceInitialized;
        Deactivated += OnWindowDeactivated;
        Closed += OnWindowClosed;

        ViewModel.StateChanged += OnStateChanged;
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        WindowHelper.SetToolWindow(this);

        _trayIconService = new TrayIconService(this);
        _trayIconService.OnTrayLeftClick += () => Dispatcher.Invoke(ToggleIslandState);
        _trayIconService.OnTrayRightClick += () => Dispatcher.Invoke(ShowContextMenuFromTray);
        _trayIconService.Initialize();
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        ResetToTopCenter();
        _ = ViewModel.InitializeServicesAsync();
    }

    private void OnWindowClosed(object? sender, EventArgs e)
    {
        _trayIconService?.Dispose();
        ViewModel.Dispose();
    }

    public void ResetToTopCenter()
    {
        WindowHelper.PositionTopCenter(this, topMargin: 12.0);
    }

    private void OnWindowDeactivated(object? sender, EventArgs e)
    {
        // Periksa apakah auto-collapse aktif di pengaturan
        if (ViewModel.Settings.Current.AutoCollapse && ViewModel.CurrentState == IslandState.Expanded)
        {
            ViewModel.Collapse();
        }
    }

    private void OnStateChanged(IslandState state)
    {
        if (state == IslandState.Expanded)
        {
            ApplyExpandedAnimation();
        }
        else
        {
            ApplyCompactAnimation();
        }
    }

    private void ApplyExpandedAnimation()
    {
        AnimationHelper.AnimateOpacity(CompactContent, 0.0, 100);

        AnimationHelper.AnimateSize(IslandContainer, ExpandedWidth, ExpandedHeight, 220, onCompleted: () =>
        {
            CompactContent.Visibility = Visibility.Collapsed;
            ExpandedContent.Visibility = Visibility.Visible;
            AnimationHelper.AnimateOpacity(ExpandedContent, 1.0, 150);
        });

        IslandContainer.CornerRadius = new CornerRadius(ExpandedCornerRadius);
        IslandContainer.Cursor = Cursors.Arrow;
    }

    private void ApplyCompactAnimation()
    {
        AnimationHelper.AnimateOpacity(ExpandedContent, 0.0, 100);

        AnimationHelper.AnimateSize(IslandContainer, CompactWidth, CompactHeight, 200, onCompleted: () =>
        {
            ExpandedContent.Visibility = Visibility.Collapsed;
            CompactContent.Visibility = Visibility.Visible;
            AnimationHelper.AnimateOpacity(CompactContent, 1.0, 140);
        });

        IslandContainer.CornerRadius = new CornerRadius(CompactCornerRadius);
        IslandContainer.Cursor = Cursors.Hand;
    }

    private void ToggleIslandState()
    {
        ViewModel.ToggleState();
    }

    // =========================================================================
    // PENANGANAN INTERAKSI MOUSE
    // =========================================================================

    private void IslandContainer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _isMouseDown = true;
        _hasDragged = false;
        _mouseDownPosition = e.GetPosition(this);
    }

    private void IslandContainer_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isMouseDown || e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        if (ViewModel.CurrentState == IslandState.Compact && !_hasDragged)
        {
            Point currentPos = e.GetPosition(this);
            Vector delta = currentPos - _mouseDownPosition;

            if (Math.Abs(delta.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(delta.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                _hasDragged = true;
                try
                {
                    DragMove();
                }
                catch
                {
                }
                _isMouseDown = false;
            }
        }
    }

    private void IslandContainer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_isMouseDown)
        {
            _isMouseDown = false;

            if (!_hasDragged && ViewModel.CurrentState == IslandState.Compact)
            {
                ViewModel.Expand();
                e.Handled = true;
            }
        }

        _hasDragged = false;
    }

    private void IslandContainer_MouseEnter(object sender, MouseEventArgs e)
    {
        if (ViewModel.CurrentState == IslandState.Compact)
        {
            IslandContainer.BorderBrush = (Brush)FindResource("BrushAccent");
        }
    }

    private void IslandContainer_MouseLeave(object sender, MouseEventArgs e)
    {
        IslandContainer.BorderBrush = (Brush)FindResource("BrushBorder");
        _isMouseDown = false;
        _hasDragged = false;
    }

    private void BtnCollapse_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Collapse();
    }

    // =========================================================================
    // NAVIGASI TAB MODUL
    // =========================================================================

    private void HideAllPanels()
    {
        PanelMedia.Visibility = Visibility.Collapsed;
        PanelSystem.Visibility = Visibility.Collapsed;
        PanelTimer.Visibility = Visibility.Collapsed;
        PanelClipboard.Visibility = Visibility.Collapsed;
        PanelLauncher.Visibility = Visibility.Collapsed;
        PanelNotif.Visibility = Visibility.Collapsed;
    }

    private void TabMedia_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SetMode(IslandActiveMode.Media);
        HideAllPanels();
        PanelMedia.Visibility = Visibility.Visible;
    }

    private void TabSystem_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SetMode(IslandActiveMode.System);
        HideAllPanels();
        PanelSystem.Visibility = Visibility.Visible;
    }

    private void TabTimer_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SetMode(IslandActiveMode.Timer);
        HideAllPanels();
        PanelTimer.Visibility = Visibility.Visible;
    }

    private void TabClipboard_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SetMode(IslandActiveMode.Clipboard);
        HideAllPanels();
        PanelClipboard.Visibility = Visibility.Visible;
    }

    private void TabLauncher_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SetMode(IslandActiveMode.Launcher);
        HideAllPanels();
        PanelLauncher.Visibility = Visibility.Visible;
    }

    private void TabNotif_Click(object sender, RoutedEventArgs e)
    {
        HideAllPanels();
        PanelNotif.Visibility = Visibility.Visible;
        // Reset badge notifikasi baru saat tab dibuka
        ViewModel.AcknowledgeNewNotification();
    }

    private void ShowContextMenuFromTray()
    {
        if (ContextMenu != null)
        {
            ContextMenu.IsOpen = true;
        }
    }

    private void MenuToggle_Click(object sender, RoutedEventArgs e) => ToggleIslandState();

    private void MenuMediaMode_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Expand();
        TabMedia_Click(sender, e);
    }

    private void MenuSystemMode_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Expand();
        TabSystem_Click(sender, e);
    }

    private void MenuTimerMode_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Expand();
        TabTimer_Click(sender, e);
    }

    private void MenuClipboardMode_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Expand();
        TabClipboard_Click(sender, e);
    }

    private void MenuLauncherMode_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Expand();
        TabLauncher_Click(sender, e);
    }

    private void OpenSettingsWindow()
    {
        var settingsVm = new SettingsViewModel(ViewModel.Settings);
        var settingsWin = new SettingsWindow(settingsVm)
        {
            Owner = this
        };
        settingsWin.ShowDialog();
    }

    private void BtnSettings_Click(object sender, RoutedEventArgs e) => OpenSettingsWindow();

    private void MenuSettings_Click(object sender, RoutedEventArgs e) => OpenSettingsWindow();

    private void MenuResetPosition_Click(object sender, RoutedEventArgs e) => ResetToTopCenter();

    private void MenuExit_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
}
