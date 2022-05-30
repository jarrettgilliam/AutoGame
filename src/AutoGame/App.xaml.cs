namespace AutoGame;

using System;
using System.IO.Abstractions;
using System.Windows;
using AutoGame.Core;
using AutoGame.Core.Interfaces;
using AutoGame.ViewModels;
using AutoGame.Views;
using AutoGame.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly ServiceProvider serviceProvider;

    public App()
    {
        ServiceCollection services = new();

        this.ConfigureServices(services);
        this.serviceProvider = services.BuildServiceProvider();
    }

    private ILoggingService? LoggingService => this.serviceProvider.GetService<ILoggingService>();

    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            base.OnStartup(e);

            Application.Current.DispatcherUnhandledException += this.Current_DispatcherUnhandledException;

            var window = new MainWindow
            {
                DataContext = this.serviceProvider.GetService<MainWindowViewModel>()
            };

            window.Show();
        }
        catch (Exception ex)
        {
            this.LoggingService?.LogException("handling startup", ex);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Application.Current.DispatcherUnhandledException -= this.Current_DispatcherUnhandledException;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<IFileSystem, FileSystem>();
        
        services.AddCore();
        services.AddInfrastructure();
    }

    private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        this.LoggingService?.LogException("unhandled exception", e.Exception);
    }
}