namespace AutoGame.Core.Services;

using System;
using System.IO;
using System.IO.Abstractions;
using AutoGame.Core.Enums;
using AutoGame.Core.Interfaces;

internal sealed class LoggingService : ILoggingService
{
    private readonly Lazy<StreamWriter> logWriter;

    public LoggingService(
        IAppInfoService appInfo,
        IDateTimeService dateTimeService,
        IFileSystem fileSystem)
    {
        this.AppInfo = appInfo;
        this.DateTimeService = dateTimeService;
        this.FileSystem = fileSystem;
        
        this.logWriter = new Lazy<StreamWriter>(() =>
        {
            this.FileSystem.Directory.CreateDirectory(this.AppInfo.AppDataFolder);

            StreamWriter sw = this.FileSystem.File.CreateText(this.AppInfo.LogFilePath);
            sw.AutoFlush = true;

            return sw;
        });
    }

    private IAppInfoService AppInfo { get; }
    private IDateTimeService DateTimeService { get; }
    private IFileSystem FileSystem { get; }

    public bool EnableTraceLogging { get; set; }

    public void Log(string message, LogLevel level)
    {
        if (level == LogLevel.Trace && !this.EnableTraceLogging)
        {
            return;
        }

        this.logWriter.Value.WriteLine($"{this.DateTimeService.NowOffset} {level}: {message}");
    }

    public void Dispose()
    {
        if (this.logWriter.IsValueCreated)
        {
            this.logWriter.Value.Dispose();
        }
    }
}