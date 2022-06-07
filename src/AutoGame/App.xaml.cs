namespace AutoGame;

using System;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Threading.Tasks;
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

            AppDomain.CurrentDomain.UnhandledException += this.OnCurrentDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += this.OnTaskSchedulerUnobservedTaskException;
            this.DispatcherUnhandledException += this.OnDispatcherUnhandledException;

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
}