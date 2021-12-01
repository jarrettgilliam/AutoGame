﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using AutoGame.Infrastructure.Enums;
using AutoGame.Infrastructure.Helper;
using AutoGame.Infrastructure.Interfaces;
using AutoGame.Infrastructure.Models;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

namespace AutoGame.Infrastructure.LaunchConditions
{
    public class ParsecConnectedCondition : ILaunchCondition
    {
        private const int MaxRetries = 3;
        private static readonly TimeSpan RetryInterval = TimeSpan.FromMilliseconds(100);

        private readonly object checkConditionLock = new object();

        private bool wasConnected;
        private bool wasMuted;

        private MMDeviceEnumerator mmDeviceEnumerator;
        private MMDevice mmDevice;
        private IAudioSessionEventsHandler audioEventClient;

        public ParsecConnectedCondition(ILoggingService loggingService)
        {
            this.LoggingService = loggingService;
            this.audioEventClient = new ParsecAudioSessionEventsHandler(loggingService, this.CheckConditionMet);
        }

        public event EventHandler ConditionMet;

        private ILoggingService LoggingService { get; }

        public void StartMonitoring()
        {
            // Listen for display setting changes
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += this.SystemEvents_DisplaySettingsChanged;

            // Listen for mute/unmute changes
            // From: https://stackoverflow.com/q/27650935/987968
            this.mmDeviceEnumerator = new MMDeviceEnumerator();
            this.mmDevice = this.mmDeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            this.mmDevice.AudioEndpointVolume.OnVolumeNotification += this.AudioEndpointVolume_OnVolumeNotification;
            this.wasMuted = this.mmDevice.AudioEndpointVolume.Mute;

            // Listen for Parsec audio session state changes
            this.mmDevice.AudioSessionManager.OnSessionCreated += this.AudioSessionManager_OnSessionCreated;
            this.RegisterParsecAudioSessionEventClient(this.GetAudioSessions(this.GetParsecdProcesses()).ToArray());

            this.CheckConditionMet();
        }

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
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

        private void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data)
        {
            try
            {
                if (this.wasMuted != data.Muted)
                {
                    this.CheckConditionMet();
                    this.wasMuted = data.Muted;
                }
            }
            catch (Exception ex)
            {
                this.LoggingService.LogException("handling volume notification", ex);
            }
        }

        private void AudioSessionManager_OnSessionCreated(object sender, IAudioSessionControl newSession)
        {
            try
            {
                var session = new AudioSessionControl(newSession);
                Process[] parsecProcs = this.GetParsecdProcesses();

                if (parsecProcs.Any(proc => proc.Id == session.GetProcessID))
                {
                    this.RegisterParsecAudioSessionEventClient(session);
                    this.CheckConditionMet();
                }
            }
            catch (Exception ex)
            {
                this.LoggingService.LogException("handling audio session created", ex);
            }
        }

        private void RegisterParsecAudioSessionEventClient(params AudioSessionControl[] sessions)
        {
            foreach (AudioSessionControl session in sessions)
            {
                session.RegisterEventClient(this.audioEventClient);
            }
        }

        private void UnRegisterParsecAudioSessionEventClient(params AudioSessionControl[] sessions)
        {
            foreach (AudioSessionControl session in sessions)
            {
                session.UnRegisterEventClient(this.audioEventClient);
            }
        }

        private void CheckConditionMet()
        {
            lock (this.checkConditionLock)
            {
                bool isConnected = this.GetIsConnected();

                this.Trace($"{nameof(isConnected)}={isConnected}; {nameof(wasConnected)}={this.wasConnected}");
                if (!this.wasConnected && isConnected)
                {
                    this.Trace($"{nameof(ConditionMet)} fired");
                    this.ConditionMet?.Invoke(this, EventArgs.Empty);
                }

                this.wasConnected = isConnected;
            }
        }

        public void StopMonitoring()
        {
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged -= this.SystemEvents_DisplaySettingsChanged;

            if (this.mmDevice != null)
            {
                this.mmDevice.AudioEndpointVolume.OnVolumeNotification -= this.AudioEndpointVolume_OnVolumeNotification;
                this.mmDevice.AudioSessionManager.OnSessionCreated -= this.AudioSessionManager_OnSessionCreated;
                this.UnRegisterParsecAudioSessionEventClient(this.GetAudioSessions(this.GetParsecdProcesses()).ToArray());
                this.mmDevice.Dispose();
                this.mmDevice = null;
            }

            if (this.mmDeviceEnumerator != null)
            {
                this.mmDeviceEnumerator.Dispose();
                this.mmDeviceEnumerator = null;
            }
        }

        private bool GetIsConnected()
        {
            Process[] parsecProcs = this.GetParsecdProcesses();

            return this.HasAnyActiveUDPPorts(parsecProcs) &&
                this.HasAnyActiveAudioSessions(parsecProcs);
        }

        private Process[] GetParsecdProcesses() => Process.GetProcessesByName("parsecd");

        private bool HasAnyActiveUDPPorts(Process[] parsecProcs)
        {
            IList<Port> ports = NetStatPorts.GetNetStatPorts();

            bool hasPorts = ports.Any(p => this.IsParsecUDPPort(p, parsecProcs));

            this.Trace($"returned {hasPorts}");
            return hasPorts;
        }

        private bool IsParsecUDPPort(Port port, Process[] parsecProcs)
        {
            if (port.Protocol != "UDP")
            {
                return false;
            }

            if (!parsecProcs.Any(proc => proc.Id == port.ProcessId))
            {
                return false;
            }

            return true;
        }

        private bool HasAnyActiveAudioSessions(Process[] parsecProcs)
        {
            bool hasAudioSession = false;

            int i = 1;
            for (; i <= MaxRetries; i++)
            {
                hasAudioSession = this.GetAudioSessions(parsecProcs)
                    .Any(s => s.State == AudioSessionState.AudioSessionStateActive);

                if (hasAudioSession)
                {
                    break;
                }

                Thread.Sleep(RetryInterval);
            }

            this.Trace($"returned {hasAudioSession}; {i} attempt(s)");
            return hasAudioSession;
        }

        private IEnumerable<AudioSessionControl> GetAudioSessions(Process[] parsecProcs)
        {
            foreach (MMDevice mmDevice in this.mmDeviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                for (int i = 0; i < mmDevice.AudioSessionManager.Sessions.Count; i++)
                {
                    AudioSessionControl session = mmDevice.AudioSessionManager.Sessions[i];

                    if (parsecProcs.Any(proc => proc.Id == session.GetProcessID))
                    {
                        yield return session;
                    }
                }
            }
        }

        private void Trace(string message, [CallerMemberName] string member = null) =>
            this.LoggingService.Log($"{nameof(ParsecConnectedCondition)}.{member} {message}", LogLevel.Trace);

        private class ParsecAudioSessionEventsHandler : IAudioSessionEventsHandler
        {

            public ParsecAudioSessionEventsHandler(
                ILoggingService loggingService,
                Action checkConditionMet)
            {
                this.LoggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
                this.CheckConditionMet = checkConditionMet ?? throw new ArgumentNullException(nameof(checkConditionMet));
            }

            private ILoggingService LoggingService { get; }

            private Action CheckConditionMet { get; }

            public void OnChannelVolumeChanged(uint channelCount, IntPtr newVolumes, uint channelIndex)
            {
            }

            public void OnDisplayNameChanged(string displayName)
            {
            }

            public void OnGroupingParamChanged(ref Guid groupingId)
            {
            }

            public void OnIconPathChanged(string iconPath)
            {
            }

            public void OnSessionDisconnected(AudioSessionDisconnectReason disconnectReason)
            {
            }

            public void OnStateChanged(AudioSessionState state)
            {
                try
                {
                    this.CheckConditionMet();
                }
                catch (Exception ex)
                {
                    this.LoggingService.LogException("handling audio session state changed", ex);
                }
            }

            public void OnVolumeChanged(float volume, bool isMuted)
            {
            }
        }
    }
}