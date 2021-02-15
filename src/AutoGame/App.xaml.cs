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
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var window = new MainWindow()
            {
                DataContext = new MainWindowViewModel(
                    new ConfigService(),
                    new AutoGameService(
                        new ISoftwareManager[]
                        {
                            new SteamBigPictureManager(),
                            new PlayniteFullscreenManager()
                        },
                        new GamepadConnectedCondition(),
                        new ParsecConnectedCondition()))
            };

            window.Show();
        }
    }
}
