namespace AutoGame;

using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using AutoGame.Core;
using AutoGame.Core.Interfaces;
using AutoGame.Infrastructure;
using AutoGame.ViewModels;
using AutoGame.Views;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FluentAvalonia.Styling;
using FluentAvalonia.UI.Media;
using Microsoft.Extensions.DependencyInjection;

public class App : Application
{
    internal readonly ServiceProvider serviceProvider;

    public App()
    {
        ServiceCollection services = new();
        this.ConfigureServices(services);
        this.serviceProvider = services.BuildServiceProvider();
        this.DataContext = new AppViewModel();
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        try
        {
            AppDomain.CurrentDomain.UnhandledException += this.OnCurrentDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += this.OnTaskSchedulerUnobservedTaskException;

            if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
            {
                lifetime.MainWindow = this.serviceProvider.GetService<MainWindow>();
                this.ApplyTheme(lifetime.MainWindow);
            }

            base.OnFrameworkInitializationCompleted();
        }
        catch (Exception ex)
        {
            this.LogExceptionAndExit("during startup", ex);
        }
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<IFileSystem, FileSystem>();

        services.AddCore();
        services.AddInfrastructure();
        services.AddPlatformInfrastructure();
    }

    private void OnCurrentDomainUnhandledException(object? sender, UnhandledExceptionEventArgs e) =>
        this.LogExceptionAndExit("app domain unhandled exception", e.ExceptionObject as Exception);

    private void OnTaskSchedulerUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e) =>
        this.LogExceptionAndExit("task scheduler unhandled exception", e.Exception);

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
            Console.Error.WriteLine(message);
            Console.Error.WriteLine(exception.ToString());
        }

        Environment.Exit(1);
    }

    private void ApplyTheme(Window? window)
    {
        if (AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>() is { } theme)
        {
            theme.ForceWin32WindowToTheme(window);

            Color2 color = Color2.FromRGB(27, 89, 0);

            if (theme.RequestedTheme == FluentAvaloniaTheme.LightModeString)
            {
                color = color.LightenPercent(1f);
            }

            theme.CustomAccentColor = color;
        }
    }
}