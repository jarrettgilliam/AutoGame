namespace AutoGame.Infrastructure.LaunchConditions;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using AutoGame.Core.Enums;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;

internal sealed class ParsecConnectedCondition : IParsecConnectedCondition
{
    private readonly object checkConditionLock = new();
    private bool wasConnected;

    private readonly string ParsecLogFileName;
    private readonly string ParsecLogDirectory;
    private IFileSystemWatcher? parsecLogWatcher;

    public ParsecConnectedCondition(
        ILoggingService loggingService,
        INetStatPortsService netStatPortsService,
        IProcessService processService,
        IFileSystem fileSystem)
    {
        this.LoggingService = loggingService;
        this.NetStatPortsService = netStatPortsService;
        this.ProcessService = processService;
        this.FileSystem = fileSystem;

        this.ParsecLogFileName = "log.txt";
        this.ParsecLogDirectory = this.FileSystem.Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Parsec");
    }

    public event EventHandler? ConditionMet;

    private ILoggingService LoggingService { get; }
    private INetStatPortsService NetStatPortsService { get; }
    private IProcessService ProcessService { get; }
    private IFileSystem FileSystem { get; }

    public void StartMonitoring()
    {
        this.WatchParsecLogFile();
        this.CheckConditionMet();
    }

    public void StopMonitoring()
    {
        this.StopWatchingParsecLogFile();
        this.wasConnected = false;
    }

    private void WatchParsecLogFile()
    {
        this.FileSystem.Directory.CreateDirectory(this.ParsecLogDirectory);

        this.parsecLogWatcher = this.FileSystem.FileSystemWatcher.CreateNew(
            this.ParsecLogDirectory, this.ParsecLogFileName);

        this.parsecLogWatcher.Changed += this.OnParsecLogWatcherEvent;
        this.parsecLogWatcher.EnableRaisingEvents = true;
    }

    private void StopWatchingParsecLogFile()
    {
        if (this.parsecLogWatcher != null)
        {
            this.parsecLogWatcher.Changed -= this.OnParsecLogWatcherEvent;
            this.parsecLogWatcher.Dispose();
            this.parsecLogWatcher = null;
        }
    }

    private void OnParsecLogWatcherEvent(object sender, FileSystemEventArgs e)
    {
        try
        {
            this.CheckConditionMet();
        }
        catch (Exception ex)
        {
            this.LoggingService.LogException("handling a Parsec log file event", ex);
        }
    }

    private IDisposableList<IProcess> GetParsecdProcesses() =>
        this.ProcessService.GetProcessesByName("parsecd");

    private void CheckConditionMet([CallerMemberName] string? source = null)
    {
        if (Monitor.TryEnter(this.checkConditionLock))
        {
            try
            {
                bool isConnected = this.GetIsConnected();
                bool fired = false;

                if (!this.wasConnected && isConnected)
                {
                    fired = true;
                    this.ConditionMet?.Invoke(this, EventArgs.Empty);
                }

                this.Trace(() => $"from {source}; " +
                                 $"{nameof(isConnected)}={isConnected}; " +
                                 $"{nameof(this.wasConnected)}={this.wasConnected}; " +
                                 $"{nameof(fired)}={fired}");

                this.wasConnected = isConnected;
            }
            finally
            {
                Monitor.Exit(this.checkConditionLock);
            }
        }
    }

    private bool GetIsConnected()
    {
        using IDisposableList<IProcess> parsecProcs = this.GetParsecdProcesses();

        return this.HasMoreThanTwoActiveUDPPorts(parsecProcs);
    }

    private bool HasMoreThanTwoActiveUDPPorts(IList<IProcess> parsecProcs)
    {
        IList<Port> ports = this.NetStatPortsService.GetUdpPorts();

        int count = ports.Count(p => this.IsParsecUDPPort(p, parsecProcs));
        bool result = count > 2;

        this.Trace(() => $"found {count} ports; returned {result}");
        return result;
    }

    private bool IsParsecUDPPort(Port port, IList<IProcess> parsecProcs) =>
        port.Protocol == "UDP" && parsecProcs.Any(proc => proc.Id == port.ProcessId);

    private void Trace(Func<string> message, [CallerMemberName] string? member = null)
    {
        if (this.LoggingService.EnableTraceLogging)
        {
            this.LoggingService.Log($"{nameof(ParsecConnectedCondition)}.{member} {message()}", LogLevel.Trace);
        }
    }
}