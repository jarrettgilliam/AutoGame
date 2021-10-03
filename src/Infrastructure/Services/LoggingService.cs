using System;
using System.IO;
using AutoGame.Infrastructure.Constants;
using AutoGame.Infrastructure.Enums;
using AutoGame.Infrastructure.Interfaces;

namespace AutoGame.Infrastructure.Services
{
    public class LoggingService : ILoggingService
    {
        private static readonly string LogPath =
            Path.Join(Strings.AppDataFolder, "Log.txt");

        private readonly Lazy<StreamWriter> logWriter;

        public LoggingService()
        {
            this.logWriter = new Lazy<StreamWriter>(() => 
                new StreamWriter(LogPath, append: false)
                {
                    AutoFlush = true
                });
        }

        public bool EnableTraceLogging { get; set; }

        public void Log(string message, LogLevel level)
        {
            if (level == LogLevel.Trace && !this.EnableTraceLogging)
            {
                return;
            }

            this.logWriter.Value.WriteLine($"{DateTime.Now} {level}: {message}");
        }

        public void Dispose()
        {
            if (this.logWriter.IsValueCreated)
            {
                this.logWriter.Value.Dispose();
            }
        }
    }
}
