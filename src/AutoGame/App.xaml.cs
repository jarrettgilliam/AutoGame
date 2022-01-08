using System;
using System.Windows;
using AutoGame.Infrastructure.Interfaces;
using AutoGame.Infrastructure.LaunchConditions;
using AutoGame.Infrastructure.Services;
using AutoGame.Infrastructure.SoftwareManager;
using AutoGame.ViewModels;
using AutoGame.Views;

namespace AutoGame
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ILoggingService LoggingService { get; } = new LoggingService();

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
                        new ConfigService(),
                        new AutoGameService(
                            this.LoggingService,
                            new ISoftwareManager[]
                            {
                            new SteamBigPictureManager(this.LoggingService),
                            new PlayniteFullscreenManager()
                            },
                            new GamepadConnectedCondition(this.LoggingService),
                            new ParsecConnectedCondition(this.LoggingService)))
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
}
