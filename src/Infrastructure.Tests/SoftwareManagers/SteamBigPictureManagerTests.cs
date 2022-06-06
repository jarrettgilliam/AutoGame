namespace AutoGame.Infrastructure.Tests.SoftwareManagers;

using System;
using System.IO;
using System.IO.Abstractions;
using AutoGame.Core.Interfaces;
using AutoGame.Infrastructure.SoftwareManagers;
using Moq;
using Xunit;

public class SteamBigPictureManagerTests
{
    private const string SOFTWARE_NAME = "steam";
    private const string SOFTWARE_PATH = $"/default/path/to/{SOFTWARE_NAME}.exe";

    private readonly SteamBigPictureManager sut;
    private readonly Mock<ILoggingService> loggingServiceMock = new();
    private readonly Mock<IUser32Service> user32ServiceMock = new();
    private readonly Mock<IFileSystem> fileSystemMock = new();
    private readonly Mock<IPath> pathMock = new();
    private readonly Mock<IProcessService> processServiceMock = new();
    private readonly Mock<IProcess> processMock = new();
    private readonly Mock<IRegistryService> registryServiceMock = new();
    
    public SteamBigPictureManagerTests()
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

        this.sut = new SteamBigPictureManager(
            this.loggingServiceMock.Object,
            this.user32ServiceMock.Object,
            this.fileSystemMock.Object,
            this.processServiceMock.Object,
            this.registryServiceMock.Object);
    }

    [Fact]
    public void Key_IsCorrect()
    {
        Assert.Equal("SteamBigPicture", this.sut.Key);
    }

    [Fact]
    public void Description_IsCorrect()
    {
        Assert.Equal("Steam Big Picture", this.sut.Description);
    }

    [Fact]
    public void DefaultArguments_IsCorrect()
    {
        Assert.Equal(
            "-start steam://open/bigpicture -fulldesktopres",
            this.sut.DefaultArguments);
    }

    [Fact]
    public void IsRunning_ReturnsTrue()
    {
        this.user32ServiceMock
            .Setup(x => x.FindWindow("CUIEngineWin32", "Steam"))
            .Returns(IntPtr.MaxValue);

        Assert.True(this.sut.IsRunning(SOFTWARE_PATH));
    }

    [Fact]
    public void IsRunning_ReturnsFalse()
    {
        this.user32ServiceMock
            .Setup(x => x.FindWindow("CUIEngineWin32", "Steam"))
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
        string defaultSteamPath = Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            "Steam",
            $"{SOFTWARE_NAME}.exe");

        Assert.Equal(defaultSteamPath, this.sut.FindSoftwarePathOrDefault());
    }

    [Fact]
    public void FindSoftwarePathOrDefault_ReturnsRegistryPath()
    {
        string registryPath = $"/custom/path/to/{SOFTWARE_NAME}.exe";

        this.registryServiceMock
            .Setup(x => x.GetValue(
                @"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam",
                "SteamExe",
                It.IsAny<string>()))
            .Returns(registryPath);

        Assert.Equal(registryPath, this.sut.FindSoftwarePathOrDefault());
    }
}