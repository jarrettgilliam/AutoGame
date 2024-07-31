namespace AutoGame.Infrastructure.Windows.Tests.Services;

using System;
using System.IO;
using Xunit;

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

    [Fact]
    public void ParsecLogDirectories_Has_CorrectEntries()
    {
        // These paths were copied from:
        // https://support.parsec.app/hc/en-us/articles/360003145951-Accessing-Your-Advanced-Settings
        Assert.Collection(this.sut.ParsecLogFiles,
            x => Assert.Equal(x, Environment.ExpandEnvironmentVariables(@"%appdata%\Parsec")),
            x => Assert.Equal(x, Environment.ExpandEnvironmentVariables(@"%programdata%\Parsec")));
    }
}