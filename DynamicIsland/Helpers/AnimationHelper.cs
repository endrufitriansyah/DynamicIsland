using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace DynamicIsland.Helpers;

public static class AnimationHelper
{
    private static readonly IEasingFunction SubtleEasing = new CubicEase { EasingMode = EasingMode.EaseOut };

    /// <summary>
    /// Menerapkan animasi perubahan ukuran yang halus, cepat, dan terukur tanpa efek lentur (bounce) yang berlebihan.
    /// </summary>
    public static void AnimateSize(FrameworkElement element, double targetWidth, double targetHeight, double durationMs = 220, Action? onCompleted = null)
    {
        var widthAnim = new DoubleAnimation
        {
            To = targetWidth,
            Duration = TimeSpan.FromMilliseconds(durationMs),
            EasingFunction = SubtleEasing
        };

        var heightAnim = new DoubleAnimation
        {
            To = targetHeight,
            Duration = TimeSpan.FromMilliseconds(durationMs),
            EasingFunction = SubtleEasing
        };

        if (onCompleted != null)
        {
            heightAnim.Completed += (s, e) => onCompleted();
        }

        element.BeginAnimation(FrameworkElement.WidthProperty, widthAnim);
        element.BeginAnimation(FrameworkElement.HeightProperty, heightAnim);
    }

    /// <summary>
    /// Animasi transisi opacity konten halus saat beralih antara status compact dan expanded.
    /// </summary>
    public static void AnimateOpacity(UIElement element, double targetOpacity, double durationMs = 150)
    {
        var anim = new DoubleAnimation
        {
            To = targetOpacity,
            Duration = TimeSpan.FromMilliseconds(durationMs),
            EasingFunction = SubtleEasing
        };
        element.BeginAnimation(UIElement.OpacityProperty, anim);
    }
}
