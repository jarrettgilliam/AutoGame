namespace AutoGame.Infrastructure.LaunchConditions;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;
using Serilog;
using Serilog.Events;

internal sealed class ParsecConnectedCondition : IParsecConnectedCondition
{
    private bool wasConnected;

    public ParsecConnectedCondition(
        ILogger logger,
        INetStatPortsService netStatPortsService,
        IProcessService processService,
        IFileSystem fileSystem,
        IAppInfoService appInfoService,
        ILogWatcherService logWatcherService)
    {
        this.Logger = logger;
        this.NetStatPortsService = netStatPortsService;
        this.ProcessService = processService;
        this.FileSystem = fileSystem;
        this.AppInfoService = appInfoService;
        this.LogWatcherService = logWatcherService;
    }

    public event EventHandler? ConditionMet;

    private ILogger Logger { get; }
    private INetStatPortsService NetStatPortsService { get; }
    private IProcessService ProcessService { get; }
    private IFileSystem FileSystem { get; }
    private IAppInfoService AppInfoService { get; }
    private ILogWatcherService LogWatcherService { get; }

    public void StartMonitoring()
    {
        IFileInfo? parsecLogFileInfo = this.AppInfoService.ParsecLogFiles
            .Select(x => this.FileSystem.FileInfo.New(x))
            .Where(x => x.Exists)
            .MaxBy(x => x.LastWriteTime);

        if (parsecLogFileInfo is not null)
        {
            this.LogWatcherService.LogEntriesAdded += this.OnParsecLogWatcherEvent;
            this.LogWatcherService.StartMonitoring(parsecLogFileInfo.FullName);
        }
    }

    public void StopMonitoring()
    {
        this.LogWatcherService.StopMonitoring();
        this.LogWatcherService.LogEntriesAdded -= this.OnParsecLogWatcherEvent;
        this.wasConnected = false;
    }

    private void OnParsecLogWatcherEvent(ILogWatcherService logWatcherService, IEnumerable<string> logEntries)
    {
        try
        {
            bool isConnected = this.GetIsConnected(logEntries);
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
                    .Debug("LogEntriesAdded event handled");
            }

            this.wasConnected = isConnected;
        }
        catch (Exception ex)
        {
            this.Logger.Error(ex, "handling a Parsec log file event");
        }
    }

    private bool GetIsConnected(IEnumerable<string> logEntries)
    {
        throw new NotImplementedException("TODO: Rewrite GetIsConnected method");

        Port[] parsecPorts = this.GetParsecUdpPorts();

        bool result = this.HasCorrectNumberOfActiveUdpPorts(parsecPorts);

        if (this.Logger.IsEnabled(LogEventLevel.Debug))
        {
            this.Logger
                .ForContext<ParsecConnectedCondition>()
                .ForContextSourceMember()
                .Debug("found {Count} ports; returned {Result}", parsecPorts.Length, result);
        }

        return result;
    }

    private Port[] GetParsecUdpPorts()
    {
        using IDisposableList<IProcess> parsecProcs = this.ProcessService.GetProcessesByName("parsecd");
        HashSet<uint> parsecProcessIds = parsecProcs.Select(p => (uint)p.Id).ToHashSet();

        IList<Port> ports = this.NetStatPortsService.GetUdpPorts();
        return ports.Where(p => parsecProcessIds.Contains(p.ProcessId)).ToArray();
    }

    private bool HasCorrectNumberOfActiveUdpPorts(Port[] parsecPorts) =>
        parsecPorts.Length > 2;
}