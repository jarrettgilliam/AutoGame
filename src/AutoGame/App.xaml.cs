namespace AutoGame;

using System;
using System.IO.Abstractions;
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
        this.FileSystem = new FileSystem();
        this.AppInfo = new AppInfoService(this.FileSystem);
        this.DateTimeService = new DateTimeService();
        this.User32Service = new WindowsUser32Service();
        this.SleepService = new SleepService();
        this.ProcessService = new ProcessService();
        
        this.LoggingService = new LoggingService(
            this.AppInfo,
            this.DateTimeService,
            this.FileSystem);
    }

    private IAppInfoService AppInfo { get; }
    private IDateTimeService DateTimeService { get; }
    private ILoggingService LoggingService { get; }
    private IUser32Service User32Service { get; }
    private ISleepService SleepService { get; }
    private IProcessService ProcessService { get; }
    private IFileSystem FileSystem { get; }

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
                    new ConfigService(
                        this.AppInfo,
                        this.FileSystem),
                    new AutoGameService(
                        this.LoggingService,
                        this.FileSystem,
                        new ISoftwareManager[]
                        {
                            new SteamBigPictureManager(
                                this.LoggingService,
                                this.User32Service,
                                this.FileSystem,
                                this.ProcessService,
                                new WindowsRegistryService()),
                            new PlayniteFullscreenManager(
                                new WindowService(
                                    this.User32Service,
                                    this.DateTimeService,
                                    this.SleepService),
                                this.FileSystem,
                                this.ProcessService)
                        },
                        new GamepadConnectedCondition(
                            this.LoggingService,
                            new WindowsRawGameControllerService()),
                        new ParsecConnectedCondition(
                            this.LoggingService,
                            new NetStatPortsService(this.ProcessService),
                            this.SleepService,
                            this.ProcessService)),
                    this.FileSystem)
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