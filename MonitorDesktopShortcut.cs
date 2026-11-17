using System.ComponentModel;
using System.Runtime.InteropServices;

namespace SimpleDisplayFusion;

internal sealed class MonitorDesktopShortcut : IDisposable
{
    private const ushort MenuMaskKey = 0xE8;
    private static readonly Native.Input[] MenuMaskInputs =
    [
        new() { Type = Native.KeyboardInputType, Data = new() { Keyboard = new() { VirtualKey = MenuMaskKey } } },
        new()
        {
            Type = Native.KeyboardInputType,
            Data = new() { Keyboard = new() { VirtualKey = MenuMaskKey, Flags = Native.ReleaseKey } },
        },
    ];
    private static readonly int InputSize = Marshal.SizeOf<Native.Input>();

    private readonly Native.KeyboardHookCallback callback;
    private readonly WindowsFormsSynchronizationContext uiContext = new();
    private readonly Action<nint> toggleDesktop;
    private nint hook;
    private bool dDown;
    private bool suppressD;

    internal MonitorDesktopShortcut(Action<nint> toggleDesktop)
    {
        this.toggleDesktop = toggleDesktop;
        callback = OnKeyboardEvent;
    }

    internal bool IsRunning => hook != 0;

    internal void Start()
    {
        if (IsRunning)
            return;

        dDown = IsKeyDown(Keys.D);
        hook = Native.SetWindowsHookEx(Native.LowLevelKeyboardHook, callback, Native.GetModuleHandle(0), 0);
        if (hook == 0)
            throw new Win32Exception(Marshal.GetLastWin32Error(), "无法注册 Win + D 键盘钩子。");
    }

    internal void Stop()
    {
        if (!IsRunning)
            return;

        Native.UnhookWindowsHookEx(hook);
        hook = 0;
        dDown = false;
        suppressD = false;
    }

    private nint OnKeyboardEvent(int code, nint message, nint data)
    {
        if (code != 0 || Marshal.ReadInt32(data) != (int)Keys.D)
            return Native.CallNextHookEx(0, code, message, data);

        int action = (int)message;
        if (action is Native.KeyUpMessage or Native.SysKeyUpMessage)
        {
            dDown = false;
            if (suppressD)
            {
                suppressD = false;
                return 1;
            }
        }
        else if (action is Native.KeyDownMessage or Native.SysKeyDownMessage)
        {
            if (suppressD)
                return 1;
            if (!dDown && TryQueueShortcut())
            {
                dDown = true;
                suppressD = true;
                return 1;
            }
            dDown = true;
        }

        return Native.CallNextHookEx(0, code, message, data);
    }

    private bool TryQueueShortcut()
    {
        if ((!IsKeyDown(Keys.LWin) && !IsKeyDown(Keys.RWin))
            || IsKeyDown(Keys.ControlKey) || IsKeyDown(Keys.ShiftKey) || IsKeyDown(Keys.Menu)
            || SystemInformation.MonitorCount <= 1 || !Native.GetCursorPos(out var cursor))
            return false;

        nint monitor = Native.MonitorFromPoint(cursor, Native.MonitorDefaultToNull);
        if (monitor == 0)
            return false;

        // An unassigned key prevents the shell from treating Win key-up as a standalone press.
        Native.SendInput((uint)MenuMaskInputs.Length, MenuMaskInputs, InputSize);

        nint currentHook = hook;
        uiContext.Post(_ =>
        {
            if (hook == currentHook && IsRunning && SystemInformation.MonitorCount > 1)
                toggleDesktop(monitor);
        }, null);
        return true;
    }

    private static bool IsKeyDown(Keys key) => Native.GetAsyncKeyState((int)key) < 0;

    public void Dispose()
    {
        Stop();
        uiContext.Dispose();
    }
}
