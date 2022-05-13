namespace AutoGame.Infrastructure.Services;

using System;
using System.IO;
using AutoGame.Core.Enums;
using AutoGame.Core.Interfaces;

public class LoggingService : ILoggingService
{
    private readonly Lazy<StreamWriter> logWriter;

    public LoggingService(
        IAppInfoService appInfo,
        IDateTimeService dateTimeService)
    {
        this.AppInfo = appInfo;
        this.DateTimeService = dateTimeService;
        
        this.logWriter = new Lazy<StreamWriter>(() =>
        {
            Directory.CreateDirectory(this.AppInfo.AppDataFolder);

            return new StreamWriter(this.AppInfo.LogFilePath, append: false)
            {
                AutoFlush = true
            };
        });
    }

    private IAppInfoService AppInfo { get; }
    private IDateTimeService DateTimeService { get; }

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