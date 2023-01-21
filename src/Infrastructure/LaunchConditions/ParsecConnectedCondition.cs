namespace AutoGame.Infrastructure.LaunchConditions;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;
using Serilog;
using Serilog.Events;

internal sealed class ParsecConnectedCondition : IParsecConnectedCondition
{
    private const string ParsecLogFileName = "log.txt";

    private readonly object checkConditionLock = new();
    private readonly List<IFileSystemWatcher> parsecLogWatchers = new();
    private bool wasConnected;

    public ParsecConnectedCondition(
        ILogger logger,
        INetStatPortsService netStatPortsService,
        IProcessService processService,
        IFileSystem fileSystem,
        IAppInfoService appInfoService,
        IRuntimeInformation runtimeInformation)
    {
        this.Logger = logger;
        this.NetStatPortsService = netStatPortsService;
        this.ProcessService = processService;
        this.FileSystem = fileSystem;
        this.AppInfoService = appInfoService;
        this.RuntimeInformation = runtimeInformation;
    }

    public event EventHandler? ConditionMet;

    private ILogger Logger { get; }
    private INetStatPortsService NetStatPortsService { get; }
    private IProcessService ProcessService { get; }
    private IFileSystem FileSystem { get; }
    private IAppInfoService AppInfoService { get; }
    private IRuntimeInformation RuntimeInformation { get; }

    public void StartMonitoring()
    {
        this.WatchParsecLogFiles();
        this.CheckConditionMet();
    }

    public void StopMonitoring()
    {
        this.StopWatchingParsecLogFiles();
        this.wasConnected = false;
    }

    private void WatchParsecLogFiles()
    {
        foreach (string directory in this.AppInfoService.ParsecLogDirectories)
        {
            if (this.FileSystem.File.Exists(this.FileSystem.Path.Join(directory, ParsecLogFileName)))
            {
                IFileSystemWatcher watcher = this.FileSystem.FileSystemWatcher.New(directory, ParsecLogFileName);

                watcher.Changed += this.OnParsecLogWatcherEvent;
                watcher.EnableRaisingEvents = true;

                this.parsecLogWatchers.Add(watcher);
            }
        }
    }

    private void StopWatchingParsecLogFiles()
    {
        foreach (IFileSystemWatcher watcher in this.parsecLogWatchers)
        {
            watcher.Changed -= this.OnParsecLogWatcherEvent;
            watcher.Dispose();
        }

        this.parsecLogWatchers.Clear();
    }

    private void OnParsecLogWatcherEvent(object sender, FileSystemEventArgs e)
    {
        try
        {
            this.CheckConditionMet();
        }
        catch (Exception ex)
        {
            this.Logger.Error(ex, "handling a Parsec log file event");
        }
    }

    private void CheckConditionMet([CallerMemberName] string? caller = null)
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

                if (this.Logger.IsEnabled(LogEventLevel.Debug))
                {
                    this.Logger
                        .ForContext(isConnected)
                        .ForContext(this.wasConnected)
                        .ForContext(fired)
                        .ForContext<ParsecConnectedCondition>()
                        .ForContextSourceMember()
                        .Debug("called from {caller}", caller);
                }

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
        using IDisposableList<IProcess> parsecProcs = this.ProcessService.GetProcessesByName("parsecd");

        return this.HasCorrectNumberOfActiveUDPPorts(parsecProcs.Select(p => (uint)p.Id).ToHashSet());
    }

    private bool HasCorrectNumberOfActiveUDPPorts(IReadOnlySet<uint> parsecProcessIds)
    {
        IList<Port> ports = this.NetStatPortsService.GetUdpPorts();

        int count = ports.Count(p => parsecProcessIds.Contains(p.ProcessId));

        bool result = this.RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? count > 2
            : count == 1;

        if (this.Logger.IsEnabled(LogEventLevel.Debug))
        {
            this.Logger
                .ForContext<ParsecConnectedCondition>()
                .ForContextSourceMember()
                .Debug("found {count} ports; returned {result}", count, result);
        }

        return result;
    }
}