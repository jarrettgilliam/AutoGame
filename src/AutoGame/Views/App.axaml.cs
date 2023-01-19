namespace AutoGame.Views;

using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using AutoGame.Core;
using AutoGame.Infrastructure;
using AutoGame.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FluentAvalonia.Styling;
using FluentAvalonia.UI.Media;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;

public class App : Application
{
    internal readonly ServiceProvider serviceProvider;

    public App()
    {
        ServiceCollection services = new();
        this.ConfigureServices(services);
        this.serviceProvider = services.BuildServiceProvider();
        SerilogConfiguration.ConfigureFullLogger(this.serviceProvider);
        this.DataContext = new AppViewModel();
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
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

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<IFileSystem, FileSystem>();

        services.AddCore();
        services.AddInfrastructure();
        services.AddPlatformInfrastructure();

        var github = new Octokit.GitHubClient(new Octokit.ProductHeaderValue(nameof(AutoGame)));
        services.AddSingleton(github.Repository.Release);

        services.AddSingleton<LoggingLevelSwitch>();
        services.AddTransient<ILogger>(_ => Log.Logger);
    }

    private void OnCurrentDomainUnhandledException(object? sender, UnhandledExceptionEventArgs e) =>
        this.LogExceptionAndExit(e.ExceptionObject as Exception, "app domain unhandled exception");

    private void OnTaskSchedulerUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e) =>
        this.LogExceptionAndExit(e.Exception, "task scheduler unhandled exception");

    private void LogExceptionAndExit(Exception? exception, string message)
    {
        exception ??= new Exception("Unknown exception");

        Log.Fatal(exception, message);

        Log.CloseAndFlush();

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