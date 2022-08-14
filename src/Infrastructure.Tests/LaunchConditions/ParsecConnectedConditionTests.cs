namespace AutoGame.Infrastructure.Tests.LaunchConditions;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;
using AutoGame.Infrastructure.LaunchConditions;

public class ParsecConnectedConditionTests
{
    private const int PARSECD_PROC_ID = 9999;
    private const int OTHER_PROC_ID = 8888;

    private readonly ParsecConnectedCondition sut;
    private readonly Mock<ILoggingService> loggingServiceMock = new();
    private readonly Mock<INetStatPortsService> netStatPortsServiceMock = new();
    private readonly Mock<ISleepService> sleepServiceMock = new();
    private readonly Mock<IProcessService> processServiceMock = new();
    private readonly Mock<IProcess> processMock = new();
    private readonly Mock<IFileSystem> fileSystemMock = new();
    private readonly Mock<IPath> pathMock = new();
    private readonly Mock<IDirectory> directoryMock = new();
    private readonly Mock<IFileSystemWatcherFactory> fileSystemWatcherFactoryMock = new();
    private readonly Mock<IFileSystemWatcher> fileSystemWatcherMock = new();

    private readonly List<Port> netstatPorts;

    private const string ParsecLogFileName = "log.txt";

    private static readonly string ParsecLogDirectory =
        Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Parsec");

    private bool suppressConditionMet;

    public ParsecConnectedConditionTests()
    {
        this.netstatPorts = new List<Port>
        {
            new() { Protocol = "UDP", ProcessId = PARSECD_PROC_ID },
            new() { Protocol = "UDP", ProcessId = PARSECD_PROC_ID },
            new() { Protocol = "UDP", ProcessId = PARSECD_PROC_ID }
        };

        this.netStatPortsServiceMock
            .Setup(x => x.GetUdpPorts())
            .Returns(this.netstatPorts);

        this.processMock
            .SetupGet(x => x.Id)
            .Returns(PARSECD_PROC_ID);

        this.processServiceMock
            .Setup(x => x.GetProcessesByName(It.IsAny<string?>()))
            .Returns(() => this.suppressConditionMet
                ? new DisposableList<IProcess>()
                : new DisposableList<IProcess> { this.processMock.Object });

        this.fileSystemMock
            .SetupGet(x => x.Path)
            .Returns(this.pathMock.Object);

        this.fileSystemMock
            .SetupGet(x => x.Directory)
            .Returns(this.directoryMock.Object);

        this.fileSystemMock
            .SetupGet(x => x.FileSystemWatcher)
            .Returns(this.fileSystemWatcherFactoryMock.Object);

        this.fileSystemWatcherFactoryMock
            .Setup(x => x.CreateNew(ParsecLogDirectory, ParsecLogFileName))
            .Returns(this.fileSystemWatcherMock.Object);

        this.pathMock
            .Setup(x => x.Join(
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns<string, string>(Path.Join);

        this.sut = new ParsecConnectedCondition(
            this.loggingServiceMock.Object,
            this.netStatPortsServiceMock.Object,
            this.processServiceMock.Object,
            this.fileSystemMock.Object);
    }

    [Fact]
    public void HasActiveUDPPorts_Fires_ConditionMet()
    {
        using var helper = new LaunchConditionTestHelper(this.sut);

        Assert.Equal(1, helper.FiredCount);
    }

    [Fact]
    public void HasActiveUDPPorts_Disposes_ParsecdProcesses()
    {
        using var helper = new LaunchConditionTestHelper(this.sut);

        this.processMock.Verify(x => x.Dispose(), Times.Exactly(1));
    }

    [Fact]
    public void NetStatNoPorts_DoesntFire_ConditionMet()
    {
        this.netstatPorts.Clear();

        using var helper = new LaunchConditionTestHelper(this.sut);

        Assert.Equal(0, helper.FiredCount);
    }

    [Fact]
    public void NetStatTwoPorts_DoesntFire_ConditionMet()
    {
        this.netstatPorts.RemoveAt(2);
        
        Assert.Equal(2, this.netstatPorts.Count);

        using var helper = new LaunchConditionTestHelper(this.sut);

        Assert.Equal(0, helper.FiredCount);
    }

    [Fact]
    public void NetStatProcessIdMismatch_DoesntFire_ConditionMet()
    {
        for (int i = 0; i < this.netstatPorts.Count; i++)
        {
            this.netstatPorts[i] = this.netstatPorts[i] with { ProcessId = OTHER_PROC_ID };
        }

        using var helper = new LaunchConditionTestHelper(this.sut);

        Assert.Equal(0, helper.FiredCount);
    }

    [Fact]
    public void NetStatNoUDPPorts_DoesntFire_ConditionMet()
    {
        for (int i = 0; i < this.netstatPorts.Count; i++)
        {
            this.netstatPorts[i] = this.netstatPorts[i] with { Protocol = "TCP" };
        }

        using var helper = new LaunchConditionTestHelper(this.sut);

        Assert.Equal(0, helper.FiredCount);
    }

    [Fact]
    public void ConnectionAlreadyDetected_DoesntFire_ConditionMet()
    {
        using var helper = new LaunchConditionTestHelper(this.sut);

        Assert.Equal(1, helper.FiredCount);

        this.fileSystemWatcherMock.Raise(x => x.Changed += null,
            new FileSystemEventArgs(WatcherChangeTypes.Changed, "", ""));

        Assert.Equal(1, helper.FiredCount);
    }

    [Fact]
    public void DisplaySettingsChanged_Fires_ConditionMet()
    {
        this.suppressConditionMet = true;
        using var helper = new LaunchConditionTestHelper(this.sut);
        this.suppressConditionMet = false;

        Assert.Equal(0, helper.FiredCount);

        this.fileSystemWatcherMock.Raise(x => x.Changed += null,
            new FileSystemEventArgs(WatcherChangeTypes.Changed, "", ""));

        Assert.Equal(1, helper.FiredCount);
    }

    [Fact]
    public void OnParsecLogWatcherEvent_Fires_ConditionMet()
    {
        this.suppressConditionMet = true;
        using var helper = new LaunchConditionTestHelper(this.sut);
        this.suppressConditionMet = false;

        Assert.Equal(0, helper.FiredCount);

        this.fileSystemWatcherMock.VerifySet(x => x.EnableRaisingEvents = true, Times.Once);

        this.fileSystemWatcherMock.Raise(x => x.Changed += null,
            new FileSystemEventArgs(WatcherChangeTypes.Changed, "", ""));

        Assert.Equal(1, helper.FiredCount);
    }

    [Fact]
    public void WatchParsecLogFile_CreatesDirectory()
    {
        using var helper = new LaunchConditionTestHelper(this.sut);

        this.directoryMock.Verify(x => x.CreateDirectory(ParsecLogDirectory), Times.Once);
    }

    [Fact]
    public void StopMonitoring_Works()
    {
        this.sut.StartMonitoring();
        this.sut.StopMonitoring();

        this.fileSystemWatcherMock.VerifyRemove(
            x => x.Changed -= It.IsAny<FileSystemEventHandler>(), Times.Once);

        this.fileSystemWatcherMock.Verify(x => x.Dispose(), Times.Once);
    }

    [Fact]
    public void StopMonitoring_ResetsWasConnected()
    {
        using var helper = new LaunchConditionTestHelper(this.sut);

        Assert.Equal(1, helper.FiredCount);

        this.sut.StopMonitoring();
        this.sut.StartMonitoring();

        Assert.Equal(2, helper.FiredCount);
    }
}