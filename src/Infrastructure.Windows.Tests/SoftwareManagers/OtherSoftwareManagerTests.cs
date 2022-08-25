namespace AutoGame.Infrastructure.Windows.Tests.SoftwareManagers;

using System.IO;
using System.IO.Abstractions;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;
using AutoGame.Infrastructure.Windows.SoftwareManagers;
using Moq;
using Xunit;

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

        this.processServiceMock
            .Setup(x => x.Start(It.IsAny<string>(), It.IsAny<string?>()))
            .Returns(this.processMock.Object);

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
    public void DefaultArguments_IsCorrect()
    {
        Assert.Equal("", this.sut.DefaultArguments);
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
    public void Start_DisposesProcesses()
    {
        this.sut.Start(SOFTWARE_PATH, null);

        this.processMock.Verify(x => x.Dispose(), Times.Once);
    }
}