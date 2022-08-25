namespace AutoGame.ViewModels;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AutoGame.Commands;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public class MainWindowViewModel : ObservableObject
{
    private Config config;
    private bool showWindow = true;
    private ISoftwareManager? selectedSoftware;

    public MainWindowViewModel(
        ILoggingService loggingService,
        IConfigService configService,
        IAutoGameService autoGameService,
        IFileSystem fileSystem,
        IDialogService dialogService)
    {
        this.LoggingService = loggingService;
        this.ConfigService = configService;
        this.FileSystem = fileSystem;
        this.DialogService = dialogService;
        this.AutoGameService = autoGameService;

        this.LoadedCommand = new RelayCommand(this.OnLoaded);

        this.BrowseSoftwarePathCommand = new AsyncDelegateCommand(this.OnBrowseSoftwarePath);
        this.BrowseSoftwarePathCommand.OnException += this.OnAsyncDelegateCommandException;

        this.OKCommand = new RelayCommand(this.OnOK);
        this.CancelCommand = new RelayCommand(this.OnCancel);
        this.ApplyCommand = new RelayCommand(() => this.OnApply());

        this.config = this.ConfigService.CreateDefault(
            this.AutoGameService.AvailableSoftware.FirstOrDefault());

        this.config.PropertyChanged += this.OnConfigSoftwareKeyChanged;
    }

    private ILoggingService LoggingService { get; }
    private IConfigService ConfigService { get; }
    private IFileSystem FileSystem { get; }
    private IDialogService DialogService { get; }

    public IAutoGameService AutoGameService { get; }

    public ICommand LoadedCommand { get; }

    public AsyncDelegateCommand BrowseSoftwarePathCommand { get; }

    public ICommand OKCommand { get; }

    public ICommand CancelCommand { get; }

    public ICommand ApplyCommand { get; }

    public Config Config
    {
        get => this.config;

        set
        {
            Config? oldValue = this.config;
            if (this.SetProperty(ref this.config, value))
            {
                oldValue.PropertyChanged -= this.OnConfigSoftwareKeyChanged;
                value.PropertyChanged += this.OnConfigSoftwareKeyChanged;
                this.SelectedSoftware = this.AutoGameService.GetSoftwareByKeyOrNull(value.SoftwareKey);
            }
        }
    }

    public bool ShowWindow
    {
        get => this.showWindow;
        set => this.SetProperty(ref this.showWindow, value);
    }

    public ISoftwareManager? SelectedSoftware
    {
        get => this.selectedSoftware;
        set
        {
            if (this.SetProperty(ref this.selectedSoftware, value))
            {
                this.Config.SoftwareKey = value?.Key;
            }
        }
    }

    private void OnLoaded()
    {
        try
        {
            if (this.TryLoadConfig())
            {
                this.ConfigService.Validate(this.Config, this.AutoGameService.AvailableSoftware);

                if (!this.Config.HasErrors)
                {
                    this.ShowWindow = false;
                    this.AutoGameService.ApplyConfiguration(this.Config);
                }
            }
            else
            {
                // The configuration file doesn't exist so consider this initial setup.
                // Create a default configuration without applying it yet and don't minimize.
                this.Config = this.ConfigService.CreateDefault(
                    this.AutoGameService.AvailableSoftware.FirstOrDefault());
            }
        }
        catch (Exception ex)
        {
            this.LoggingService.LogException("handling application loaded", ex);
        }
    }

    private async Task OnBrowseSoftwarePath()
    {
        try
        {
            ISoftwareManager? software = this.AutoGameService.GetSoftwareByKeyOrNull(this.Config.SoftwareKey);

            if (software is null)
            {
                return;
            }

            string defaultPath = software.FindSoftwarePathOrDefault();
            string? executable = this.FileSystem.Path.GetFileName(defaultPath);

            var parms = new OpenFileDialogParms
            {
                FileName = executable,
                InitialDirectory = this.FileSystem.Path.GetDirectoryName(this.Config.SoftwarePath),
                FilterName = software.Description,
                FilterExtensions = new List<string> { "exe" }
            };

            if (string.IsNullOrEmpty(parms.InitialDirectory))
            {
                parms.InitialDirectory = this.FileSystem.Path.GetDirectoryName(defaultPath);
            }

            if (await this.DialogService.ShowOpenFileDialog(parms) is { } selectedFileName)
            {
                this.Config.SoftwarePath = selectedFileName;
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
                this.ShowWindow = false;
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
            this.ConfigService.Validate(this.Config, this.AutoGameService.AvailableSoftware);
            this.ShowWindow = false;
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
            this.ConfigService.Upgrade(c, this.AutoGameService.GetSoftwareByKeyOrNull(c.SoftwareKey));
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
                this.ConfigService.Validate(this.Config, this.AutoGameService.AvailableSoftware);

                if (this.Config.HasErrors)
                {
                    return false;
                }

                this.AutoGameService.ApplyConfiguration(this.Config);
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
                this.SelectedSoftware = s;
                c.SoftwarePath = s?.FindSoftwarePathOrDefault();
                c.SoftwareArguments = s?.DefaultArguments;
            }
        }
        catch (Exception ex)
        {
            this.LoggingService.LogException("handling config software key changed", ex);
        }
    }

    private void OnAsyncDelegateCommandException(object? sender, Exception e) =>
        this.LoggingService.LogException(e.Message, e);
}