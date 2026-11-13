using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using DynamicIsland.Helpers;

namespace DynamicIsland.Services;

public class TrayIconService : IDisposable
{
    private readonly Window _window;
    private HwndSource? _hwndSource;
    private NativeMethods.NOTIFYICONDATA _notifyData;
    private bool _isCreated;

    public event Action? OnTrayLeftClick;
    public event Action? OnTrayRightClick;

    public TrayIconService(Window window)
    {
        _window = window;
    }

    public void Initialize()
    {
        var helper = new WindowInteropHelper(_window);
        _hwndSource = HwndSource.FromHwnd(helper.Handle);
        _hwndSource?.AddHook(WndProc);

        IntPtr iconHandle = NativeMethods.LoadIcon(IntPtr.Zero, NativeMethods.IDI_APPLICATION);

        _notifyData = new NativeMethods.NOTIFYICONDATA
        {
            cbSize = Marshal.SizeOf<NativeMethods.NOTIFYICONDATA>(),
            hWnd = helper.Handle,
            uID = 1001,
            uFlags = NativeMethods.NIF_MESSAGE | NativeMethods.NIF_ICON | NativeMethods.NIF_TIP,
            uCallbackMessage = NativeMethods.WM_TRAYICON,
            hIcon = iconHandle,
            szTip = "Dynamic Island untuk Windows"
        };

        _isCreated = NativeMethods.Shell_NotifyIcon(NativeMethods.NIM_ADD, ref _notifyData);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_TRAYICON)
        {
            int action = lParam.ToInt32();
            if (action == NativeMethods.WM_LBUTTONUP)
            {
                OnTrayLeftClick?.Invoke();
                handled = true;
            }
            else if (action == NativeMethods.WM_RBUTTONUP)
            {
                OnTrayRightClick?.Invoke();
                handled = true;
            }
        }

        return IntPtr.Zero;
    }

    public void Dispose()
    {
        if (_isCreated)
        {
            NativeMethods.Shell_NotifyIcon(NativeMethods.NIM_DELETE, ref _notifyData);
            _isCreated = false;
        }

        if (_hwndSource != null)
        {
            _hwndSource.RemoveHook(WndProc);
            _hwndSource = null;
        }
    }
}
