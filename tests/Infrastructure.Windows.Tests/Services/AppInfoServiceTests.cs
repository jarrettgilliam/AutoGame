﻿namespace AutoGame.Infrastructure.Windows.Tests.Services;

using System;
using System.IO;
using System.IO.Abstractions;
using AutoGame.Infrastructure.Windows.Services;
using Moq;
using Xunit;

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

    [Fact]
    public void ParsecLogDirectories_Has_CorrectEntries()
    {
        // These paths were copied from:
        // https://support.parsec.app/hc/en-us/articles/360003145951-Accessing-Your-Advanced-Settings
        Assert.Collection(this.sut.ParsecLogDirectories,
            x => Assert.Equal(x, Environment.ExpandEnvironmentVariables(@"%appdata%\Parsec")),
            x => Assert.Equal(x, Environment.ExpandEnvironmentVariables(@"%programdata%\Parsec")));
    }
}