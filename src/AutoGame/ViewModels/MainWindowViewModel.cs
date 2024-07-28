namespace AutoGame.ViewModels;

using System;
using System.ComponentModel;
using System.IO.Abstractions;
using System.Threading.Tasks;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using Serilog.Core;
using Serilog.Events;

public sealed partial class MainWindowViewModel : ObservableObject
{
    private Config config;
    private bool showWindow = true;
    private UpdateInfo? updateInfo;

#nullable disable
    // This constructor exists only for auto-completion in the axaml
    // ReSharper disable once NotNullOrRequiredMemberIsNotInitialized
    internal MainWindowViewModel()
    {
    }
#nullable restore

    public MainWindowViewModel(
        ILogger logger,
        LoggingLevelSwitch loggingLevelSwitch,
        IConfigService configService,
        IAutoGameService autoGameService,
        IFileSystem fileSystem,
        IDialogService dialogService,
        IUpdateCheckingService updateCheckingService,
        ISoftwareCollection availableSoftware)
    {
        this.Logger = logger;
        this.LoggingLevelSwitch = loggingLevelSwitch;
        this.ConfigService = configService;
        this.FileSystem = fileSystem;
        this.DialogService = dialogService;
        this.AutoGameService = autoGameService;
        this.UpdateCheckingService = updateCheckingService;
        this.AvailableSoftware = availableSoftware;

        this.BrowseSoftwarePathCommand = new AsyncRelayCommand(this.BrowseSoftwarePath);
        this.LoadedCommand = new AsyncRelayCommand(this.Loaded);

        this.config = this.ConfigService.CreateDefault();

        this.config.PropertyChanged += this.OnConfigPropertyChanged;
    }

    private ILogger Logger { get; }
    private LoggingLevelSwitch LoggingLevelSwitch { get; }
    private IConfigService ConfigService { get; }
    private IFileSystem FileSystem { get; }
    private IDialogService DialogService { get; }
    private IAutoGameService AutoGameService { get; }
    private IUpdateCheckingService UpdateCheckingService { get; }

    public ISoftwareCollection AvailableSoftware { get; }

    public IAsyncRelayCommand BrowseSoftwarePathCommand { get; }

    public IAsyncRelayCommand LoadedCommand { get; }

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

    private async Task Loaded()
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
                this.UpdateInfo = await this.UpdateCheckingService.GetUpdateInfo();
            }
        }
        catch (Exception ex)
        {
            this.Logger.Error(ex, "handling application loaded");
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
                InitialDirectory = this.FileSystem.Path.GetDirectoryName(this.Config.SoftwarePath),
                FilterName = software.Description,
                FilterPatterns = [string.IsNullOrEmpty(executable) ? "*.exe" : executable]
            };

            if (string.IsNullOrEmpty(parms.InitialDirectory))
            {
                parms = parms with { InitialDirectory = this.FileSystem.Path.GetDirectoryName(defaultPath) };
            }

            if (await this.DialogService.ShowOpenFileDialog(parms) is { } selectedFileName)
            {
                this.Config.SoftwarePath = selectedFileName;
            }
        }
        catch (Exception ex)
        {
            this.Logger.Error(ex, "browsing for a software path");
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
            this.Logger.Error(ex, "handling OK");
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
            this.Logger.Error(ex, "handling Cancel");
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
        this.LoggingLevelSwitch.MinimumLevel = c.EnableTraceLogging
            ? LogEventLevel.Verbose
            : LogEventLevel.Information;

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
            this.Logger.Error(ex, "handling Apply");
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
            this.Logger.Error(ex, "handling config property changed");
        }
    }
}