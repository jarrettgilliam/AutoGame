namespace AutoGame.Infrastructure.macOS.Tests.Services;

using System.IO;
using System.IO.Abstractions;
using AutoGame.Infrastructure.macOS.Services;

public class AppInfoServiceTests
{
    private readonly AppInfoService sut;
    private readonly Mock<IFileSystem> fileSystemMock = new();
    private readonly Mock<IPath> pathMock = new();

    public AppInfoServiceTests()
    {
        this.pathMock
            .Setup(x => x.Join(
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns<string, string>(Path.Join);

        this.pathMock
            .Setup(x => x.Join(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns<string, string, string, string>(Path.Join);

        this.fileSystemMock
            .SetupGet(x => x.Path)
            .Returns(this.pathMock.Object);

        this.sut = new AppInfoService(
            this.fileSystemMock.Object);
    }

    [Fact]
    public void ConfigFilePath_InsideAppDataFolder()
    {
        Assert.Equal(
            this.sut.AppDataFolder,
            Path.GetDirectoryName(this.sut.ConfigFilePath));
    }

    [Fact]
    public void LogFilePath_InsideAppDataFolder()
    {
        Assert.Equal(
            this.sut.AppDataFolder,
            Path.GetDirectoryName(this.sut.LogFilePath));
    }
}