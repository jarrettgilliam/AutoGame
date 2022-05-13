namespace AutoGame;

using System;
using System.Windows;
using AutoGame.Core.Interfaces;
using AutoGame.Infrastructure.LaunchConditions;
using AutoGame.Infrastructure.Services;
using AutoGame.Infrastructure.SoftwareManagers;
using AutoGame.ViewModels;
using AutoGame.Views;
using AutoGame.Core.Services;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        this.AppInfo = new AppInfoService();
        this.LoggingService = new LoggingService(this.AppInfo);
    }
        
    private IAppInfoService AppInfo { get; }
    private ILoggingService LoggingService { get; }

    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            base.OnStartup(e);

            Application.Current.DispatcherUnhandledException += this.Current_DispatcherUnhandledException;

            var window = new MainWindow()
            {
                DataContext = new MainWindowViewModel(
                    this.LoggingService,
                    new ConfigService(this.AppInfo),
                    new AutoGameService(
                        this.LoggingService,
                        new ISoftwareManager[]
                        {
                            new SteamBigPictureManager(this.LoggingService),
                            new PlayniteFullscreenManager(new WindowService())
                        },
                        new GamepadConnectedCondition(this.LoggingService),
                        new ParsecConnectedCondition(
                            this.LoggingService,
                            new NetStatPortsService())))
            };

            window.Show();
        }
        catch (Exception ex)
        {
            this.LoggingService.LogException("handling startup", ex);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Application.Current.DispatcherUnhandledException -= this.Current_DispatcherUnhandledException;
    }

    private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        this.LoggingService.LogException("unhandled exception", e.Exception);
    }
}