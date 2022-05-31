namespace AutoGame.Tests.ViewModels;

using System.IO.Abstractions;
using System.Reflection;
using System.Windows;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;
using AutoGame.ViewModels;
using Newtonsoft.Json;

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
        LaunchWhenGamepadConnected = true,
        LaunchWhenParsecConnected = true,
        IsDirty = false
    };

    private readonly Config defaultConfigMock = new()
    {
        EnableTraceLogging = false,
        SoftwareKey = SoftwareKey,
        SoftwarePath = Path.Join(DefaultDirectory, ExecutableName),
        LaunchWhenGamepadConnected = true,
        LaunchWhenParsecConnected = true,
        IsDirty = false
    };

    private OpenFileDialogParms openFileDialogParms = new();

    private bool canApplyConfiguration = true;
    private bool fileSelected = true;

    public MainWindowViewModelTests()
    {
        this.configServiceMock
            .Setup(x => x.GetConfigOrNull())
            .Returns(() => this.savedConfigMock);

        this.configServiceMock
            .Setup(x => x.CreateDefault(It.IsAny<ISoftwareManager>()))
            .Returns(() =>
                JsonConvert.DeserializeObject<Config>(
                    JsonConvert.SerializeObject(this.defaultConfigMock))!);

        this.configServiceMock
            .Setup(x => x.Validate(It.IsAny<Config>(), It.IsAny<IEnumerable<ISoftwareManager>>()))
            .Callback<Config, IEnumerable<ISoftwareManager>>((config, _) =>
            {
                if (!this.canApplyConfiguration)
                {
                    config.AddError("property", "Error message");
                }
            });

        this.autoGameServiceMock
            .Setup(x => x.GetSoftwareByKeyOrNull(It.IsAny<string?>()))
            .Returns(this.softwareManagerMock.Object);

        this.autoGameServiceMock
            .SetupGet(x => x.AvailableSoftware)
            .Returns(new[] { this.softwareManagerMock.Object });

        this.softwareManagerMock
            .Setup(x => x.FindSoftwarePathOrDefault())
            .Returns(this.defaultConfigMock.SoftwarePath);

        this.softwareManagerMock
            .SetupGet(x => x.Description)
            .Returns(SoftwareDescription);

        this.dialogServiceMock
            .Setup(x => x.ShowOpenFileDialog(It.IsAny<OpenFileDialogParms>(), out It.Ref<string?>.IsAny))
            .Callback((OpenFileDialogParms parms, out string? selectedFileName) =>
            {
                this.openFileDialogParms = parms;
                selectedFileName = this.savedConfigMock.SoftwarePath;
            })
            .Returns(() => this.fileSelected);

        this.fileSystemMock
            .SetupGet(x => x.Path)
            .Returns(this.pathMock.Object);

        this.pathMock
            .Setup(x => x.GetFileName(It.IsAny<string>()))
            .Returns<string>(Path.GetFileName);

        this.pathMock
            .Setup(x => x.GetDirectoryName(It.IsAny<string>()))
            .Returns<string>(Path.GetDirectoryName!);

        this.sut = new MainWindowViewModel(
            this.loggingServiceMock.Object,
            this.configServiceMock.Object,
            this.autoGameServiceMock.Object,
            this.fileSystemMock.Object,
            this.dialogServiceMock.Object);
    }

    [Theory]
    [InlineData(nameof(MainWindowViewModel.AutoGameService))]
    public void Property_IsPublic(string propertyName)
    {
        PropertyInfo? prop = typeof(MainWindowViewModel)
            .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);

        Assert.NotNull(prop);
        Assert.True(prop!.CanRead);
    }

    [Fact]
    public void OnLoaded_TryLoadConfigFalse_CreatesDefaultConfiguration()
    {
        this.configServiceMock.Setup(x => x.GetConfigOrNull()).Returns(() => null);
        List<string?> propertyChanges = new();
        this.sut.PropertyChanged += (_, e) => propertyChanges.Add(e.PropertyName);
        this.configServiceMock.Verify(
            x => x.CreateDefault(It.IsAny<ISoftwareManager>()),
            Times.Exactly(1));

        this.sut.LoadedCommand.Execute(null);

        this.configServiceMock.Verify(
            x => x.CreateDefault(It.IsAny<ISoftwareManager>()),
            Times.Exactly(2));

        Assert.Equal(1, propertyChanges.Count(p => p == nameof(this.sut.Config)));
    }

    [Fact]
    public void OnLoaded_TryLoadConfigFalse_DoesntSaveConfiguration()
    {
        this.configServiceMock.Setup(x => x.GetConfigOrNull()).Returns(() => null);

        this.sut.LoadedCommand.Execute(null);

        this.configServiceMock.Verify(x => x.Save(It.IsAny<Config>()), Times.Never);
    }

    [Fact]
    public void OnLoaded_TryLoadConfigFalse_DoesntApplyConfiguration()
    {
        this.configServiceMock.Setup(x => x.GetConfigOrNull()).Returns(() => null);

        this.sut.LoadedCommand.Execute(null);

        this.autoGameServiceMock.Verify(x => x.ApplyConfiguration(It.IsAny<Config>()), Times.Never);
    }

    [Fact]
    public void OnLoaded_TryLoadConfigFalse_DoesntMinimize()
    {
        this.configServiceMock.Setup(x => x.GetConfigOrNull()).Returns(() => null);

        this.sut.LoadedCommand.Execute(null);

        Assert.NotEqual(WindowState.Minimized, this.sut.WindowState);
    }

    [Fact]
    public void OnLoaded_TryLoadConfigTrue_AppliesConfiguration()
    {
        this.sut.LoadedCommand.Execute(null);

        this.autoGameServiceMock.Verify(x => x.ApplyConfiguration(this.savedConfigMock), Times.Once);
    }

    [Fact]
    public void OnLoaded_TryLoadConfigTrue_DoesntLoadDefaultConfig()
    {
        List<string?> propertyChanges = new();
        this.sut.PropertyChanged += (_, e) => propertyChanges.Add(e.PropertyName);

        this.configServiceMock.Verify(x => x.CreateDefault(It.IsAny<ISoftwareManager>()), Times.Once);

        this.sut.LoadedCommand.Execute(null);

        this.configServiceMock.Verify(x => x.CreateDefault(It.IsAny<ISoftwareManager>()), Times.Once);
        Assert.Equal(1, propertyChanges.Count(p => p == nameof(this.sut.Config)));
    }

    [Fact]
    public void OnLoaded_TryLoadConfigTrue_MinimizesWindow()
    {
        this.sut.LoadedCommand.Execute(null);

        Assert.Equal(WindowState.Minimized, this.sut.WindowState);
    }

    [Fact]
    public void OnLoaded_TryApplyConfigurationFalse_DoesntMinimizeWindow()
    {
        this.canApplyConfiguration = false;

        this.sut.LoadedCommand.Execute(null);

        Assert.NotEqual(WindowState.Minimized, this.sut.WindowState);
    }

    [Fact]
    public void OnNotifyIconClick_RestoresWindow()
    {
        this.sut.WindowState = WindowState.Minimized;

        this.sut.NotifyIconClickCommand.Execute(null);

        Assert.Equal(WindowState.Normal, this.sut.WindowState);
    }

    [Fact]
    public void OnBrowseSoftwarePath_FullSoftwarePath_SetsPropertiesCorrectly()
    {
        this.sut.Config = this.savedConfigMock;
        this.sut.BrowseSoftwarePathCommand.Execute(null);

        Assert.Equal(ExecutableName, this.openFileDialogParms.FileName);
        Assert.Equal(CustomDirectory, this.openFileDialogParms.InitialDirectory);
        Assert.Equal($"{SoftwareDescription}|{ExecutableName}", this.openFileDialogParms.Filter);
    }

    [Fact]
    public void OnBrowseSoftwarePath_EmptySoftwarePath_SetsDefaultInitialDirectory()
    {
        this.savedConfigMock.SoftwarePath = null;

        this.sut.Config = this.savedConfigMock;
        this.sut.BrowseSoftwarePathCommand.Execute(null);

        Assert.Equal(DefaultDirectory, this.openFileDialogParms.InitialDirectory);
    }

    [Fact]
    public void OnBrowseSoftwarePath_FileSelected_SetsSoftwarePath()
    {
        Assert.Equal(this.defaultConfigMock.SoftwarePath, this.sut.Config.SoftwarePath);

        this.fileSelected = true;
        this.sut.BrowseSoftwarePathCommand.Execute(null);

        Assert.Equal(this.savedConfigMock.SoftwarePath, this.sut.Config.SoftwarePath);
    }

    [Fact]
    public void OnBrowseSoftwarePath_FileNotSelected_DoesntSetSoftwarePath()
    {
        Assert.Equal(this.defaultConfigMock.SoftwarePath, this.sut.Config.SoftwarePath);

        this.fileSelected = false;
        this.sut.BrowseSoftwarePathCommand.Execute(null);

        Assert.Equal(this.defaultConfigMock.SoftwarePath, this.sut.Config.SoftwarePath);
    }

    [Fact]
    public void OnOK_CanApply_MinimizesWindow()
    {
        this.sut.WindowState = WindowState.Normal;
        this.sut.OKCommand.Execute(null);
        Assert.Equal(WindowState.Minimized, this.sut.WindowState);
    }

    [Fact]
    public void OnOK_CannotApply_DoesntMinimizeWindow()
    {
        this.canApplyConfiguration = false;
        this.sut.WindowState = WindowState.Normal;
        this.sut.OKCommand.Execute(null);
        Assert.Equal(WindowState.Normal, this.sut.WindowState);
    }

    [Fact]
    public void OnCancel_RestoresConfig()
    {
        this.sut.CancelCommand.Execute(null);
        this.configServiceMock.Verify(x => x.GetConfigOrNull(), Times.Once);
    }

    [Fact]
    public void OnCancel_ValidatesConfig()
    {
        this.sut.CancelCommand.Execute(null);
        
        this.configServiceMock.Verify(
            x => x.Validate(It.IsAny<Config>(), It.IsAny<IEnumerable<ISoftwareManager>>()),
            Times.Once);
    }

    [Fact]
    public void TryLoadConfig_SetsTraceLogging()
    {
        this.sut.CancelCommand.Execute(null);

        this.loggingServiceMock.VerifySet(
            x => x.EnableTraceLogging = true, Times.Once());
    }

    [Fact]
    public void OnCancel_MinimizesWindow()
    {
        this.sut.WindowState = WindowState.Normal;
        this.sut.CancelCommand.Execute(null);
        Assert.Equal(WindowState.Minimized, this.sut.WindowState);
    }

    [Fact]
    public void OnApply_ConfigNotDirty_DoNothing()
    {
        this.sut.Config = this.savedConfigMock;
        this.sut.Config.IsDirty = false;

        this.sut.ApplyCommand.Execute(null);

        this.autoGameServiceMock.Verify(
            x => x.ApplyConfiguration(It.IsAny<Config>()),
            Times.Never);
    }

    [Fact]
    public void OnApply_ConfigIsDirty_ApplyConfig()
    {
        this.sut.Config = this.savedConfigMock;
        this.sut.Config.IsDirty = true;

        this.sut.ApplyCommand.Execute(null);

        this.autoGameServiceMock.Verify(
            x => x.ApplyConfiguration(It.IsAny<Config>()),
            Times.Once);
    }

    [Fact]
    public void OnApply_ConfigIsDirty_SaveConfig()
    {
        this.sut.Config = this.savedConfigMock;
        this.sut.Config.IsDirty = true;

        this.sut.ApplyCommand.Execute(null);

        this.configServiceMock.Verify(
            x => x.Save(It.IsAny<Config>()), Times.Once);
    }

    [Fact]
    public void OnApply_ApplyFailed_DontSaveConfig()
    {
        this.canApplyConfiguration = false;

        this.sut.ApplyCommand.Execute(null);

        this.configServiceMock.Verify(
            x => x.Save(It.IsAny<Config>()), Times.Never);
    }

    [Fact]
    public void OnConfigSoftwareKeyChanged_SoftwareKeyChanged_UpdateSoftwarePath()
    {
        this.savedConfigMock.SoftwareKey = null;
        this.savedConfigMock.SoftwarePath = null;
        this.sut.Config = this.savedConfigMock;

        this.sut.Config.SoftwareKey = SoftwareKey;

        Assert.Equal(this.defaultConfigMock.SoftwarePath, this.sut.Config.SoftwarePath);
    }

    [Fact]
    public void OnConfigSoftwareKeyChanged_OtherChanged_DoNothing()
    {
        this.savedConfigMock.SoftwareKey = null;
        this.savedConfigMock.SoftwarePath = null;
        this.sut.Config = this.savedConfigMock;

        this.sut.Config.LaunchWhenParsecConnected = !this.sut.Config.LaunchWhenParsecConnected;

        Assert.Null(this.sut.Config.SoftwarePath);
    }

    [Fact]
    public void OnConfigSoftwareKeyChanged_Startup_Subscribed()
    {
        this.sut.Config.SoftwareKey = "NewKey";

        this.autoGameServiceMock.Verify(
            x => x.GetSoftwareByKeyOrNull(It.IsAny<string?>()),
            Times.Once);
    }

    [Fact]
    public void OnConfigSoftwareKeyChanged_Changed_SubscriptionMoved()
    {
        Config oldConfig = this.sut.Config;
        this.sut.Config = this.savedConfigMock;

        oldConfig.SoftwareKey = "NewKey";

        this.autoGameServiceMock.Verify(
            x => x.GetSoftwareByKeyOrNull(It.IsAny<string?>()),
            Times.Never);
    }

    [Fact]
    public void SetWindowState_Restore_PropertyOrderCorrect()
    {
        this.sut.WindowState = WindowState.Minimized;
        this.sut.ShowWindow = false;
        this.sut.NotifyIconVisible = true;

        List<string?> propertyChanges = new();
        this.sut.PropertyChanged += (_, args) => propertyChanges.Add(args.PropertyName);

        this.sut.NotifyIconClickCommand.Execute(null);

        Assert.Collection(propertyChanges,
            p => Assert.Equal(nameof(this.sut.NotifyIconVisible), p),
            p => Assert.Equal(nameof(this.sut.ShowWindow), p),
            p => Assert.Equal(nameof(this.sut.WindowState), p));
    }

    [Fact]
    public void SetWindowState_Minimize_PropertyOrderCorrect()
    {
        this.sut.NotifyIconVisible = false;
        this.sut.ShowWindow = true;
        this.sut.WindowState = WindowState.Normal;

        List<string?> propertyChanges = new();
        this.sut.PropertyChanged += (_, args) => propertyChanges.Add(args.PropertyName);

        this.configServiceMock.Setup(x => x.GetConfigOrNull()).Returns(() => null);
        this.sut.CancelCommand.Execute(null);

        Assert.Collection(propertyChanges,
            p => Assert.Equal(nameof(this.sut.WindowState), p),
            p => Assert.Equal(nameof(this.sut.ShowWindow), p),
            p => Assert.Equal(nameof(this.sut.NotifyIconVisible), p));
    }

    [Fact]
    public void SetWindowState_Minimize_PropertyValueCorrect()
    {
        this.sut.WindowState = WindowState.Normal;
        this.sut.ShowWindow = true;
        this.sut.NotifyIconVisible = false;

        this.sut.CancelCommand.Execute(null);

        Assert.Equal(WindowState.Minimized, this.sut.WindowState);
        Assert.False(this.sut.ShowWindow);
        Assert.True(this.sut.NotifyIconVisible);
    }

    [Fact]
    public void SetWindowState_Restore_PropertyValueCorrect()
    {
        this.sut.WindowState = WindowState.Minimized;
        this.sut.ShowWindow = false;
        this.sut.NotifyIconVisible = true;

        this.sut.NotifyIconClickCommand.Execute(null);

        Assert.Equal(WindowState.Normal, this.sut.WindowState);
        Assert.True(this.sut.ShowWindow);
        Assert.False(this.sut.NotifyIconVisible);
    }

    [Fact]
    public void Dispose_Works()
    {
        this.sut.Dispose();
        this.autoGameServiceMock.Verify(x => x.Dispose(), Times.Once);
        this.loggingServiceMock.Verify(x => x.Dispose(), Times.Once);
    }
}