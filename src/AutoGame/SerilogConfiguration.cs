namespace AutoGame;

using System;
using System.Diagnostics;
using System.IO;
using AutoGame.Core.Interfaces;
using AutoGame.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using SerilogTraceListener;

internal static class SerilogConfiguration
{
    private const string OutputTemplate =
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}";

    internal static void ConfigureInitialLogger()
    {
        var appInfo = new AppInfoService();

        // The Serilog file sink doesn't support truncating log files at startup
        Exception? ex = TryDeleteFile(appInfo.LogFilePath);

        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(
                path: appInfo.LogFilePath,
                outputTemplate: OutputTemplate)
            .WriteToPlatformSystemLog(OutputTemplate)
            .CreateLogger();

        if (ex is not null)
        {
            Log.Warning(ex, "Unable to delete log.txt");
        }
    }

    internal static void ConfigureFullLogger(ServiceProvider serviceProvider)
    {
        var messageBoxSink = serviceProvider.GetService<IMessageBoxSink>();
        var logSwitch = serviceProvider.GetService<LoggingLevelSwitch>();

        ArgumentNullException.ThrowIfNull(messageBoxSink);
        ArgumentNullException.ThrowIfNull(logSwitch);

        var logConfig = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(logSwitch)
            .WriteTo.Sink(messageBoxSink);

        if (Log.Logger is ILogEventSink currentLogger)
        {
            logConfig.WriteTo.Sink(currentLogger);
        }

        Log.Logger = logConfig.CreateLogger();

        Trace.Listeners.Add(new SerilogTraceListener(Log.Logger));
    }

    private static Exception? TryDeleteFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}