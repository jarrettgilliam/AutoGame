namespace AutoGame.Tests.ViewModels;

using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;
using AutoGame.Core.Services;
using AutoGame.ViewModels;

public class MainWindowViewModelTests
{
    private readonly MainWindowViewModel sut;

    private readonly Mock<ILoggingService> loggingServiceMock = new();
    private readonly Mock<IConfigService> configServiceMock = new();
    private readonly Mock<IAutoGameService> autoGameServiceMock = new();
    private readonly Mock<ISoftwareManager> softwareManagerMock = new();
    private readonly Mock<IFileSystem> fileSystemMock = new();
    private readonly Mock<IPath> pathMock = new();
    private readonly Mock<IDialogService> dialogServiceMock = new();
    private readonly Mock<IUpdateCheckingService> updateCheckingServiceMock = new();
    private readonly Mock<SoftwareCollection> softwareCollectionMock;

    private const string SoftwareKey = "SteamBigPicture";
    private const string ExecutableName = "steam.exe";
    private const string DefaultDirectory = @"C:\Program Files (x86)\Steam";
    private const string CustomDirectory = @"D:\steam";
    private const string SoftwareDescription = "Steam Big Picture";

    private readonly Config savedConfigMock = new()
    {
        EnableTraceLogging = true,
        SoftwareKey = SoftwareKey,
        SoftwarePath = Path.Join(CustomDirectory, ExecutableName),
        LaunchWhenGameControllerConnected = true,
        LaunchWhenParsecConnected = true,
        IsDirty = false
    };

    private readonly Config defaultConfigMock = new()
    {
        EnableTraceLogging = false,
        SoftwareKey = SoftwareKey,
        SoftwarePath = Path.Join(DefaultDirectory, ExecutableName),
        SoftwareArguments = "--arguments",
        LaunchWhenGameControllerConnected = true,
        LaunchWhenParsecConnected = true,
        IsDirty = false
    };

    private OpenFileDialogParms openFileDialogParms;

    private bool canApplyConfiguration = true;
    private bool fileSelected = true;

    public MainWindowViewModelTests()
    {
        this.configServiceMock
            .Setup(x => x.GetConfigOrNull())
            .Returns(() => this.savedConfigMock);

        this.configServiceMock
            .Setup(x => x.CreateDefault())
            .Returns(() =>
                JsonSerializer.Deserialize<Config>(
                    JsonSerializer.Serialize(this.defaultConfigMock))!);

        this.configServiceMock
            .Setup(x => x.Validate(It.IsAny<Config>()))
            .Callback<Config>(config =>
            {
                if (!this.canApplyConfiguration)
                {
                    config.AddError("property", "Error message");
                }
            });

        this.softwareManagerMock
            .Setup(x => x.FindSoftwarePathOrDefault())
            .Returns(this.defaultConfigMock.SoftwarePath ?? "");

        this.softwareManagerMock
            .SetupGet(x => x.DefaultArguments)
            .Returns(this.defaultConfigMock.SoftwareArguments ?? "");

        this.softwareManagerMock
            .SetupGet(x => x.Description)
            .Returns(SoftwareDescription);

        this.softwareManagerMock
            .SetupGet(x => x.Key)
            .Returns(SoftwareKey);

        this.softwareCollectionMock = new(
            new object[]
            {
                new[] { this.softwareManagerMock.Object }
            });

        this.softwareCollectionMock.CallBase = true;

        this.dialogServiceMock
            .Setup(x => x.ShowOpenFileDialog(It.IsAny<OpenFileDialogParms>()))
            .Callback((OpenFileDialogParms parms) => { this.openFileDialogParms = parms; })
            .ReturnsAsync(() => this.fileSelected ? this.savedConfigMock.SoftwarePath : null);

        this.fileSystemMock
            .SetupGet(x => x.Path)
            .Returns(this.pathMock.Object);

        this.pathMock
            .Setup(x => x.GetFileName(It.IsAny<string>()))
            .Returns<string>(Path.GetFileName);

        this.pathMock
            .Setup(x => x.GetDirectoryName(It.IsAny<string>()))
            .Returns<string>(Path.GetDirectoryName);

        this.updateCheckingServiceMock
            .Setup(x => x.GetUpdateInfo())
            .Returns(Task.FromResult(new UpdateInfo()));

        this.sut = new MainWindowViewModel(
            this.loggingServiceMock.Object,
            this.configServiceMock.Object,
            this.autoGameServiceMock.Object,
            this.fileSystemMock.Object,
            this.dialogServiceMock.Object,
            this.updateCheckingServiceMock.Object,
            this.softwareCollectionMock.Object);
    }

    [Theory]
    [InlineData(nameof(MainWindowViewModel.AvailableSoftware))]
    public void Property_IsPublic(string propertyName)
    {
        PropertyInfo? prop = typeof(MainWindowViewModel)
            .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);

        Assert.NotNull(prop);
        Assert.True(prop.CanRead);
    }

    [Fact]
    public async Task Loaded_TryLoadConfigFalse_CreatesDefaultConfiguration()
    {
        this.configServiceMock.Setup(x => x.GetConfigOrNull()).Returns(() => null);
        List<string?> propertyChanges = new();
        this.sut.PropertyChanged += (_, e) => propertyChanges.Add(e.PropertyName);
        this.configServiceMock.Verify(
            x => x.CreateDefault(),
            Times.Exactly(1));

        await this.sut.LoadedCommand.ExecuteAsync(null);

        this.configServiceMock.Verify(
            x => x.CreateDefault(),
            Times.Exactly(2));

        Assert.Equal(1, propertyChanges.Count(p => p == nameof(this.sut.Config)));
    }

    [Fact]
    public async Task Loaded_TryLoadConfigFalse_DoesntSaveConfiguration()
    {
        this.configServiceMock.Setup(x => x.GetConfigOrNull()).Returns(() => null);

        await this.sut.LoadedCommand.ExecuteAsync(null);

        this.configServiceMock.Verify(x => x.Save(It.IsAny<Config>()), Times.Never);
    }

    [Fact]
    public async Task Loaded_TryLoadConfigFalse_DoesntApplyConfiguration()
    {
        this.configServiceMock.Setup(x => x.GetConfigOrNull()).Returns(() => null);

        await this.sut.LoadedCommand.ExecuteAsync(null);

        this.autoGameServiceMock.Verify(x => x.ApplyConfiguration(It.IsAny<Config>()), Times.Never);
    }

    [Fact]
    public async Task Loaded_TryLoadConfigFalse_DoesntMinimize()
    {
        this.configServiceMock.Setup(x => x.GetConfigOrNull()).Returns(() => null);

        await this.sut.LoadedCommand.ExecuteAsync(null);

        Assert.True(this.sut.ShowWindow);
    }

    [Fact]
    public async Task Loaded_TryLoadConfigTrue_AppliesConfiguration()
    {
        await this.sut.LoadedCommand.ExecuteAsync(null);

        this.autoGameServiceMock.Verify(x => x.ApplyConfiguration(this.savedConfigMock), Times.Once);
    }

    [Fact]
    public async Task Loaded_TryLoadConfigTrue_DoesntLoadDefaultConfig()
    {
        List<string?> propertyChanges = new();
        this.sut.PropertyChanged += (_, e) => propertyChanges.Add(e.PropertyName);

        this.configServiceMock.Verify(x => x.CreateDefault(), Times.Once);

        await this.sut.LoadedCommand.ExecuteAsync(null);

        this.configServiceMock.Verify(x => x.CreateDefault(), Times.Once);
        Assert.Equal(1, propertyChanges.Count(p => p == nameof(this.sut.Config)));
    }

    [Fact]
    public async Task Loaded_TryLoadConfigTrue_MinimizesWindow()
    {
        await this.sut.LoadedCommand.ExecuteAsync(null);

        Assert.False(this.sut.ShowWindow);
    }

    [Fact]
    public async Task Loaded_TryApplyConfigurationFalse_DoesntMinimizeWindow()
    {
        this.canApplyConfiguration = false;

        await this.sut.LoadedCommand.ExecuteAsync(null);

        Assert.True(this.sut.ShowWindow);
    }

    [Fact]
    public async Task Loaded_StartMinimizedFalse_DoesntMinimizeWindow()
    {
        this.savedConfigMock.StartMinimized = false;

        await this.sut.LoadedCommand.ExecuteAsync(null);

        Assert.True(this.sut.ShowWindow);
    }

    [Fact]
    public async Task Loaded_Gets_UpdateInfo()
    {
        await this.sut.LoadedCommand.ExecuteAsync(null);

        this.updateCheckingServiceMock.Verify(x => x.GetUpdateInfo(), Times.Once);
    }

    [Fact]
    public async Task Loaded_Doesnt_Get_UpdateInfo()
    {
        this.savedConfigMock.CheckForUpdates = false;

        await this.sut.LoadedCommand.ExecuteAsync(null);

        this.updateCheckingServiceMock.Verify(x => x.GetUpdateInfo(), Times.Never);
    }

    [Fact]
    public async Task OnBrowseSoftwarePath_FullSoftwarePath_SetsPropertiesCorrectly()
    {
        this.sut.Config = this.savedConfigMock;
        await this.sut.BrowseSoftwarePathCommand.ExecuteAsync(null);

        Assert.Equal(ExecutableName, this.openFileDialogParms.FileName);
        Assert.Equal(CustomDirectory, this.openFileDialogParms.InitialDirectory);
        Assert.Equal(SoftwareDescription, this.openFileDialogParms.FilterName);

        Assert.Collection(this.openFileDialogParms.FilterExtensions,
            x => Assert.Equal("exe", x));
    }

    [Fact]
    public async Task OnBrowseSoftwarePath_EmptySoftwarePath_SetsDefaultInitialDirectory()
    {
        this.savedConfigMock.SoftwarePath = null;

        this.sut.Config = this.savedConfigMock;
        await this.sut.BrowseSoftwarePathCommand.ExecuteAsync(null);

        Assert.Equal(DefaultDirectory, this.openFileDialogParms.InitialDirectory);
    }

    [Fact]
    public async Task OnBrowseSoftwarePath_FileSelected_SetsSoftwarePath()
    {
        Assert.Equal(this.defaultConfigMock.SoftwarePath, this.sut.Config.SoftwarePath);

        await this.sut.LoadedCommand.ExecuteAsync(null);
        this.fileSelected = true;
        await this.sut.BrowseSoftwarePathCommand.ExecuteAsync(null);

        Assert.Equal(this.savedConfigMock.SoftwarePath, this.sut.Config.SoftwarePath);
    }

    [Fact]
    public async Task OnBrowseSoftwarePath_FileNotSelected_DoesntSetSoftwarePath()
    {
        Assert.Equal(this.defaultConfigMock.SoftwarePath, this.sut.Config.SoftwarePath);

        this.fileSelected = false;
        await this.sut.BrowseSoftwarePathCommand.ExecuteAsync(null);

        Assert.Equal(this.defaultConfigMock.SoftwarePath, this.sut.Config.SoftwarePath);
    }

    [Fact]
    public async Task OnBrowseSoftwarePath_SoftwareNotFound_DontShowDialog()
    {
        this.softwareCollectionMock.Object.Clear();

        await this.sut.BrowseSoftwarePathCommand.ExecuteAsync(null);

        this.dialogServiceMock.Verify(
            x => x.ShowOpenFileDialog(It.IsAny<OpenFileDialogParms>()),
            Times.Never);
    }

    [Fact]
    public void OK_CanApply_MinimizesWindow()
    {
        this.sut.ShowWindow = true;
        this.sut.OKCommand.Execute(null);
        Assert.False(this.sut.ShowWindow);
    }

    [Fact]
    public void OK_CannotApply_DoesntMinimizeWindow()
    {
        this.canApplyConfiguration = false;
        this.sut.ShowWindow = true;
        this.sut.OKCommand.Execute(null);
        Assert.True(this.sut.ShowWindow);
    }

    [Fact]
    public async Task OK_HasErrorsButNotDirty_DoesntMinimizeWindow()
    {
        this.canApplyConfiguration = false;
        this.savedConfigMock.IsDirty = false;

        await this.sut.LoadedCommand.ExecuteAsync(null);
        this.sut.OKCommand.Execute(null);

        Assert.True(this.sut.ShowWindow);
    }

    [Fact]
    public void Cancel_RestoresConfig()
    {
        this.sut.CancelCommand.Execute(null);
        this.configServiceMock.Verify(x => x.GetConfigOrNull(), Times.Once);
    }

    [Fact]
    public void Cancel_ValidatesConfig()
    {
        this.sut.CancelCommand.Execute(null);

        this.configServiceMock.Verify(
            x => x.Validate(It.IsAny<Config>()),
            Times.Once);
    }

    [Fact]
    public void Cancel_Doesnt_Minimize_With_Errors()
    {
        this.savedConfigMock.AddError(nameof(Config.SoftwarePath), "test error");

        this.sut.CancelCommand.Execute(null);

        Assert.True(this.sut.ShowWindow);
    }

    [Fact]
    public void TryLoadConfig_SetsTraceLogging()
    {
        this.sut.CancelCommand.Execute(null);

        this.loggingServiceMock.VerifySet(
            x => x.EnableTraceLogging = true, Times.Once());
    }

    [Fact]
    public void TryLoadConfig_UpgradesConfig()
    {
        this.sut.CancelCommand.Execute(null);
        this.configServiceMock.Verify(x => x.Upgrade(this.savedConfigMock), Times.Once);
    }

    [Fact]
    public void Cancel_MinimizesWindow()
    {
        this.sut.ShowWindow = true;
        this.sut.CancelCommand.Execute(null);
        Assert.False(this.sut.ShowWindow);
    }

    [Fact]
    public void Apply_ConfigNotDirty_DoNothing()
    {
        this.sut.Config = this.savedConfigMock;
        this.sut.Config.IsDirty = false;

        this.sut.ApplyCommand.Execute(null);

        this.autoGameServiceMock.Verify(
            x => x.ApplyConfiguration(It.IsAny<Config>()),
            Times.Never);
    }

    [Fact]
    public void Apply_ConfigIsDirty_ApplyConfig()
    {
        this.sut.Config = this.savedConfigMock;
        this.sut.Config.IsDirty = true;

        this.sut.ApplyCommand.Execute(null);

        this.autoGameServiceMock.Verify(
            x => x.ApplyConfiguration(It.IsAny<Config>()),
            Times.Once);
    }

    [Fact]
    public void Apply_ConfigIsDirty_SaveConfig()
    {
        this.sut.Config = this.savedConfigMock;
        this.sut.Config.IsDirty = true;

        this.sut.ApplyCommand.Execute(null);

        this.configServiceMock.Verify(
            x => x.Save(It.IsAny<Config>()), Times.Once);
    }

    [Fact]
    public void Apply_ApplyFailed_DontSaveConfig()
    {
        this.canApplyConfiguration = false;

        this.sut.ApplyCommand.Execute(null);

        this.configServiceMock.Verify(
            x => x.Save(It.IsAny<Config>()), Times.Never);
    }

    [Fact]
    public void OnConfigPropertyChanged_SoftwareKeyChanged_UpdateSoftwarePath()
    {
        this.savedConfigMock.SoftwareKey = null;
        this.savedConfigMock.SoftwarePath = null;
        this.sut.Config = this.savedConfigMock;

        this.sut.Config.SoftwareKey = SoftwareKey;

        Assert.Equal(this.defaultConfigMock.SoftwarePath, this.sut.Config.SoftwarePath);
    }

    [Fact]
    public void OnConfigPropertyChanged_SoftwareKeyChanged_UpdateSoftwareArguments()
    {
        this.savedConfigMock.SoftwareKey = null;
        this.savedConfigMock.SoftwareArguments = null;
        this.sut.Config = this.savedConfigMock;

        this.sut.Config.SoftwareKey = SoftwareKey;

        Assert.Equal(
            this.defaultConfigMock.SoftwareArguments,
            this.sut.Config.SoftwareArguments);
    }

    [Fact]
    public void OnConfigPropertyChanged_OtherChanged_DoNothing()
    {
        this.sut.Config = this.savedConfigMock;
        this.savedConfigMock.SoftwareKey = null;
        this.savedConfigMock.SoftwarePath = null;

        this.sut.Config.LaunchWhenParsecConnected = !this.sut.Config.LaunchWhenParsecConnected;

        Assert.Null(this.sut.Config.SoftwarePath);
    }

    [Fact]
    public void OnConfigPropertyChanged_Startup_Subscribed()
    {
        this.sut.Config.SoftwareKey = "NewKey";

        this.softwareCollectionMock.Verify(
            x => x.GetSoftwareByKeyOrNull(It.IsAny<string?>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public void OnConfigPropertyChanged_SoftwareKeyChanged_SubscriptionMoved()
    {
        Config oldConfig = this.sut.Config;
        this.sut.Config = this.savedConfigMock;

        oldConfig.SoftwareKey = "NewKey";

        this.softwareCollectionMock.Verify(
            x => x.GetSoftwareByKeyOrNull(It.IsAny<string?>()),
            Times.Never);
    }

    [Fact]
    public void SetWindowState_Minimize_PropertyValueCorrect()
    {
        this.sut.ShowWindow = true;

        this.sut.CancelCommand.Execute(null);

        Assert.False(this.sut.ShowWindow);
    }
}