using Microsoft.Win32;

namespace SimpleDisplayFusion;

internal static class AppSettings
{
    private const string SettingsKey = @"Software\SimpleDisplayFusion";
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string StartupName = "SimpleDisplayFusion";

    internal static bool FollowMouseEnabled
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(SettingsKey);
            return key?.GetValue("FollowMouseEnabled") is not 0;
        }
        set
        {
            using var key = Registry.CurrentUser.CreateSubKey(SettingsKey);
            key.SetValue("FollowMouseEnabled", value ? 1 : 0, RegistryValueKind.DWord);
        }
    }

    internal static bool WinDPerMonitorEnabled
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(SettingsKey);
            return key?.GetValue(nameof(WinDPerMonitorEnabled)) is 1;
        }
        set
        {
            using var key = Registry.CurrentUser.CreateSubKey(SettingsKey);
            key.SetValue(nameof(WinDPerMonitorEnabled), value ? 1 : 0, RegistryValueKind.DWord);
        }
    }

    internal static bool LaunchAtSignIn
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey);
            return key?.GetValue(StartupName) is string;
        }
        set
        {
            if (!value)
            {
                using var existingKey = Registry.CurrentUser.OpenSubKey(RunKey, writable: true);
                existingKey?.DeleteValue(StartupName, throwOnMissingValue: false);
                return;
            }

            string executable = Environment.ProcessPath
                ?? throw new InvalidOperationException("无法确定当前程序的路径。");
            string command = $"\"{executable}\"";
            if (Path.GetFileName(executable).Equals("dotnet.exe", StringComparison.OrdinalIgnoreCase))
                command += $" \"{Path.Combine(AppContext.BaseDirectory, StartupName + ".dll")}\"";
            if (command.Length > 260)
                throw new InvalidOperationException("程序路径太长，请移到更短的路径后再开启自启动。");

            using var key = Registry.CurrentUser.CreateSubKey(RunKey);
            key.SetValue(StartupName, command, RegistryValueKind.String);
        }
    }
}
