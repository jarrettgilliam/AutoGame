namespace AutoGame.ViewModels;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Abstractions;
using System.Threading.Tasks;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public sealed partial class MainWindowViewModel : ObservableObject
{
    private Config config;
    private bool showWindow = true;
    private UpdateInfo? updateInfo;

#nullable disable

    // This constructor exists only for auto completion in the axaml
    // ReSharper disable once NotNullOrRequiredMemberIsNotInitialized
    internal MainWindowViewModel()
    {
    }
#nullable restore

    public MainWindowViewModel(
        ILoggingService loggingService,
        IConfigService configService,
        IAutoGameService autoGameService,
        IFileSystem fileSystem,
        IDialogService dialogService,
        IUpdateCheckingService updateCheckingService,
        ISoftwareCollection availableSoftware)
    {
        this.LoggingService = loggingService;
        this.ConfigService = configService;
        this.FileSystem = fileSystem;
        this.DialogService = dialogService;
        this.AutoGameService = autoGameService;
        this.UpdateCheckingService = updateCheckingService;
        this.AvailableSoftware = availableSoftware;

        this.BrowseSoftwarePathCommand = new AsyncRelayCommand(this.BrowseSoftwarePath);

        this.config = this.ConfigService.CreateDefault();

        this.config.PropertyChanged += this.OnConfigPropertyChanged;
    }

    private ILoggingService LoggingService { get; }
    private IConfigService ConfigService { get; }
    private IFileSystem FileSystem { get; }
    private IDialogService DialogService { get; }
    private IAutoGameService AutoGameService { get; }
    private IUpdateCheckingService UpdateCheckingService { get; }

    public ISoftwareCollection AvailableSoftware { get; }

    public IAsyncRelayCommand BrowseSoftwarePathCommand { get; }

    public Config Config
    {
        get => this.config;

        set
        {
            Config oldValue = this.config;
            if (this.SetProperty(ref this.config, value))
            {
                oldValue.PropertyChanged -= this.OnConfigPropertyChanged;
                value.PropertyChanged += this.OnConfigPropertyChanged;
            }
        }
    }

    public bool ShowWindow
    {
        get => this.showWindow;
        set => this.SetProperty(ref this.showWindow, value);
    }

    public UpdateInfo? UpdateInfo
    {
        get => this.updateInfo;
        set => this.SetProperty(ref this.updateInfo, value);
    }

    [RelayCommand]
    private void Loaded()
    {
        try
        {
            if (this.TryLoadConfig())
            {
                this.ConfigService.Validate(this.Config);

                if (!this.Config.HasErrors)
                {
                    this.ShowWindow = !this.Config.StartMinimized;
                    this.AutoGameService.ApplyConfiguration(this.Config);
                }
            }
            else
            {
                // The configuration file doesn't exist so consider this initial setup.
                // Create a default configuration without applying it yet and don't minimize.
                this.Config = this.ConfigService.CreateDefault();
            }

            if (this.Config.CheckForUpdates)
            {
                this.UpdateCheckingService.GetUpdateInfo().ContinueWith(
                    t => this.UpdateInfo = t.Result);
            }
        }
        catch (Exception ex)
        {
            this.LoggingService.LogException("handling application loaded", ex);
        }
    }

    private async Task BrowseSoftwarePath()
    {
        try
        {
            ISoftwareManager? software = this.AvailableSoftware.GetSoftwareByKeyOrNull(this.Config.SoftwareKey);

            if (software is null)
            {
                return;
            }

            string defaultPath = software.FindSoftwarePathOrDefault();
            string executable = this.FileSystem.Path.GetFileName(defaultPath);

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

    [RelayCommand]
    private void OK()
    {
        try
        {
            if (this.ApplyInternal())
            {
                this.ShowWindow = false;
            }
        }
        catch (Exception ex)
        {
            this.LoggingService.LogException("handling OK", ex);
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        try
        {
            this.TryLoadConfig();
            this.ConfigService.Validate(this.Config);

            if (!this.Config.HasErrors)
            {
                this.ShowWindow = false;
            }
        }
        catch (Exception ex)
        {
            this.LoggingService.LogException("handling Cancel", ex);
        }
    }

    private bool TryLoadConfig()
    {
        Config? c = this.ConfigService.GetConfigOrNull();

        if (c is null)
        {
            return false;
        }

        this.ConfigService.Upgrade(c);
        this.Config = c;
        this.LoggingService.EnableTraceLogging = c.EnableTraceLogging;

        return true;
    }

    [RelayCommand]
    private void Apply() => this.ApplyInternal();

    private bool ApplyInternal()
    {
        try
        {
            this.ConfigService.Validate(this.Config);
            if (this.Config.HasErrors)
            {
                return false;
            }

            if (this.Config.IsDirty)
            {
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

    private void OnConfigPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        try
        {
            if (e.PropertyName == nameof(this.Config.SoftwareKey) && sender is Config c)
            {
                ISoftwareManager? s = this.AvailableSoftware.GetSoftwareByKeyOrNull(c.SoftwareKey);
                c.SoftwarePath = s?.FindSoftwarePathOrDefault();
                c.SoftwareArguments = s?.DefaultArguments;
            }
        }
        catch (Exception ex)
        {
            this.LoggingService.LogException("handling config property changed", ex);
        }
    }
}