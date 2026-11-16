namespace SimpleDisplayFusion;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        using var instance = new Mutex(false, @"Local\SimpleDisplayFusion", out bool firstInstance);
        if (!firstInstance)
            return;

        ApplicationConfiguration.Initialize();

        using var iconStream = typeof(Program).Assembly.GetManifestResourceStream("SimpleDisplayFusion.Tray.ico")!;
        using var icon = new Icon(iconStream);
        using var context = new TrayContext(icon);
        Application.Run(context);
    }
}
