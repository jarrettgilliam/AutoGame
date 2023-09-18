namespace AutoGame;

using System;
using AutoGame.Views;
using Avalonia;
using Serilog;

internal class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        try
        {
            SerilogConfiguration.ConfigureInitialLogger();
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "in main method");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}