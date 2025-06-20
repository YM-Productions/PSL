using System;
using Avalonia;
using Client_PSL.Services;

namespace Client_PSL.Desktop;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    // [STAThread]
    // public static void Main(string[] args) => BuildAvaloniaApp()
    //     .StartWithClassicDesktopLifetime(args);
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        if (Globals.spacetimeController.IsConnected)
            Globals.spacetimeController.CloseCon();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<Client_PSL.Desktop.App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
