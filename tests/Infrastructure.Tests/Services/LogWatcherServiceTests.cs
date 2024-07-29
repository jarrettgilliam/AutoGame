namespace AutoGame.Infrastructure.Tests.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoGame.Infrastructure.Services;
using Serilog;

public sealed class LogWatcherServiceTests
{
    private readonly Mock<IFileSystem> fileSystemMock = new();
    private readonly Mock<IPath> pathMock = new();
    private readonly Mock<IDirectory> directoryMock = new();
    private readonly Mock<IFile> fileMock = new();
    private readonly Mock<IFileSystemWatcherFactory> fileSystemWatcherFactoryMock = new();
    private readonly Mock<IFileSystemWatcher> fileSystemWatcherMock = new();
    private readonly Mock<ILogger> loggerMock = new();

    private readonly LogWatcherService sut;

    private const string LogFilePath = @"C:\logs\log.txt";

    private List<string> logEntries = new()
    {
        "[F 2024-07-21 20:08:04] ===== Parsec: Started =====",
        "[I 2024-07-21 20:11:32] STUN reply from ::ffff:9.999.999.99:9999",
        "[D 2024-07-21 20:11:33] net           = BUD|::ffff:999.999.9.999|99999",
        "[D 2024-07-21 20:11:33] BUD AES_GCM   = 256",
        "[D 2024-07-21 20:11:33] display_x     = 3840",
        "[D 2024-07-21 20:11:33] display_y     = 2160",
        "[D 2024-07-21 20:11:33] display_hz    = 60",
        "[I 2024-07-21 20:11:33] username#1234567 connected.",
        "[D 2024-07-21 20:09:50] display_x     = 3840",
        "[D 2024-07-21 20:09:50] display_y     = 2160",
        "[D 2024-07-21 20:09:50] display_hz    = 144",
        "[I 2024-07-21 20:09:50] Virtual tablet removed due to client disconnect",
        "[I 2024-07-21 20:09:50] username#1234567 disconnected.",
    };

    public LogWatcherServiceTests()
    {
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

        this.fileMock
            .Setup(x => x.OpenText(LogFilePath))
            .Returns(() =>
                new StreamReader(
                    new MemoryStream(
                        Encoding.UTF8.GetBytes(
                            string.Join(Environment.NewLine, this.logEntries)))));

        this.fileSystemWatcherFactoryMock
            .Setup(x => x.New(LogFilePath))
            .Returns(this.fileSystemWatcherMock.Object);

        this.sut = new LogWatcherService(
            this.fileSystemMock.Object,
            this.loggerMock.Object);
    }

    [Fact]
    public void Should_Watch_File_Path_When_Monitoring_Is_Started()
    {
        // Arrange
        // Act
        this.sut.StartMonitoring(LogFilePath);

        // Assert
        this.fileSystemWatcherFactoryMock
            .Verify(x => x.New(LogFilePath), Times.Once);
        this.fileSystemWatcherMock
            .VerifySet(x => x.EnableRaisingEvents = true, Times.Once);
    }

    [Fact]
    public void Should_Throw_ArgumentException_When_Monitoring_Is_Started_With_Empty_File_Path()
    {
        Assert.Throws<ArgumentException>(() => this.sut.StartMonitoring(string.Empty));
    }

    [Fact]
    public void Should_Throw_ArgumentNullException_When_Monitoring_Is_Started_With_Null_File_Path()
    {
        Assert.Throws<ArgumentNullException>(() => this.sut.StartMonitoring(null!));
    }

    [Fact]
    public void Should_Throw_Exception_When_Monitoring_Is_Started_With_NonExisting_File_Path()
    {
        // Arrange
        this.fileMock
            .Setup(x => x.Exists(It.IsAny<string?>()))
            .Returns(false);

        // Act
        // Assert
        Assert.Throws<ArgumentException>(() => this.sut.StartMonitoring(LogFilePath));
    }

    [Fact]
    public void Should_Send_All_Log_Entries_When_Monitoring_Is_Started()
    {
        // Arrange
        List<string> receivedLogEntries = [];
        this.sut.LogEntriesAdded += (_, entries) => receivedLogEntries.AddRange(entries);

        // Act
        this.sut.StartMonitoring(LogFilePath);

        // Assert
        Assert.Equal(this.logEntries, receivedLogEntries);
    }

    [Fact]
    public void Should_Fire_LogEntriesAdded_Event_For_New_Entries_When_Log_File_Is_Updated()
    {
        // Arrange
        this.sut.StartMonitoring(LogFilePath);

        List<string> receivedLogEntries = [];
        this.sut.LogEntriesAdded += (_, entries) => receivedLogEntries.AddRange(entries);

        List<string> newLogEntries =
        [
            "[I 2024-07-21 20:12:00] New log entry",
            "[I 2024-07-21 20:12:01] Another new log entry"
        ];

        // Act
        this.logEntries.AddRange(newLogEntries);
        this.RaiseLogChangedEvent();

        // Assert
        Assert.Equal(newLogEntries, receivedLogEntries);
    }

    [Fact]
    public void Should_Fire_LogEntriesAdded_Event_For_All_Entries_When_Log_File_Is_Truncated()
    {
        // Arrange
        this.sut.StartMonitoring(LogFilePath);

        List<string> receivedLogEntries = [];
        this.sut.LogEntriesAdded += (_, entries) => receivedLogEntries.AddRange(entries);

        this.logEntries =
        [
            "[I 2024-07-21 20:12:00] New log entry",
            "[I 2024-07-21 20:12:01] Another new log entry"
        ];

        // Act
        this.RaiseLogChangedEvent();

        // Assert
        Assert.Equal(this.logEntries, receivedLogEntries);
    }

    [Fact]
    public void Should_Stop_Watching_File_Path_When_Monitoring_Stopped()
    {
        // Arrange
        this.sut.StartMonitoring(LogFilePath);

        List<string> receivedLogEntries = [];
        this.sut.LogEntriesAdded +=
            (_, entries) => receivedLogEntries.AddRange(entries);

        List<string> newLogEntries =
        [
            "[I 2024-07-21 20:12:00] New log entry",
            "[I 2024-07-21 20:12:01] Another new log entry"
        ];

        // Act
        this.sut.StopMonitoring();
        this.logEntries.AddRange(newLogEntries);
        this.RaiseLogChangedEvent();

        // Assert
        this.fileSystemWatcherMock.VerifySet(x => x.EnableRaisingEvents = false, Times.Once);
        this.fileSystemWatcherMock.VerifyRemove(x => x.Changed -= It.IsAny<FileSystemEventHandler>(), Times.Once);
        this.fileSystemWatcherMock.Verify(x => x.Dispose(), Times.Once);
        Assert.Empty(receivedLogEntries);
    }

    [Fact]
    public async Task Should_Handle_Log_File_Changes_With_Thread_Safety()
    {
        // Arrange
        List<string> newLogEntries = this.logEntries;
        this.logEntries = [];
        this.sut.StartMonitoring(LogFilePath);
        this.logEntries.AddRange(newLogEntries);

        List<string> receivedLogEntries = [];
        this.sut.LogEntriesAdded += (_, entries) =>
        {
            lock (receivedLogEntries)
            {
                receivedLogEntries.AddRange(entries);
            }
        };

        this.fileMock
            .Setup(x => x.OpenText(LogFilePath))
            .Returns(() =>
                new SlowStreamReader(
                    new MemoryStream(
                        Encoding.UTF8.GetBytes(
                            string.Join(Environment.NewLine, this.logEntries)))));

        // Act
        await Task.WhenAll(
            Task.Run(this.RaiseLogChangedEvent),
            Task.Run(this.RaiseLogChangedEvent));

        // Assert
        Assert.Equal(this.logEntries.Count, receivedLogEntries.Count);
        Assert.Equal(this.logEntries, receivedLogEntries);
    }

    private void RaiseLogChangedEvent()
    {
        this.fileSystemWatcherMock.Raise(
            x => x.Changed += null,
            new FileSystemEventArgs(WatcherChangeTypes.Changed, "", ""));
    }

    private class SlowStreamReader : StreamReader
    {
        public SlowStreamReader(Stream stream) : base(stream)
        {
        }

        public override string? ReadLine()
        {
            Thread.Sleep(1);
            return base.ReadLine();
        }
    }
}