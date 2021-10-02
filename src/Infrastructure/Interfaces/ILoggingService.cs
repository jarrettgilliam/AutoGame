using System;
using AutoGame.Infrastructure.Enums;

namespace AutoGame.Infrastructure.Interfaces
{
    public interface ILoggingService : IDisposable
    {
        void Log(string message, LogLevel level);

        void LogException(string message, Exception exception) =>
            this.Log($"{message}: {exception}", LogLevel.Error);
    }
}
