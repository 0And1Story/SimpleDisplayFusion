using System.Runtime.InteropServices;
using System.Text;

namespace SimpleDisplayFusion;

internal static class WindowMover
{
    internal static bool IsApplicationWindow(nint window)
    {
        if (Native.GetAncestor(window, Native.Root) != window || Native.GetWindow(window, Native.Owner) != 0)
            return false;

        uint style = Native.GetWindowLong(window, Native.WindowStyle);
        uint extended = Native.GetWindowLong(window, Native.ExtendedWindowStyle);
        if ((style & Native.Child) != 0 || (extended & (Native.ToolWindow | Native.NoActivateStyle)) != 0)
            return false;
        if ((style & Native.Caption) != Native.Caption && (style & Native.ThickFrame) == 0
            && (extended & Native.AppWindow) == 0)
            return false;

        var name = new StringBuilder(256);
        if (Native.GetClassName(window, name, name.Capacity) == 0)
            return false;
        return name.ToString() is not ("#32770" or "Progman" or "WorkerW" or "Shell_TrayWnd"
            or "Shell_SecondaryTrayWnd" or "TaskSwitcherWnd" or "MultitaskingViewFrame"
            or "XamlExplorerHostIslandWindow");
    }

    // True when a move was requested, even if the target app rejected the request.
    internal static bool RequestMove(nint window, nint targetMonitor)
    {
        if (!Native.IsWindowVisible(window) || Native.IsIconic(window) || !IsApplicationWindow(window))
            return false;
        if (Native.DwmGetWindowAttribute(window, Native.DwmCloaked, out int cloaked, sizeof(int)) == 0
            && cloaked != 0)
            return false;

        nint sourceMonitor = Native.MonitorFromWindow(window, Native.MonitorDefaultToNearest);
        if (sourceMonitor == 0 || sourceMonitor == targetMonitor)
            return false;

        var source = new Native.MonitorInfo { Size = Marshal.SizeOf<Native.MonitorInfo>() };
        var target = new Native.MonitorInfo { Size = source.Size };
        if (!Native.GetMonitorInfo(sourceMonitor, ref source) || !Native.GetMonitorInfo(targetMonitor, ref target))
            return false;

        if (Native.IsZoomed(window))
            return RequestMaximizedMove(window, source, target);

        if (!Native.GetWindowRect(window, out var bounds) || bounds.Width <= 0 || bounds.Height <= 0
            || bounds == source.Bounds)
            return false;

        var destination = FitInWorkArea(bounds, source.WorkArea, target.WorkArea);
        uint flags = Native.NoZOrder | Native.NoActivate | Native.AsyncWindowPos;
        if (destination.Width == bounds.Width && destination.Height == bounds.Height)
            flags |= Native.NoSize;
        Native.SetWindowPos(window, 0, destination.Left, destination.Top, destination.Width, destination.Height, flags);
        return true;
    }

    private static bool RequestMaximizedMove(nint window, Native.MonitorInfo source, Native.MonitorInfo target)
    {
        var placement = new Native.WindowPlacement { Length = Marshal.SizeOf<Native.WindowPlacement>() };
        if (!Native.GetWindowPlacement(window, ref placement) || placement.ShowCommand != Native.ShowMaximized
            || placement.NormalPosition.Width <= 0 || placement.NormalPosition.Height <= 0)
            return false;

        // WINDOWPLACEMENT uses workspace coordinates for ordinary top-level windows.
        var restoredBounds = placement.NormalPosition.Offset(
            source.WorkArea.Left - source.Bounds.Left, source.WorkArea.Top - source.Bounds.Top);
        var destination = FitInWorkArea(restoredBounds, source.WorkArea, target.WorkArea);
        placement.NormalPosition = destination.Offset(
            target.Bounds.Left - target.WorkArea.Left, target.Bounds.Top - target.WorkArea.Top);
        placement.Flags = Native.AsyncWindowPlacement;

        // The system picks a monitor when maximizing a restored window. Updating only the
        // restore rectangle of an already maximized window leaves it on the original monitor.
        placement.ShowCommand = Native.ShowNormalWithoutActivation;
        if (Native.SetWindowPlacement(window, in placement))
        {
            placement.ShowCommand = Native.ShowMaximized;
            Native.SetWindowPlacement(window, in placement);
        }
        return true;
    }

    private static Native.Rect FitInWorkArea(Native.Rect window, Native.Rect source, Native.Rect target)
    {
        int width = Math.Min(window.Width, target.Width);
        int height = Math.Min(window.Height, target.Height);
        int left = Math.Clamp(target.Left + (window.Left - source.Left), target.Left, target.Right - width);
        int top = Math.Clamp(target.Top + (window.Top - source.Top), target.Top, target.Bottom - height);
        return new Native.Rect(left, top, left + width, top + height);
    }
}
