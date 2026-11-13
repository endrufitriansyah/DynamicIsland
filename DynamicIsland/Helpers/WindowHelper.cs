using System;
using System.Windows;
using System.Windows.Interop;

namespace DynamicIsland.Helpers;

public static class WindowHelper
{
    /// <summary>
    /// Menetapkan window style sebagai ToolWindow agar tidak muncul di Alt-Tab atau Taskbar.
    /// </summary>
    public static void SetToolWindow(Window window)
    {
        var helper = new WindowInteropHelper(window);
        int exStyle = NativeMethods.GetWindowLong(helper.Handle, NativeMethods.GWL_EXSTYLE);
        NativeMethods.SetWindowLong(helper.Handle, NativeMethods.GWL_EXSTYLE, exStyle | NativeMethods.WS_EX_TOOLWINDOW);
    }

    /// <summary>
    /// Menempatkan window di bagian tengah atas (Top-Center) dari area kerja monitor utama.
    /// </summary>
    public static void PositionTopCenter(Window window, double topMargin = 12.0)
    {
        Rect workArea = SystemParameters.WorkArea;
        window.Left = workArea.Left + ((workArea.Width - window.Width) / 2.0);
        window.Top = workArea.Top + topMargin;
    }

    /// <summary>
    /// Memungkinkan drag window secara natural dengan Win32 SendMessage.
    /// </summary>
    public static void DragMoveWindow(Window window)
    {
        var helper = new WindowInteropHelper(window);
        NativeMethods.ReleaseCapture();
        NativeMethods.SendMessage(helper.Handle, NativeMethods.WM_NCLBUTTONDOWN, (IntPtr)NativeMethods.HT_CAPTION, IntPtr.Zero);
    }
}
