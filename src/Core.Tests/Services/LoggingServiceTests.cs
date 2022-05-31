namespace AutoGame.Core.Tests.Services;

using System.IO.Abstractions;
using System.Text;
using AutoGame.Core.Enums;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;
using AutoGame.Core.Services;
using Xunit.Sdk;

public class LoggingServiceTests
{
    private readonly LoggingService sut;
    private readonly Mock<IAppInfoService> appInfoServiceMock = new();
    private readonly Mock<IDateTimeService> dateTimeServiceMock = new();
    private readonly Mock<IFileSystem> fileSystemMock = new();
    private readonly Mock<IFile> fileMock = new();
    private readonly Mock<IDirectory> directoryMock = new();
    private readonly Mock<IDialogService> dialogServiceMock = new();

    private MemoryStream logMemoryStream = new();

    public LoggingServiceTests()
    {
        this.appInfoServiceMock
            .SetupGet(x => x.AppDataFolder)
            .Returns(@"C:\AutoGame");

        this.appInfoServiceMock
            .SetupGet(x => x.LogFilePath)
            .Returns(@"C:\AutoGame\Log.txt");

        this.fileMock
            .Setup(x => x.CreateText(this.appInfoServiceMock.Object.LogFilePath))
            .Returns(new StreamWriter(this.logMemoryStream));

        this.fileSystemMock
            .SetupGet(x => x.File)
            .Returns(this.fileMock.Object);

        this.fileSystemMock
            .SetupGet(x => x.Directory)
            .Returns(this.directoryMock.Object);

        this.sut = new LoggingService(
            this.appInfoServiceMock.Object,
            this.dateTimeServiceMock.Object,
            this.fileSystemMock.Object,
            this.dialogServiceMock.Object);
    }

    [Fact]
    public void Log_Error_Writes()
    {
        DateTimeOffset dtOffset = DateTimeOffset.MinValue;
        const LogLevel level = LogLevel.Error;
        const string error = "Error message";

        this.sut.Log(error, LogLevel.Error);

        Assert.Equal(
            $"{dtOffset} {level}: {error}{Environment.NewLine}",
            Encoding.UTF8.GetString(this.logMemoryStream.ToArray()));
    }

    [Fact]
    public void Log_EnableTraceLoggingTrue_WritesTraceLogs()
    {
        this.sut.EnableTraceLogging = true;
        this.sut.Log("Trace log", LogLevel.Trace);
        Assert.NotEqual(0, this.logMemoryStream.Length);
    }

    [Fact]
    public void Log_EnableTraceLoggingFalse_SkipsTraceLogs()
    {
        this.sut.EnableTraceLogging = false;
        this.sut.Log("Trace log", LogLevel.Trace);
        Assert.Equal(0, this.logMemoryStream.Length);
    }

    [Fact]
    public void Log_CreatesDirectory()
    {
        this.sut.Log("", LogLevel.Error);

        this.directoryMock.Verify(
            x => x.CreateDirectory(this.appInfoServiceMock.Object.AppDataFolder),
            Times.Once);
    }

    [Fact]
    public void Log_GetsDateTimeOffset()
    {
        this.sut.Log("", LogLevel.Error);

        this.dateTimeServiceMock
            .Verify(x => x.NowOffset, Times.Once);
    }

    [Fact]
    public void Log_FileWriteException_ShowsErrorDialog()
    {
        this.fileMock
            .Setup(x => x.CreateText(this.appInfoServiceMock.Object.LogFilePath))
            .Returns(() => throw new Exception("test exception"));

        this.sut.Log("message", LogLevel.Error);

        this.dialogServiceMock.Verify(
            x => x.ShowMessageBox(It.IsAny<MessageBoxParms>()),
            Times.Once);
    }

    [Fact]
    public void LogException_LogsAllInput()
    {
        string message = "This is the error message";
        Exception exception = new("This is the exception message");

        this.sut.LogException(message, exception);

        string logEntry = Encoding.UTF8.GetString(this.logMemoryStream.ToArray());

        Assert.Contains(message, logEntry);
        Assert.Contains(exception.ToString(), logEntry);
        Assert.Contains(LogLevel.Error.ToString(), logEntry);
    }

    [Fact]
    public void LogException_ShowsMessageBox()
    {
        this.sut.LogException("message", new Exception("exception"));

        this.dialogServiceMock.Verify(
            x => x.ShowMessageBox(It.IsAny<MessageBoxParms>()),
            Times.Once);
    }

    [Fact]
    public void Dispose_Works()
    {
        this.sut.Log("", LogLevel.Error);
        this.sut.Dispose();
        Assert.False(this.logMemoryStream.CanWrite);
    }
}