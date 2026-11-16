using System.ComponentModel;
using System.Security;

namespace SimpleDisplayFusion;

internal sealed class TrayContext : ApplicationContext
{
    private const string AppName = "SimpleDisplayFusion";

    private readonly WindowWatcher watcher = new();
    private readonly ToolStripMenuItem followMouse = new("新窗口跟随鼠标");
    private readonly ToolStripMenuItem launchAtSignIn = new("登录 Windows 时启动");
    private readonly ContextMenuStrip menu = new();
    private readonly NotifyIcon tray;

    internal TrayContext(Icon icon)
    {
        followMouse.Click += (_, _) => ToggleFollowMouse();
        launchAtSignIn.Click += (_, _) => ToggleStartup();
        var exit = new ToolStripMenuItem("退出", null, (_, _) => ExitThread());
        menu.Items.AddRange([followMouse, launchAtSignIn, new ToolStripSeparator(), exit]);
        menu.Opening += (_, _) => RefreshStartup();

        tray = new NotifyIcon { Icon = icon, ContextMenuStrip = menu, Text = AppName, Visible = true };
        try
        {
            if (AppSettings.FollowMouseEnabled)
                watcher.Start();
        }
        catch (Exception error) when (IsSettingsError(error))
        {
            ShowError("无法启用窗口跟随。", error);
        }
        RefreshFollowMouse();
    }

    private void ToggleFollowMouse()
    {
        try
        {
            if (watcher.IsRunning)
            {
                AppSettings.FollowMouseEnabled = false;
                watcher.Stop();
            }
            else
            {
                watcher.Start();
                try
                {
                    AppSettings.FollowMouseEnabled = true;
                }
                catch
                {
                    watcher.Stop();
                    throw;
                }
            }
        }
        catch (Exception error) when (IsSettingsError(error))
        {
            ShowError("无法更改窗口跟随设置。", error);
        }
        RefreshFollowMouse();
    }

    private void ToggleStartup()
    {
        try
        {
            AppSettings.LaunchAtSignIn = !AppSettings.LaunchAtSignIn;
        }
        catch (Exception error) when (IsSettingsError(error))
        {
            ShowError("无法更改自启动设置。", error);
        }
        RefreshStartup();
    }

    private void RefreshFollowMouse()
    {
        followMouse.Checked = watcher.IsRunning;
        tray.Text = watcher.IsRunning ? AppName : $"{AppName}（已暂停）";
    }

    private void RefreshStartup()
    {
        try
        {
            launchAtSignIn.Checked = AppSettings.LaunchAtSignIn;
            launchAtSignIn.Enabled = true;
        }
        catch (Exception error) when (IsSettingsError(error))
        {
            launchAtSignIn.Checked = false;
            launchAtSignIn.Enabled = false;
            launchAtSignIn.ToolTipText = error.Message;
            return;
        }
        launchAtSignIn.ToolTipText = "";
    }

    private static bool IsSettingsError(Exception error) =>
        error is Win32Exception or IOException or UnauthorizedAccessException
            or SecurityException or InvalidOperationException;

    private static void ShowError(string message, Exception error) =>
        MessageBox.Show($"{message}\n{error.Message}", AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            watcher.Dispose();
            tray.Dispose();
            menu.Dispose();
        }
        base.Dispose(disposing);
    }
}
