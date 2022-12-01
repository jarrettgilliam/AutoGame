namespace AutoGame.Infrastructure.Windows.Tests.SoftwareManagers;

using System;
using System.IO;
using System.IO.Abstractions;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;
using AutoGame.Infrastructure.Windows.Interfaces;
using AutoGame.Infrastructure.Windows.SoftwareManagers;
using Moq;
using Xunit;

public class PlayniteFullscreenManagerTests
{
    private const string SOFTWARE_NAME = "Playnite.FullscreenApp";
    private const string SOFTWARE_PATH = $"/default/path/to/{SOFTWARE_NAME}.exe";

    private readonly PlayniteFullscreenManager sut;
    private readonly Mock<IWindowService> windowServiceMock = new();
    private readonly Mock<IFileSystem> fileSystemMock = new();
    private readonly Mock<IPath> pathMock = new();
    private readonly Mock<IProcessService> processServiceMock = new();
    private readonly Mock<IProcess> processMock = new();

    public PlayniteFullscreenManagerTests()
    {
        this.pathMock
            .Setup(x => x.Join(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns<string, string, string>(Path.Join);

        this.fileSystemMock
            .SetupGet(x => x.Path)
            .Returns(this.pathMock.Object);

        this.processServiceMock
            .Setup(x => x.Start(It.IsAny<string>(), It.IsAny<string?>()))
            .Returns(this.processMock.Object);

        this.sut = new PlayniteFullscreenManager(
            this.windowServiceMock.Object,
            this.fileSystemMock.Object,
            this.processServiceMock.Object);
    }

    [Fact]
    public void Key_IsCorrect()
    {
        Assert.Equal("PlayniteFullscreen", this.sut.Key);
    }

    [Fact]
    public void Description_IsCorrect()
    {
        Assert.Equal("Playnite Fullscreen", this.sut.Description);
    }

    [Fact]
    public void DefaultArguments_IsCorrect()
    {
        Assert.Equal("--startfullscreen", this.sut.DefaultArguments);
    }

    [Fact]
    public void IsRunning_ReturnsTrue()
    {
        this.processServiceMock
            .Setup(x => x.GetProcessesByName(SOFTWARE_NAME))
            .Returns(new DisposableList<IProcess> { this.processMock.Object });

        Assert.True(this.sut.IsRunning(SOFTWARE_PATH));
    }

    [Fact]
    public void IsRunning_ReturnsFalse()
    {
        this.processServiceMock
            .Setup(x => x.GetProcessesByName(It.IsAny<string?>()))
            .Returns(new DisposableList<IProcess>());

        Assert.False(this.sut.IsRunning(SOFTWARE_PATH));
    }

    [Fact]
    public void IsRunning_DisposesProcesses()
    {
        this.processServiceMock
            .Setup(x => x.GetProcessesByName(SOFTWARE_NAME))
            .Returns(new DisposableList<IProcess> { this.processMock.Object });

        this.sut.IsRunning(SOFTWARE_PATH);

        this.processMock.Verify(x => x.Dispose(), Times.Once);
    }

    [Fact]
    public void Start_StartsProcess()
    {
        string customArgs = "--my-custom-arguments";
        this.sut.Start(SOFTWARE_PATH, customArgs);

        this.processServiceMock.Verify(x => x.Start(SOFTWARE_PATH, customArgs), Times.Once);
    }

    [Fact]
    public void Start_SetsForegroundWindow()
    {
        this.sut.Start("", "");

        this.windowServiceMock.Verify(
            x => x.RepeatTryForceForegroundWindowByTitle("Playnite", It.IsAny<TimeSpan>()),
            Times.Once());
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
        string defaultPath = Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Playnite",
            $"{SOFTWARE_NAME}.exe");

        Assert.Equal(defaultPath, this.sut.FindSoftwarePathOrDefault());
    }
}