namespace AutoGame;

using System;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using AutoGame.Core;
using AutoGame.Core.Enums;
using AutoGame.Core.Interfaces;
using AutoGame.ViewModels;
using AutoGame.Views;
using AutoGame.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Appearance;
using Wpf.Ui.Extensions;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    internal readonly ServiceProvider serviceProvider;

    public App()
    {
        ServiceCollection services = new();
        this.ConfigureServices(services);
        this.serviceProvider = services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            base.OnStartup(e);

            AppDomain.CurrentDomain.UnhandledException += this.OnCurrentDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += this.OnTaskSchedulerUnobservedTaskException;
            this.DispatcherUnhandledException += this.OnDispatcherUnhandledException;

            var window = new MainWindow
            {
                DataContext = this.serviceProvider.GetService<MainWindowViewModel>()
            };

            this.ApplyTheme();

            window.Show();
        }
        catch (TypeInitializationException ex) when (ex.Message.Contains("Joysticks"))
        {
            this.HandleMissingCPPRedistributable(ex);
        }
        catch (Exception ex)
        {
            this.LogExceptionAndExit("during startup", ex);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        this.serviceProvider.Dispose();
        AppDomain.CurrentDomain.UnhandledException -= this.OnCurrentDomainUnhandledException;
        TaskScheduler.UnobservedTaskException -= this.OnTaskSchedulerUnobservedTaskException;
        this.Dispatcher.UnhandledException -= this.OnDispatcherUnhandledException;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<IFileSystem, FileSystem>();

        services.AddCore();
        services.AddInfrastructure();
    }

    private void OnCurrentDomainUnhandledException(object? sender, UnhandledExceptionEventArgs e) =>
        this.LogExceptionAndExit("app domain unhandled exception", e.ExceptionObject as Exception);

    private void OnTaskSchedulerUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e) =>
        this.LogExceptionAndExit("task scheduler unhandled exception", e.Exception);

    private void OnDispatcherUnhandledException(object? sender, DispatcherUnhandledExceptionEventArgs e)
    {
        if (!Debugger.IsAttached)
        {
            e.Handled = true;
            this.LogExceptionAndExit("dispatcher unhandled exception", e.Exception);
        }
    }

    private void LogExceptionAndExit(string message, Exception? exception)
    {
        exception ??= new Exception("Unknown exception");

        var loggingService = this.serviceProvider.GetService<ILoggingService>();

        if (loggingService is not null)
        {
            loggingService.LogException(message, exception);
        }
        else
        {
            MessageBox.Show(exception.ToString(), $"Error {message}", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        Environment.Exit(1);
    }

    private void ApplyTheme()
    {
        // The watcher kept crashing on me for some reason.
        // Only apply the theme on startup for now.
        // Watcher.Watch(window, updateAccents: false);
        Theme.Apply(this.GetThemeType(), updateAccent: false);

        Color secondaryAccent = Color.FromRgb(0x26, 0x7F, 0x00);
        Color tertiaryAccent = secondaryAccent.Update(15f, -12f);
        Color primaryAccent = secondaryAccent.Update(-15f, 12f);
        Color systemAccent = secondaryAccent.Update(-30f, 24f);

        Accent.Apply(systemAccent, primaryAccent, secondaryAccent, tertiaryAccent);
    }

    private ThemeType GetThemeType()
    {
        return Theme.GetSystemTheme() switch
        {
            SystemThemeType.Dark => ThemeType.Dark,
            _ => ThemeType.Light,
        };
    }

    private void HandleMissingCPPRedistributable(Exception ex)
    {
        this.serviceProvider.GetService<ILoggingService>()?.Log($"Error during startup: {ex}", LogLevel.Error);

        string message = "To run this application, you must install Visual C++ Redistributable for Visual Studio 2015." + Environment.NewLine +
                         Environment.NewLine +
                         "Would you like to download it now?";

        var result = MessageBox.Show(message, $"{nameof(AutoGame)}.exe", MessageBoxButton.YesNo, MessageBoxImage.Error);

        if (result == MessageBoxResult.Yes)
        {
            Process.Start(new ProcessStartInfo("cmd", "/c start https://www.microsoft.com/en-us/download/details.aspx?id=48145"));
        }

        Environment.Exit(1);
    }
}