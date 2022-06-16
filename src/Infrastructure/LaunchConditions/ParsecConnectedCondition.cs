namespace AutoGame.Infrastructure.LaunchConditions;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.CompilerServices;
using AutoGame.Core.Enums;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;
using AutoGame.Infrastructure.Interfaces;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

internal sealed class ParsecConnectedCondition : IParsecConnectedCondition
{
    private readonly object checkConditionLock = new();
    private bool wasConnected;

    private readonly ParsecAudioSessionEventsHandler audioEventClient = new();
    private readonly ParsecMMNotificationClient mmNotificationClient = new();
    internal ConcurrentDictionary<string, AudioSessionControl> registeredAudioSessions = new();

    private readonly string ParsecLogFileName;
    private readonly string ParsecLogDirectory;
    private IFileSystemWatcher? parsecLogWatcher;

    public ParsecConnectedCondition(
        ILoggingService loggingService,
        INetStatPortsService netStatPortsService,
        ISleepService sleepService,
        IProcessService processService,
        ISystemEventsService systemEventsService,
        IMMDeviceEnumerator mmDeviceEnumerator,
        IFileSystem fileSystem)
    {
        this.LoggingService = loggingService;
        this.NetStatPortsService = netStatPortsService;
        this.SleepService = sleepService;
        this.ProcessService = processService;
        this.SystemEventsService = systemEventsService;
        this.MMDeviceEnumerator = mmDeviceEnumerator;
        this.FileSystem = fileSystem;

        this.ParsecLogFileName = "log.txt";
        this.ParsecLogDirectory = this.FileSystem.Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Parsec");
    }

    public event EventHandler? ConditionMet;

    private ILoggingService LoggingService { get; }
    private INetStatPortsService NetStatPortsService { get; }
    private ISleepService SleepService { get; }
    private IProcessService ProcessService { get; }
    private ISystemEventsService SystemEventsService { get; }
    private IMMDeviceEnumerator MMDeviceEnumerator { get; }
    private IFileSystem FileSystem { get; }

    public void StartMonitoring()
    {
        this.SystemEventsService.DisplaySettingsChanged += this.SystemEvents_DisplaySettingsChanged;
        this.RegisterAudioEvents();
        this.WatchParsecLogFile();
        this.CheckConditionMet();
    }

    public void StopMonitoring()
    {
        this.SystemEventsService.DisplaySettingsChanged -= this.SystemEvents_DisplaySettingsChanged;
        this.UnRegisterAudioEvents();
        this.StopWatchingParsecLogFile();
        this.wasConnected = false;
    }

    private void SystemEvents_DisplaySettingsChanged(object? sender, EventArgs e)
    {
        try
        {
            this.CheckConditionMet();
        }
        catch (Exception ex)
        {
            this.LoggingService.LogException("handling display settings changed", ex);
        }
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

    private void RegisterAudioEvents()
    {
        this.audioEventClient.StateChanged += this.OnAudioSessionStateChanged;
        this.mmNotificationClient.DeviceStateChanged += this.OnMMDeviceStateChanged;

        using IDisposableList<IProcess> parsecProcs = this.GetParsecdProcesses();

        this.MMDeviceEnumerator.RegisterEndpointNotificationCallback(this.mmNotificationClient);

        foreach (IMMDevice mmd in this.MMDeviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active | DeviceState.Disabled))
        {
            this.RegisterMMDeviceEvents(mmd, parsecProcs);
        }
    }

    private void RegisterMMDeviceEvents(IMMDevice mmd, IList<IProcess> parsecProcs)
    {
        mmd.AudioSessionManager.OnSessionCreated += this.OnAudioSessionSessionCreated;
        this.Trace(() => $"Listening for new sessions on {mmd.FriendlyName} ({mmd.ID})");

        foreach (AudioSessionControl session in mmd.AudioSessionManager.Sessions)
        {
            if (parsecProcs.Any(proc => proc.Id == session.GetProcessID))
            {
                this.RegisterAudioSessionEventClient(session);
            }
        }
    }

    private void RegisterAudioSessionEventClient(AudioSessionControl session)
    {
        session.RegisterEventClient(this.audioEventClient);

        // Hold onto these so they don't get garbage collected
        this.registeredAudioSessions[session.GetSessionIdentifier] = session;
        this.Trace(() => $"Listening for session state changes on {session.DisplayName} ({session.GetSessionIdentifier})");
    }

    private void UnRegisterAudioEvents()
    {
        this.audioEventClient.StateChanged -= this.OnAudioSessionStateChanged;
        this.mmNotificationClient.DeviceStateChanged -= this.OnMMDeviceStateChanged;

        using IDisposableList<IProcess> parsecProcs = this.GetParsecdProcesses();

        this.MMDeviceEnumerator.UnregisterEndpointNotificationCallback(this.mmNotificationClient);

        foreach (IMMDevice mmd in this.MMDeviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active | DeviceState.Disabled))
        {
            this.UnRegisterMMDeviceEvents(mmd, parsecProcs);
            mmd.Dispose();
        }

        // Clear out any remaining sessions
        ConcurrentDictionary<string, AudioSessionControl> oldAudioSession = Interlocked.Exchange(
            ref this.registeredAudioSessions,
            new ConcurrentDictionary<string, AudioSessionControl>());

        foreach (AudioSessionControl session in oldAudioSession.Values)
        {
            session.UnRegisterEventClient(this.audioEventClient);
        }
    }

    private void UnRegisterMMDeviceEvents(IMMDevice mmd, IList<IProcess> parsecProcs)
    {
        mmd.AudioSessionManager.OnSessionCreated -= this.OnAudioSessionSessionCreated;

        foreach (AudioSessionControl session in mmd.AudioSessionManager.Sessions)
        {
            if (parsecProcs.Any(proc => proc.Id == session.GetProcessID))
            {
                this.UnRegisterAudioSessionEventClient(session.GetSessionIdentifier);
            }
        }
    }

    private void UnRegisterAudioSessionEventClient(string sessionIdentifier)
    {
        if (this.registeredAudioSessions.TryRemove(sessionIdentifier, out AudioSessionControl? session))
        {
            session.UnRegisterEventClient(this.audioEventClient);
        }
    }

    private void OnMMDeviceStateChanged(object? sender, ParsecMMNotificationClient.DeviceStateChangedArgs e)
    {
        try
        {
            if (e.NewState is not DeviceState.Active and not DeviceState.Disabled)
            {
                return;
            }

            IMMDevice mmd = this.MMDeviceEnumerator.GetDevice(e.DeviceId);

            if (mmd.DataFlow != DataFlow.Render)
            {
                return;
            }

            using IDisposableList<IProcess> parsecProcs = this.GetParsecdProcesses();

            this.RegisterMMDeviceEvents(mmd, parsecProcs);
        }
        catch (Exception ex)
        {
            this.LoggingService.LogException("handling multimedia device state changed", ex);
        }
    }

    private void OnAudioSessionSessionCreated(object sender, IAudioSessionControl newSession)
    {
        AudioSessionControl? session = null;

        try
        {
            session = new AudioSessionControl(newSession);
            using IDisposableList<IProcess> parsecProcs = this.GetParsecdProcesses();

            if (parsecProcs.Any(proc => proc.Id == session.GetProcessID))
            {
                this.RegisterAudioSessionEventClient(session);
                this.CheckConditionMet();
            }
        }
        catch (Exception ex)
        {
            this.LoggingService.LogException("handling audio session created", ex);
        }
        finally
        {
            this.Trace(() => $"{session?.DisplayName ?? "Unknown Name"} " +
                             $"({(session != null ? session.GetProcessID : "Unknown PID")})");
        }
    }

    private void OnAudioSessionStateChanged(object? sender, AudioSessionState state)
    {
        try
        {
            this.CheckConditionMet();
        }
        catch (Exception ex)
        {
            this.LoggingService.LogException("handling audio session state change", ex);
        }
        finally
        {
            this.Trace(state.ToString);
        }
    }

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

        return this.HasAnyActiveUDPPorts(parsecProcs) &&
               this.HasAnyActiveAudioSessions(parsecProcs);
    }

    private bool HasAnyActiveUDPPorts(IList<IProcess> parsecProcs)
    {
        IList<Port> ports = this.NetStatPortsService.GetNetStatPorts();

        bool hasPorts = ports.Any(p => this.IsParsecUDPPort(p, parsecProcs));

        this.Trace(() => $"returned {hasPorts}");
        return hasPorts;
    }

    private bool IsParsecUDPPort(Port port, IList<IProcess> parsecProcs) =>
        port.Protocol == "UDP" && parsecProcs.Any(proc => proc.Id == port.ProcessId);

    private bool HasAnyActiveAudioSessions(IList<IProcess> parsecProcs)
    {
        const int MaxRetries = 3;
        TimeSpan retryInterval = TimeSpan.FromMilliseconds(100);
        bool hasAudioSession = false;
        int i = 1;

        try
        {
            for (; i <= MaxRetries; i++)
            {
                if (this.MMDeviceEnumerator
                    .EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)
                    .SelectMany(mmd => mmd.AudioSessionManager.Sessions)
                    .Any(session => session.State == AudioSessionState.AudioSessionStateActive &&
                                    parsecProcs.Any(proc => proc.Id == session.GetProcessID)))
                {
                    hasAudioSession = true;
                    break;
                }

                this.SleepService.Sleep(retryInterval);
            }

            return hasAudioSession;
        }
        finally
        {
            this.Trace(() => $"returned {hasAudioSession}; {i} attempt(s)");
        }
    }

    private void Trace(Func<string> message, [CallerMemberName] string? member = null)
    {
        if (this.LoggingService.EnableTraceLogging)
        {
            this.LoggingService.Log($"{nameof(ParsecConnectedCondition)}.{member} {message()}", LogLevel.Trace);
        }
    }
}