namespace AutoGame.Infrastructure.Windows.Tests.SoftwareManagers;

using System;
using System.IO;
using System.IO.Abstractions;
using AutoGame.Core.Interfaces;
using AutoGame.Infrastructure.Windows.Interfaces;
using AutoGame.Infrastructure.Windows.SoftwareManagers;
using Moq;
using Serilog;
using Xunit;

public class OneGameLauncherManagerTests
{
    private const string SOFTWARE_NAME = "OneGameLauncher";
    private const string SOFTWARE_PATH = $"/default/path/to/{SOFTWARE_NAME}.exe";

    private readonly OneGameLauncherManager sut;
    private readonly Mock<ILogger> loggerMock = new();
    private readonly Mock<IUser32Service> user32ServiceMock = new();
    private readonly Mock<IFileSystem> fileSystemMock = new();
    private readonly Mock<IPath> pathMock = new();
    private readonly Mock<IProcessService> processServiceMock = new();
    private readonly Mock<IProcess> processMock = new();
    private readonly Mock<IRegistryService> registryServiceMock = new();

    public OneGameLauncherManagerTests()
    {
        this.pathMock
            .Setup(x => x.Join(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns<string, string, string>(Path.Join);

        this.pathMock
            .Setup(x => x.GetFullPath(It.IsAny<string>()))
            .Returns<string>(path => path);

        this.fileSystemMock
            .SetupGet(x => x.Path)
            .Returns(this.pathMock.Object);

        this.processServiceMock
            .Setup(x => x.Start(It.IsAny<string>(), It.IsAny<string?>()))
            .Returns(this.processMock.Object);

        this.sut = new OneGameLauncherManager(
            this.loggerMock.Object,
            this.user32ServiceMock.Object,
            this.fileSystemMock.Object,
            this.processServiceMock.Object,
            this.registryServiceMock.Object);
    }

    [Fact]
    public void Key_IsCorrect()
    {
        Assert.Equal("OneGameLauncher", this.sut.Key);
    }

    [Fact]
    public void Description_IsCorrect()
    {
        Assert.Equal("One Game Launcher", this.sut.Description);
    }

    [Fact]
    public void DefaultArguments_IsCorrect()
    {
        Assert.Equal(
            "shell:appsfolder\\62269AlexShats.OneGameLauncherBeta_gghb1w55myjr2!App",
            this.sut.DefaultArguments);
    }

    [Fact]
    public void IsRunning_ReturnsTrue()
    {
        this.user32ServiceMock
            .Setup(x => x.FindWindow("CUIEngineWin32", "GameLauncherWidget"))
            .Returns(IntPtr.MaxValue);

        Assert.True(this.sut.IsRunning(SOFTWARE_PATH));
    }

    [Fact]
    public void IsRunning_ReturnsFalse()
    {
        this.user32ServiceMock
            .Setup(x => x.FindWindow("CUIEngineWin32", "GameLauncherWidget"))
            .Returns(IntPtr.Zero);

        Assert.False(this.sut.IsRunning(SOFTWARE_PATH));
    }

    [Fact]
    public void Start_StartsProcess()
    {
        string customArgs = "--my-custom-arguments";
        this.sut.Start(SOFTWARE_PATH, customArgs);

        this.processServiceMock.Verify(
            x => x.Start(SOFTWARE_PATH, customArgs),
            Times.Once);
    }

    [Fact]
    public void Start_DisposesProcesses()
    {
        this.sut.Start(SOFTWARE_PATH, null);

        this.processMock.Verify(x => x.Dispose(), Times.Once);
    }

    [Fact]
    public void FindSoftwarePathOrDefault_ReturnsDefaultPath()
    {
        string defaultSteamPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");
         
        Assert.Equal(defaultSteamPath, this.sut.FindSoftwarePathOrDefault());
    }

     
}