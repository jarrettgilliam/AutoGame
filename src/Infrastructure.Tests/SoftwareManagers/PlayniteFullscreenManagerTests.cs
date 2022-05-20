namespace AutoGame.Infrastructure.Tests.SoftwareManagers;

using System;
using System.IO;
using System.IO.Abstractions;
using AutoGame.Core.Interfaces;
using AutoGame.Infrastructure.SoftwareManagers;
using Moq;
using Xunit;

public class PlayniteFullscreenManagerTests
{
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
    public void IsRunning_ReturnsTrue()
    {
        this.processServiceMock
            .Setup(x => x.GetProcessesByName("Playnite.FullscreenApp"))
            .Returns(new[] { this.processMock.Object });

        Assert.True(this.sut.IsRunning);
    }

    [Fact]
    public void IsRunning_ReturnsFalse()
    {
        this.processServiceMock
            .Setup(x => x.GetProcessesByName(It.IsAny<string?>()))
            .Returns(Array.Empty<IProcess>());

        Assert.False(this.sut.IsRunning);
    }

    [Fact]
    public void Start_StartsProcess()
    {
        string softwarePath = "/test/playnite/software/path/Playnite.FullscreenApp.exe";

        this.sut.Start(softwarePath);

        this.processServiceMock.Verify(x => x.Start(softwarePath), Times.Once);
    }

    [Fact]
    public void Start_SetsForegroundWindow()
    {
        this.sut.Start("");
        
        this.windowServiceMock.Verify(
            x => x.RepeatTryForceForegroundWindowByTitle("Playnite", It.IsAny<TimeSpan>()),
            Times.Once());
    }

    [Fact]
    public void FindSoftwarePathOrDefault_ReturnsDefaultPath()
    {
        string defaultPath = Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Playnite",
            "Playnite.FullscreenApp.exe");

        Assert.Equal(defaultPath, this.sut.FindSoftwarePathOrDefault());
    }
}