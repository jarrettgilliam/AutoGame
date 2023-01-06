namespace AutoGame.Infrastructure.Tests.LaunchConditions;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
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
    private readonly Mock<IProcessService> processServiceMock = new();
    private readonly Mock<IProcess> processMock = new();
    private readonly Mock<IFileSystem> fileSystemMock = new();
    private readonly Mock<IPath> pathMock = new();
    private readonly Mock<IDirectory> directoryMock = new();
    private readonly Mock<IFile> fileMock = new();
    private readonly Mock<IFileSystemWatcherFactory> fileSystemWatcherFactoryMock = new();

    private readonly List<Mock<IFileSystemWatcher>> fileSystemWatcherMocks = new();

    private readonly Mock<IFileSystemWatcher> fileSystemWatcherMock1 = new();
    private readonly Mock<IAppInfoService> appInfoServiceMock = new();
    private readonly Mock<IRuntimeInformation> runtimeInformationMock = new();

    private readonly List<Port> netstatPorts;

    private const string ParsecLogFileName = "log.txt";

    private static readonly IList<string> ParsecLogDirectories = new[]
    {
        Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Parsec"),
        Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Parsec")
    };

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
            .SetupGet(x => x.File)
            .Returns(this.fileMock.Object);

        this.fileSystemMock
            .SetupGet(x => x.FileSystemWatcher)
            .Returns(this.fileSystemWatcherFactoryMock.Object);

        this.fileMock
            .Setup(x => x.Exists(It.IsAny<string?>()))
            .Returns(true);

        foreach (string parsecLogDirectory in ParsecLogDirectories)
        {
            var fswm = new Mock<IFileSystemWatcher>();

            this.fileSystemWatcherFactoryMock
                .Setup(x => x.New(parsecLogDirectory, ParsecLogFileName))
                .Returns(fswm.Object);

            this.fileSystemWatcherMocks.Add(fswm);
        }

        this.pathMock
            .Setup(x => x.Join(
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns<string, string>(Path.Join);

        this.appInfoServiceMock
            .SetupGet(x => x.ParsecLogDirectories)
            .Returns(ParsecLogDirectories);

        this.runtimeInformationMock
            .Setup(x => x.IsOSPlatform(OSPlatform.Windows))
            .Returns(true);

        this.sut = new ParsecConnectedCondition(
            this.loggingServiceMock.Object,
            this.netStatPortsServiceMock.Object,
            this.processServiceMock.Object,
            this.fileSystemMock.Object,
            this.appInfoServiceMock.Object,
            this.runtimeInformationMock.Object);
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

        this.fileSystemWatcherMocks[0].Raise(x => x.Changed += null,
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

        foreach (Mock<IFileSystemWatcher> fswm in this.fileSystemWatcherMocks)
        {
            fswm.VerifySet(x => x.EnableRaisingEvents = true, Times.Once);

            fswm.Raise(x => x.Changed += null,
                new FileSystemEventArgs(WatcherChangeTypes.Changed, "", ""));
        }

        Assert.Equal(1, helper.FiredCount);
    }

    [Fact]
    public void WatchParsecLogFiles_Doesnt_Create_Directory()
    {
        using var helper = new LaunchConditionTestHelper(this.sut);

        this.directoryMock.Verify(x => x.CreateDirectory(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void WatchParsecLogFiles_Doesnt_Watch_Nonexistent_File()
    {
        this.fileMock
            .Setup(x => x.Exists(It.IsAny<string?>()))
            .Returns(false);

        using var helper = new LaunchConditionTestHelper(this.sut);

        this.fileSystemWatcherFactoryMock.Verify(
            x => x.New(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public void StopMonitoring_Works()
    {
        this.sut.StartMonitoring();
        this.sut.StopMonitoring();

        foreach (Mock<IFileSystemWatcher> fswm in this.fileSystemWatcherMocks)
        {
            fswm.VerifyRemove(x => x.Changed -= It.IsAny<FileSystemEventHandler>(), Times.Once);
            fswm.Verify(x => x.Dispose(), Times.Once);
        }

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