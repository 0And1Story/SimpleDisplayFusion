using System.Runtime.InteropServices;

namespace SimpleDisplayFusion;

internal sealed class MonitorDesktop
{
    private readonly List<MinimizedWindow> minimizedWindows = [];

    internal void Toggle(nint monitor)
    {
        var info = new Native.MonitorInfo { Size = Marshal.SizeOf<Native.MonitorInfo>() };
        if (!Native.GetMonitorInfo(monitor, ref info))
            return;

        var candidates = new List<MinimizedWindow>();
        bool enumerated = Native.EnumWindows((window, _) =>
        {
            if (Native.MonitorFromWindow(window, Native.MonitorDefaultToNull) == monitor
                && Native.IsWindowVisible(window) && !Native.IsIconic(window)
                && WindowMover.IsApplicationWindow(window) && !Native.IsWindowCloaked(window))
            {
                uint thread = Native.GetWindowThreadProcessId(window, out uint process);
                if (thread != 0)
                    candidates.Add(new MinimizedWindow(window, thread, process));
            }
            return true;
        }, 0);
        if (!enumerated)
            return;

        if (candidates.Count == 0)
        {
            RestoreWindows(monitor);
            return;
        }

        var nextBatch = new List<MinimizedWindow>(candidates.Count);
        foreach (var candidate in candidates)
        {
            if (Native.ShowWindowAsync(candidate.Handle, Native.ShowMinimizedWithoutActivation))
                nextBatch.Add(candidate);
        }
        if (nextBatch.Count == 0)
            return;

        // Visible windows start a new Show Desktop cycle for this screen and desktop.
        minimizedWindows.RemoveAll(window =>
            !IsSameWindow(window) || IsOnDesktopMonitor(window.Handle, monitor));
        minimizedWindows.AddRange(nextBatch);
    }

    private void RestoreWindows(nint monitor)
    {
        // Restore in reverse enumeration order so the original top window is restored last.
        for (int i = minimizedWindows.Count - 1; i >= 0; i--)
        {
            var window = minimizedWindows[i];
            if (!IsSameWindow(window))
            {
                minimizedWindows.RemoveAt(i);
                continue;
            }

            if (!IsOnDesktopMonitor(window.Handle, monitor))
                continue;

            if (!Native.IsIconic(window.Handle)
                || (Native.IsWindowVisible(window.Handle)
                    && Native.ShowWindowAsync(window.Handle, Native.ShowNormalWithoutActivation)))
                minimizedWindows.RemoveAt(i);
        }
    }

    private static bool IsSameWindow(MinimizedWindow window) =>
        Native.GetWindowThreadProcessId(window.Handle, out uint process) == window.Thread
        && process == window.Process;

    private static bool IsOnDesktopMonitor(nint window, nint monitor) =>
        Native.MonitorFromWindow(window, Native.MonitorDefaultToNull) == monitor
        && !Native.IsWindowCloaked(window);

    private readonly record struct MinimizedWindow(nint Handle, uint Thread, uint Process);
}
