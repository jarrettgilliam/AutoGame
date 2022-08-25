namespace AutoGame.Core.Services;

using System;
using System.IO;
using System.IO.Abstractions;
using AutoGame.Core.Enums;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;

internal sealed class LoggingService : ILoggingService
{
    private readonly Lazy<StreamWriter> logWriter;

    public LoggingService(
        IAppInfoService appInfo,
        IDateTimeService dateTimeService,
        IFileSystem fileSystem,
        IDialogService dialogService)
    {
        this.AppInfo = appInfo;
        this.DateTimeService = dateTimeService;
        this.FileSystem = fileSystem;
        this.DialogService = dialogService;

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
    private IDialogService DialogService { get; }

    public bool EnableTraceLogging { get; set; }

    public void Log(string message, LogLevel level)
    {
        try
        {
            if (level == LogLevel.Trace && !this.EnableTraceLogging)
            {
                return;
            }

            this.logWriter.Value.WriteLine($"{this.DateTimeService.NowOffset} {level}: {message}");
        }
        catch (Exception ex)
        {
            this.ShowExceptionDialog("writing a log entry", ex);
        }
    }

    public void LogException(string message, Exception exception)
    {
        this.ShowExceptionDialog(message, exception);
        this.Log($"{message}: {exception}", LogLevel.Error);
    }

    private void ShowExceptionDialog(string message, Exception exception)
    {
        this.DialogService.ShowMessageBox(new MessageBoxParms
        {
            Message = exception.ToString(),
            Title = $"{LogLevel.Error} {message}",
            Icon = LogLevel.Error
        });
    }

    public void Dispose()
    {
        if (this.logWriter.IsValueCreated)
        {
            this.logWriter.Value.Dispose();
        }
    }
}