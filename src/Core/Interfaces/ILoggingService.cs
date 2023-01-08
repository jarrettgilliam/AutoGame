namespace AutoGame.Core.Interfaces;

using System;
using AutoGame.Core.Enums;

public interface ILoggingService : IDisposable
{
    bool EnableTraceLogging { get; set; }

    void Log(string message, LogLevel level);

    void LogException(string message, Exception exception, LogLevel logLevel = LogLevel.Error);
}