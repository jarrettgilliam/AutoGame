namespace AutoGame;

using System;
using System.IO.Abstractions;
using System.Windows;
using System.Windows.Threading;
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

            Application.Current.DispatcherUnhandledException += this.Current_DispatcherUnhandledException;

            var window = new MainWindow
            {
                DataContext = this.serviceProvider.GetService<MainWindowViewModel>()
            };

            window.Show();
        }
        catch (Exception ex)
        {
            this.LogExceptionAndExit("during startup", ex);
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

    private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) =>
        this.LogExceptionAndExit("unhandled exception", e.Exception);

    private void LogExceptionAndExit(string message, Exception exception)
    {
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
}