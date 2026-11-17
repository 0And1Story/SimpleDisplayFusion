using System.Runtime.InteropServices;
using System.Text;

namespace SimpleDisplayFusion;

internal static class Native
{
    internal const uint EventForeground = 0x0003;
    internal const uint EventMoveSizeStart = 0x000A;
    internal const uint EventCreate = 0x8000;
    internal const uint EventDestroy = 0x8001;
    internal const uint EventShow = 0x8002;
    internal const uint EventHide = 0x8003;
    internal const uint EventLocationChange = 0x800B;
    internal const uint EventUncloaked = 0x8018;
    internal const uint SkipOwnProcess = 0x0002;

    internal const int WindowStyle = -16;
    internal const int ExtendedWindowStyle = -20;
    internal const uint Child = 0x40000000;
    internal const uint Caption = 0x00C00000;
    internal const uint ThickFrame = 0x00040000;
    internal const uint ToolWindow = 0x00000080;
    internal const uint AppWindow = 0x00040000;
    internal const uint NoActivateStyle = 0x08000000;
    internal const uint Owner = 4;
    internal const uint Root = 2;

    internal const uint MonitorDefaultToNull = 0;
    internal const uint MonitorDefaultToNearest = 2;
    internal const uint NoSize = 0x0001;
    internal const uint NoZOrder = 0x0004;
    internal const uint NoActivate = 0x0010;
    internal const uint AsyncWindowPos = 0x4000;
    internal const uint AsyncWindowPlacement = 0x0004;
    internal const uint ShowMaximized = 3;
    internal const int ShowNormalWithoutActivation = 4;
    internal const int ShowMinimizedWithoutActivation = 7;
    internal const int DwmCloaked = 14;

    internal const int LowLevelKeyboardHook = 13;
    internal const int KeyDownMessage = 0x0100;
    internal const int KeyUpMessage = 0x0101;
    internal const int SysKeyDownMessage = 0x0104;
    internal const int SysKeyUpMessage = 0x0105;
    internal const uint KeyboardInputType = 1;
    internal const uint ReleaseKey = 0x0002;

    internal delegate void WinEventCallback(nint hook, uint eventType, nint window, int objectId,
        int childId, uint thread, uint timestamp);
    internal delegate bool EnumWindowsCallback(nint window, nint parameter);
    internal delegate nint KeyboardHookCallback(int code, nint message, nint data);

    [StructLayout(LayoutKind.Sequential)]
    internal record struct Point(int X, int Y);

    [StructLayout(LayoutKind.Sequential)]
    internal record struct Rect(int Left, int Top, int Right, int Bottom)
    {
        internal readonly int Width => Right - Left;
        internal readonly int Height => Bottom - Top;
        internal readonly Rect Offset(int x, int y) => new(Left + x, Top + y, Right + x, Bottom + y);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MonitorInfo
    {
        internal int Size;
        internal Rect Bounds;
        internal Rect WorkArea;
        internal uint Flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowPlacement
    {
        internal int Length;
        internal uint Flags;
        internal uint ShowCommand;
        internal Point MinPosition;
        internal Point MaxPosition;
        internal Rect NormalPosition;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Input
    {
        internal uint Type;
        internal InputData Data;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct InputData
    {
        [FieldOffset(0)] internal KeyboardInput Keyboard;
        [FieldOffset(0)] private MouseInput mouse;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct KeyboardInput
    {
        internal ushort VirtualKey;
        internal ushort ScanCode;
        internal uint Flags;
        internal uint Timestamp;
        internal nuint ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MouseInput
    {
        internal int X;
        internal int Y;
        internal uint MouseData;
        internal uint Flags;
        internal uint Timestamp;
        internal nuint ExtraInfo;
    }

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern nint SetWinEventHook(uint first, uint last, nint module,
        WinEventCallback callback, uint process, uint thread, uint flags);

    [DllImport("user32.dll")]
    internal static extern bool UnhookWinEvent(nint hook);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool EnumWindows(EnumWindowsCallback callback, nint parameter);

    [DllImport("user32.dll")]
    internal static extern nint GetAncestor(nint window, uint flags);

    [DllImport("user32.dll")]
    internal static extern nint GetWindow(nint window, uint command);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW", ExactSpelling = true)]
    internal static extern uint GetWindowLong(nint window, int index);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    internal static extern int GetClassName(nint window, StringBuilder name, int maxCount);

    [DllImport("user32.dll")]
    internal static extern bool IsWindowVisible(nint window);

    [DllImport("user32.dll")]
    internal static extern bool IsIconic(nint window);

    [DllImport("user32.dll")]
    internal static extern bool IsZoomed(nint window);

    [DllImport("user32.dll")]
    internal static extern bool ShowWindowAsync(nint window, int command);

    [DllImport("user32.dll")]
    internal static extern uint GetWindowThreadProcessId(nint window, out uint processId);

    [DllImport("user32.dll")]
    internal static extern bool GetCursorPos(out Point point);

    [DllImport("user32.dll")]
    internal static extern nint MonitorFromPoint(Point point, uint flags);

    [DllImport("user32.dll")]
    internal static extern nint MonitorFromWindow(nint window, uint flags);

    [DllImport("user32.dll", EntryPoint = "GetMonitorInfoW", ExactSpelling = true)]
    internal static extern bool GetMonitorInfo(nint monitor, ref MonitorInfo info);

    [DllImport("user32.dll")]
    internal static extern bool GetWindowRect(nint window, out Rect rect);

    [DllImport("user32.dll")]
    internal static extern bool SetWindowPos(nint window, nint insertAfter, int x, int y,
        int width, int height, uint flags);

    [DllImport("user32.dll")]
    internal static extern bool GetWindowPlacement(nint window, ref WindowPlacement placement);

    [DllImport("user32.dll")]
    internal static extern bool SetWindowPlacement(nint window, in WindowPlacement placement);

    [DllImport("dwmapi.dll")]
    internal static extern int DwmGetWindowAttribute(nint window, int attribute, out int value, int size);

    internal static bool IsWindowCloaked(nint window) =>
        DwmGetWindowAttribute(window, DwmCloaked, out int cloaked, sizeof(int)) == 0 && cloaked != 0;

    [DllImport("user32.dll", EntryPoint = "SetWindowsHookExW", ExactSpelling = true, SetLastError = true)]
    internal static extern nint SetWindowsHookEx(int type, KeyboardHookCallback callback, nint module, uint thread);

    [DllImport("user32.dll")]
    internal static extern bool UnhookWindowsHookEx(nint hook);

    [DllImport("user32.dll")]
    internal static extern nint CallNextHookEx(nint hook, int code, nint message, nint data);

    [DllImport("user32.dll")]
    internal static extern short GetAsyncKeyState(int key);

    [DllImport("user32.dll")]
    internal static extern uint SendInput(uint count, [In] Input[] inputs, int size);

    [DllImport("kernel32.dll", EntryPoint = "GetModuleHandleW", ExactSpelling = true)]
    internal static extern nint GetModuleHandle(nint moduleName);
}
