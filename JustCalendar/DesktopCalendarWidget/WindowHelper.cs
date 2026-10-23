using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace DesktopCalendarWidget;

public static class WindowHelper
{
    private const int HWND_BOTTOM = 1;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOACTIVATE = 0x0010;

    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_APPWINDOW = 0x00040000;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
    private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    /// <summary>
    /// Excludes a window from the Alt+Tab list by marking it as a tool window.
    /// Call from SourceInitialized (before the window is first shown).
    /// </summary>
    public static void HideFromAltTab(Window window)
    {
        try
        {
            IntPtr hWnd = new WindowInteropHelper(window).Handle;
            if (hWnd != IntPtr.Zero)
            {
                long exStyle = GetWindowLongPtr(hWnd, GWL_EXSTYLE).ToInt64();
                exStyle = (exStyle | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW;
                SetWindowLongPtr(hWnd, GWL_EXSTYLE, new IntPtr(exStyle));
            }
        }
        catch
        {
            // Ignore Win32 errors
        }
    }

    public static void SendToDesktopBottom(Window window)
    {
        try
        {
            IntPtr hWnd = new WindowInteropHelper(window).Handle;
            if (hWnd != IntPtr.Zero)
            {
                SetWindowPos(hWnd, (IntPtr)HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
            }
        }
        catch
        {
            // Ignore Win32 errors
        }
    }
}