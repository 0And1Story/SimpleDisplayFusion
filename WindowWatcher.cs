using System.ComponentModel;
using System.Runtime.InteropServices;
using Timer = System.Windows.Forms.Timer;

namespace SimpleDisplayFusion;

internal sealed class WindowWatcher : IDisposable
{
    private const int FallbackDelayMs = 200;
    private readonly Native.WinEventCallback callback;
    private readonly List<nint> hooks = [];
    private readonly HashSet<nint> seen = [];
    private readonly Dictionary<nint, PendingWindow> pending = [];
    private readonly Timer fallbackTimer = new();

    internal WindowWatcher()
    {
        callback = OnWindowEvent;
        fallbackTimer.Tick += (_, _) => RecheckDueWindows();
    }

    internal bool IsRunning => hooks.Count != 0;

    internal void Start()
    {
        if (IsRunning)
            return;

        try
        {
            // Out-of-context hooks deliver events on this same WinForms message loop.
            AddHook(Native.EventCreate, Native.EventHide);
            AddHook(Native.EventForeground, Native.EventForeground);
            AddHook(Native.EventMoveSizeStart, Native.EventMoveSizeStart);
            AddHook(Native.EventLocationChange, Native.EventLocationChange);
            AddHook(Native.EventUncloaked, Native.EventUncloaked);
            if (!Native.EnumWindows((window, _) =>
                {
                    seen.Add(window);
                    return true;
                }, 0))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "无法读取当前桌面上的窗口。");
        }
        catch
        {
            Stop();
            throw;
        }
    }

    private void AddHook(uint first, uint last)
    {
        nint hook = Native.SetWinEventHook(first, last, 0, callback, 0, 0, Native.SkipOwnProcess);
        if (hook == 0)
            throw new Win32Exception(Marshal.GetLastWin32Error(), "无法注册 Windows 窗口事件。");
        hooks.Add(hook);
    }

    private void OnWindowEvent(nint hook, uint eventType, nint window, int objectId, int childId,
        uint thread, uint timestamp)
    {
        // OBJID_WINDOW and CHILDID_SELF distinguish window events from accessible child elements.
        if (window == 0 || objectId != 0 || childId != 0 || !hooks.Contains(hook))
            return;

        if (eventType is Native.EventDestroy or Native.EventHide or Native.EventMoveSizeStart)
        {
            if (eventType == Native.EventDestroy)
                seen.Remove(window);
            if (pending.Remove(window) && pending.Count == 0)
                fallbackTimer.Stop();
            return;
        }

        if (!pending.TryGetValue(window, out var candidate))
        {
            // A location change can make an opening window ready, but cannot start a new move.
            if (eventType == Native.EventLocationChange || seen.Contains(window)
                || !Native.IsWindowVisible(window) || !WindowMover.IsApplicationWindow(window))
                return;

            seen.Add(window);
            if (!Native.GetCursorPos(out var cursor))
                return;
            nint monitor = Native.MonitorFromPoint(cursor, Native.MonitorDefaultToNull);
            if (monitor == 0)
                return;

            candidate = new PendingWindow(monitor, Environment.TickCount64 + FallbackDelayMs);
            pending.Add(window, candidate);
            if (!fallbackTimer.Enabled)
            {
                fallbackTimer.Interval = FallbackDelayMs;
                fallbackTimer.Start();
            }
        }

        if (!candidate.RequestedMove && Environment.TickCount64 < candidate.Deadline)
            candidate.RequestedMove = WindowMover.RequestMove(window, candidate.Monitor);
    }

    private void RecheckDueWindows()
    {
        fallbackTimer.Stop();
        long now = Environment.TickCount64;
        foreach (var (window, candidate) in pending.ToArray())
        {
            if (candidate.Deadline <= now && pending.Remove(window))
                WindowMover.RequestMove(window, candidate.Monitor);
        }

        if (pending.Count != 0)
        {
            long next = pending.Values.Min(candidate => candidate.Deadline);
            fallbackTimer.Interval = (int)Math.Max(1, next - Environment.TickCount64);
            fallbackTimer.Start();
        }
    }

    internal void Stop()
    {
        fallbackTimer.Stop();
        foreach (nint hook in hooks)
            Native.UnhookWinEvent(hook);
        hooks.Clear();
        pending.Clear();
        seen.Clear();
    }

    public void Dispose()
    {
        Stop();
        fallbackTimer.Dispose();
    }

    private sealed class PendingWindow(nint monitor, long deadline)
    {
        internal nint Monitor { get; } = monitor;
        internal long Deadline { get; } = deadline;
        internal bool RequestedMove { get; set; }
    }
}
