namespace AutoGame.Core.Tests.Services;

using System.IO.Abstractions;
using AutoGame.Core.Services;

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
            Directory.GetParent(
                this.sut.ConfigFilePath)?.FullName);
    }

    [Fact]
    public void LogFilePath_InsideAppDataFolder()
    {
        Assert.Equal(
            this.sut.AppDataFolder,
            Directory.GetParent(
                this.sut.LogFilePath)?.FullName);
    }
}