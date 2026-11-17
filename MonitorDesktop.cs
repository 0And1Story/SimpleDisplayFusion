using System.Runtime.InteropServices;

namespace SimpleDisplayFusion;

internal sealed class MonitorDesktop
{
    private readonly List<MinimizedWindow> minimizedWindows = [];

    internal void Toggle(nint monitor)
    {
        var info = new Native.MonitorInfo { Size = Marshal.SizeOf<Native.MonitorInfo>() };
        if (!Native.GetMonitorInfo(monitor, ref info) || RestoreWindows(monitor))
            return;

        var candidates = new List<MinimizedWindow>();
        Native.EnumWindows((window, _) =>
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

        foreach (var candidate in candidates)
        {
            if (Native.ShowWindowAsync(candidate.Handle, Native.ShowMinimizedWithoutActivation))
                minimizedWindows.Add(candidate);
        }
    }

    private bool RestoreWindows(nint monitor)
    {
        bool found = false;

        // Restore in reverse enumeration order so the original top window is restored last.
        for (int i = minimizedWindows.Count - 1; i >= 0; i--)
        {
            var window = minimizedWindows[i];
            uint thread = Native.GetWindowThreadProcessId(window.Handle, out uint process);
            if (thread != window.Thread || process != window.Process || !Native.IsIconic(window.Handle))
            {
                minimizedWindows.RemoveAt(i);
                continue;
            }

            if (Native.MonitorFromWindow(window.Handle, Native.MonitorDefaultToNull) != monitor
                || !Native.IsWindowVisible(window.Handle) || Native.IsWindowCloaked(window.Handle))
                continue;

            found = true;
            if (Native.ShowWindowAsync(window.Handle, Native.RestoreWindow))
                minimizedWindows.RemoveAt(i);
        }
        return found;
    }

    private readonly record struct MinimizedWindow(nint Handle, uint Thread, uint Process);
}
