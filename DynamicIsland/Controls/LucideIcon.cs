using System;
using System.Windows;
using System.Windows.Media;

namespace DynamicIsland.Controls;

public enum LucideIconKind
{
    Music,
    Play,
    Pause,
    SkipBack,
    SkipForward,
    Battery,
    BatteryCharging,
    Cpu,
    Wifi,
    Bell,
    Timer,
    Clipboard,
    Settings,
    Launcher,
    Calendar,
    Download,
    Volume,
    Search,
    Close,
    ChevronDown,
    ChevronUp,
    Refresh
}

/// <summary>
/// Kontrol visual ringan untuk merender ikon vektor Lucide dengan stroke round yang tajam.
/// Menghindari overhead visual tree dengan merender langsung menggunakan DrawingContext.
/// </summary>
public class LucideIcon : FrameworkElement
{
    public static readonly DependencyProperty KindProperty =
        DependencyProperty.Register(
            nameof(Kind),
            typeof(LucideIconKind),
            typeof(LucideIcon),
            new FrameworkPropertyMetadata(LucideIconKind.Music, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ForegroundProperty =
        DependencyProperty.Register(
            nameof(Foreground),
            typeof(Brush),
            typeof(LucideIcon),
            new FrameworkPropertyMetadata(Brushes.White, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty StrokeThicknessProperty =
        DependencyProperty.Register(
            nameof(StrokeThickness),
            typeof(double),
            typeof(LucideIcon),
            new FrameworkPropertyMetadata(1.75, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.Register(
            nameof(Size),
            typeof(double),
            typeof(LucideIcon),
            new FrameworkPropertyMetadata(18.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnSizeChanged));

    public LucideIconKind Kind
    {
        get => (LucideIconKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    public Brush Foreground
    {
        get => (Brush)GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public LucideIcon()
    {
        Width = Size;
        Height = Size;
        SnapsToDevicePixels = true;
    }

    private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LucideIcon icon && e.NewValue is double newSize)
        {
            icon.Width = newSize;
            icon.Height = newSize;
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return new Size(Size, Size);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        string resourceKey = GetResourceKey(Kind);
        if (Application.Current.TryFindResource(resourceKey) is not Geometry geometry)
        {
            return;
        }

        double width = ActualWidth > 0 ? ActualWidth : Size;
        double height = ActualHeight > 0 ? ActualHeight : Size;

        if (width <= 0 || height <= 0) return;

        double scaleX = width / 24.0;
        double scaleY = height / 24.0;

        dc.PushTransform(new ScaleTransform(scaleX, scaleY));

        var pen = new Pen(Foreground, StrokeThickness)
        {
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round,
            LineJoin = PenLineJoin.Round
        };
        pen.Freeze();

        // Untuk tombol play, berikan sedikit fill halus jika diinginkan, atau stroke outline
        Brush? fill = (Kind == LucideIconKind.Play) ? Foreground : null;

        dc.DrawGeometry(fill, pen, geometry);

        dc.Pop();
    }

    private static string GetResourceKey(LucideIconKind kind) => kind switch
    {
        LucideIconKind.Music => "IconGeometryMusic",
        LucideIconKind.Play => "IconGeometryPlay",
        LucideIconKind.Pause => "IconGeometryPause",
        LucideIconKind.SkipBack => "IconGeometrySkipBack",
        LucideIconKind.SkipForward => "IconGeometrySkipForward",
        LucideIconKind.Battery => "IconGeometryBattery",
        LucideIconKind.BatteryCharging => "IconGeometryBatteryCharging",
        LucideIconKind.Cpu => "IconGeometryCpu",
        LucideIconKind.Wifi => "IconGeometryWifi",
        LucideIconKind.Bell => "IconGeometryBell",
        LucideIconKind.Timer => "IconGeometryTimer",
        LucideIconKind.Clipboard => "IconGeometryClipboard",
        LucideIconKind.Settings => "IconGeometrySettings",
        LucideIconKind.Launcher => "IconGeometryLauncher",
        LucideIconKind.Calendar => "IconGeometryCalendar",
        LucideIconKind.Download => "IconGeometryDownload",
        LucideIconKind.Volume => "IconGeometryVolume",
        LucideIconKind.Search => "IconGeometrySearch",
        LucideIconKind.Close => "IconGeometryClose",
        LucideIconKind.ChevronDown => "IconGeometryChevronDown",
        LucideIconKind.ChevronUp => "IconGeometryChevronUp",
        LucideIconKind.Refresh => "IconGeometryRefresh",
        _ => "IconGeometryMusic"
    };
}
