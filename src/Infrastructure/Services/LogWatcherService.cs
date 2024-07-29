namespace AutoGame.Infrastructure.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using AutoGame.Core.Delegates;
using AutoGame.Core.Interfaces;
using Serilog;

internal sealed class LogWatcherService : ILogWatcherService
{
    private object locker = new();

    public LogWatcherService(
        IFileSystem fileSystem,
        ILogger logger)
    {
        this.FileSystem = fileSystem;
        this.Logger = logger;
    }

    private IFileSystem FileSystem { get; }

    private ILogger Logger { get; }

    private string? LogFilePath { get; set; }

    private long LogFilePosition { get; set; }

    private IFileSystemWatcher? FileSystemWatcher { get; set; }

    public event EventHandler<ILogWatcherService, IEnumerable<string>>? LogEntriesAdded;

    public void StartMonitoring(string filePath)
    {
        lock (this.locker)
        {
            ArgumentException.ThrowIfNullOrEmpty(filePath);

            if (this.FileSystem.File.Exists(filePath) == false)
            {
                throw new ArgumentException($"File does not exist: {filePath}");
            }

            this.LogFilePath = filePath;
            this.LogFilePosition = 0;
            this.FileSystemWatcher = this.FileSystem.FileSystemWatcher.New(filePath);
            this.FileSystemWatcher.Changed += this.OnLogFileChanged;
            this.FileSystemWatcher.EnableRaisingEvents = true;

            this.HandleLogFileChanged();
        }
    }

    private void OnLogFileChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            this.HandleLogFileChanged();
        }
        catch (Exception ex)
        {
            this.Logger.Error(ex, "Error while processing log file change event");
        }
    }

    private void HandleLogFileChanged()
    {
        lock (this.locker)
        {
            if (this.LogFilePath is null)
            {
                throw new InvalidOperationException("Log file path is not set");
            }

            using StreamReader fileStream = this.FileSystem.File.OpenText(this.LogFilePath);

            if (this.LogFilePosition > fileStream.BaseStream.Length)
            {
                this.LogFilePosition = 0;
            }

            fileStream.BaseStream.Seek(this.LogFilePosition, SeekOrigin.Begin);

            List<string> logEntries = [];
            while (fileStream.ReadLine() is { } line)
            {
                if (line != string.Empty)
                {
                    logEntries.Add(line);
                }
            }

            this.LogFilePosition = fileStream.BaseStream.Position;

            if (logEntries.Count != 0)
            {
                this.LogEntriesAdded?.Invoke(this, logEntries);
            }
        }
    }

    public void StopMonitoring()
    {
        lock (this.locker)
        {
            if (this.FileSystemWatcher is not null)
            {
                this.FileSystemWatcher.EnableRaisingEvents = false;
                this.FileSystemWatcher.Changed -= this.OnLogFileChanged;
                this.FileSystemWatcher.Dispose();
                this.FileSystemWatcher = null;
            }

            this.LogFilePath = null;
            this.LogFilePosition = 0;
        }
    }
}