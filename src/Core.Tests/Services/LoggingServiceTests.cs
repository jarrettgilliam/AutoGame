namespace AutoGame.Core.Tests.Services;

using System.IO.Abstractions;
using System.Text;
using AutoGame.Core.Enums;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Services;

public class LoggingServiceTests
{
    private readonly LoggingService sut;
    private Mock<IAppInfoService> appInfoServiceMock = new();
    private Mock<IDateTimeService> dateTimeServiceMock = new();
    private Mock<IFileSystem> fileSystemMock = new();
    private readonly Mock<IFile> fileMock = new();
    private readonly Mock<IDirectory> directoryMock = new();

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
            this.fileSystemMock.Object);
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
    public void Dispose_Works()
    {
        this.sut.Log("", LogLevel.Error);
        this.sut.Dispose();
        Assert.False(this.logMemoryStream.CanWrite);
    }
}