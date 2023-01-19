namespace AutoGame.Infrastructure.macOS.Tests.Services;

using System.IO;

public class AppInfoServiceTests
{
    private readonly AppInfoService sut;

    public AppInfoServiceTests()
    {
        this.sut = new AppInfoService();
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