namespace AutoGame.Infrastructure.Tests.SoftwareManagers;

using System;
using System.IO;
using System.IO.Abstractions;
using AutoGame.Core.Interfaces;
using AutoGame.Infrastructure.SoftwareManagers;

public class OtherSoftwareManagerTests
{
    private const string SOFTWARE_NAME = "anything";
    private const string SOFTWARE_PATH = $"/default/path/to/{SOFTWARE_NAME}.exe";
    
    private readonly OtherSoftwareManager sut;
    private readonly Mock<IProcessService> processServiceMock = new();
    private readonly Mock<IProcess> processMock = new();
    private readonly Mock<IFileSystem> fileSystemMock = new();
    private readonly Mock<IPath> pathMock = new();

    public OtherSoftwareManagerTests()
    {
        this.fileSystemMock
            .SetupGet(x => x.Path)
            .Returns(this.pathMock.Object);

        this.pathMock
            .Setup(x => x.GetFileName(It.IsAny<string>()))
            .Returns<string>(Path.GetFileName);

        this.pathMock
            .Setup(x => x.ChangeExtension(It.IsAny<string>(), It.IsAny<string>()))
            .Returns<string, string>(Path.ChangeExtension);

        this.sut = new OtherSoftwareManager(
            this.processServiceMock.Object,
            this.fileSystemMock.Object);
    }

    [Fact]
    public void Key_IsCorrect()
    {
        Assert.Equal("Other", this.sut.Key);
    }

    [Fact]
    public void Description_IsCorrect()
    {
        Assert.Equal("Other", this.sut.Description);
    }

    [Fact]
    public void IsRunning_ReturnsTrue()
    {
        this.processServiceMock
            .Setup(x => x.GetProcessesByName(SOFTWARE_NAME))
            .Returns(new[] { this.processMock.Object });

        Assert.True(this.sut.IsRunning(SOFTWARE_PATH));
    }

    [Fact]
    public void IsRunning_ReturnsFalse()
    {
        this.processServiceMock
            .Setup(x => x.GetProcessesByName(It.IsAny<string?>()))
            .Returns(Array.Empty<IProcess>());

        Assert.False(this.sut.IsRunning(SOFTWARE_PATH));
    }

    [Fact]
    public void Start_StartsProcess()
    {
        this.sut.Start(SOFTWARE_PATH);

        this.processServiceMock.Verify(x => x.Start(SOFTWARE_PATH), Times.Once);
    }
}