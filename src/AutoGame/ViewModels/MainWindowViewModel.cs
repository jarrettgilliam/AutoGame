namespace AutoGame.ViewModels;

using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;

internal class MainWindowViewModel : BindableBase, IDisposable
{
    private Config config;
    private WindowState windowState;
    private bool showWindow = true;
    private bool notifyIconVisible;

    public MainWindowViewModel(
        ILoggingService loggingService,
        IConfigService configService,
        IAutoGameService autoGameService)
    {
        this.LoggingService = loggingService;
        this.ConfigService = configService;
        this.AutoGameService = autoGameService;

        this.LoadedCommand = new DelegateCommand(this.OnLoaded);
        this.NotifyIconClickCommand = new DelegateCommand(this.OnNotifyIconClick);
        this.BrowseSoftwarePathCommand = new DelegateCommand(this.OnBrowseSoftwarePath);
        this.OKCommand = new DelegateCommand(this.OnOK);
        this.CancelCommand = new DelegateCommand(this.OnCancel);
        this.ApplyCommand = new DelegateCommand(() => this.OnApply());
            
        this.config = this.AutoGameService.CreateDefaultConfiguration();
    }

    private ILoggingService LoggingService { get; }

    private IConfigService ConfigService { get; }

    public IAutoGameService AutoGameService { get; }

    public ICommand LoadedCommand { get; }

    public ICommand NotifyIconClickCommand { get; }

    public ICommand BrowseSoftwarePathCommand { get; }

    public ICommand OKCommand { get; }

    public ICommand CancelCommand { get; }

    public ICommand ApplyCommand { get; }

    public Config Config
    {
        get => this.config;

        set
        {
            var oldValue = this.config;
            if (this.SetProperty(ref this.config, value))
            {
                oldValue.PropertyChanged -= this.OnConfigSoftwareKeyChanged;
                value.PropertyChanged += this.OnConfigSoftwareKeyChanged;
            }
        }
    }

    public WindowState WindowState
    {
        get => this.windowState;
        set => this.SetProperty(ref this.windowState, value);
    }

    public bool ShowWindow
    {
        get => this.showWindow;
        set => this.SetProperty(ref this.showWindow, value);
    }

    public bool NotifyIconVisible
    {
        get => this.notifyIconVisible;
        set => this.SetProperty(ref this.notifyIconVisible, value);
    }

    public void Dispose()
    {
        try
        {
            this.AutoGameService.Dispose();
        }
        catch (Exception ex)
        {
            this.LoggingService.LogException("disposing main window view model", ex);
        }

        this.LoggingService.Dispose();
    }

    private void OnLoaded()
    {
        try
        {
            if (this.TryLoadConfig())
            {
                if (this.AutoGameService.TryApplyConfiguration(this.Config))
                {
                    this.SetWindowState(WindowState.Minimized);
                }
            }
            else
            {
                // The configuration file doesn't exist so consider this initial setup.
                // Create a default configuration without applying it yet and don't minimize.
                this.Config = this.AutoGameService.CreateDefaultConfiguration();
            }
        }
        catch (Exception ex)
        {
            this.LoggingService.LogException("handling application loaded", ex);
        }
    }

    private void OnNotifyIconClick()
    {
        try
        {
            this.SetWindowState(WindowState.Normal);
        }
        catch (Exception ex)
        {
            this.LoggingService.LogException("handling tray icon click", ex);
        }
    }

    private void OnBrowseSoftwarePath()
    {
        try
        {
            ISoftwareManager? software = this.AutoGameService.GetSoftwareByKeyOrNull(this.Config.SoftwareKey);
            string? defaultPath = software?.FindSoftwarePathOrDefault();
            string? executable = Path.GetFileName(defaultPath);

            var dialog = new OpenFileDialog()
            {
                FileName = executable,
                InitialDirectory = Path.GetDirectoryName(this.Config.SoftwarePath)
            };

            if (string.IsNullOrEmpty(dialog.InitialDirectory))
            {
                dialog.InitialDirectory = Path.GetDirectoryName(defaultPath);
            }

            if (software?.Description is not null && executable is not null)
            {   
                dialog.Filter = $"{software.Description}|{executable}";
            }

            if (dialog.ShowDialog() == true)
            {
                this.Config.SoftwarePath = dialog.FileName;
            }
        }
        catch (Exception ex)
        {
            this.LoggingService.LogException("browsing for a software path", ex);
        }
    }

    private void OnOK()
    {
        try
        {
            if (this.OnApply())
            {
                this.SetWindowState(WindowState.Minimized);
            }
        }
        catch (Exception ex)
        {
            this.LoggingService.LogException("handling OK", ex);
        }
    }

    private void OnCancel()
    {
        try
        {
            this.TryLoadConfig();
            this.SetWindowState(WindowState.Minimized);
        }
        catch (Exception ex)
        {
            this.LoggingService.LogException("handling Cancel", ex);
        }
    }

    private bool TryLoadConfig()
    {
        Config? c = this.ConfigService.GetConfigOrNull();

        if (c != null)
        {
            this.Config = c;
            this.LoggingService.EnableTraceLogging = c.EnableTraceLogging;
        }

        return c != null;
    }

    private bool OnApply()
    {
        try
        {
            if (this.Config.IsDirty)
            {
                if (!this.AutoGameService.TryApplyConfiguration(this.Config))
                {
                    return false;
                }

                this.ConfigService.Save(this.Config);
            }

            return true;
        }
        catch (Exception ex)
        {
            this.LoggingService.LogException("handling Apply", ex);
            return false;
        }
    }

    private void OnConfigSoftwareKeyChanged(object? sender, PropertyChangedEventArgs e)
    {
        try
        {
            if (e.PropertyName == nameof(this.Config.SoftwareKey) && sender is Config c)
            {
                ISoftwareManager? s = this.AutoGameService.GetSoftwareByKeyOrNull(c.SoftwareKey);
                c.SoftwarePath = s?.FindSoftwarePathOrDefault();
            }
        }
        catch (Exception ex)
        {
            this.LoggingService.LogException("handling config software key changed", ex);
        }
    }

    private void SetWindowState(WindowState state)
    {
        if (this.WindowState == state)
        {
            return;
        }

        if (state == WindowState.Minimized)
        {
            this.WindowState = state;
            this.ShowWindow = false;
            this.NotifyIconVisible = true;
        }
        else
        {
            this.NotifyIconVisible = false;
            this.ShowWindow = true;
            this.WindowState = state;
        }
    }
}