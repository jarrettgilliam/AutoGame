using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AutoGame.Infrastructure.Helper;
using AutoGame.Infrastructure.Interfaces;
using AutoGame.Infrastructure.Models;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

namespace AutoGame.Infrastructure.LaunchConditions
{
    public class ParsecConnectedCondition : ILaunchCondition
    {
        private readonly object checkConditionLock = new object();
        private readonly object audioSessionLock = new object();

        private bool wasConnected;
        private bool wasMuted;

        private MMDeviceEnumerator mmDeviceEnumerator;
        private MMDevice mmDevice;
        private AudioSessionControl audioSession;
        private IAudioSessionEventsHandler audioEventClient;

        public ParsecConnectedCondition()
        {
            this.audioEventClient = new ParsecAudioSessionEventsHandler(this.CheckConditionMet);
        }

        public event EventHandler ConditionMet;

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
            this.SetParsecAudioSession(this.GetAudioSessionOrDefault(this.GetParsecdProcesses()));

            this.CheckConditionMet();
        }

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            this.CheckConditionMet();
        }

        private void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data)
        {
            if (this.wasMuted != data.Muted)
            {
                this.CheckConditionMet();
                this.wasMuted = data.Muted;
            }
        }

        private void AudioSessionManager_OnSessionCreated(object sender, IAudioSessionControl newSession)
        {
            var session = new AudioSessionControl(newSession);
            Process[] parsecProcs = this.GetParsecdProcesses();

            if (parsecProcs.Any(proc => proc.Id == session.GetProcessID))
            {
                this.SetParsecAudioSession(session);
                this.CheckConditionMet();
            }
        }

        private void SetParsecAudioSession(AudioSessionControl session)
        {
            lock (this.audioSessionLock)
            {
                this.audioSession?.UnRegisterEventClient(this.audioEventClient);
                this.audioSession = session;
                this.audioSession?.RegisterEventClient(this.audioEventClient);
            }
        }

        private void CheckConditionMet()
        {
            lock (this.checkConditionLock)
            {
                bool isConnected = this.GetIsConnected();

                if (!this.wasConnected && isConnected)
                {
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
                this.SetParsecAudioSession(null);
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
            return this.HasAnyActiveUDPPorts(this.GetParsecdProcesses()) &&
                this.audioSession?.State == AudioSessionState.AudioSessionStateActive;
        }

        private Process[] GetParsecdProcesses() => Process.GetProcessesByName("parsecd");

        private bool HasAnyActiveUDPPorts(Process[] parsecProcs)
        {
            IList<Port> ports = NetStatPorts.GetNetStatPorts();

            return ports.Any(p => this.IsParsecUDPPort(p, parsecProcs));
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

        private AudioSessionControl GetAudioSessionOrDefault(Process[] parsecProcs)
        {
            foreach (MMDevice mmDevice in this.mmDeviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                for (int i = 0; i < mmDevice.AudioSessionManager.Sessions.Count; i++)
                {
                    AudioSessionControl session = mmDevice.AudioSessionManager.Sessions[i];

                    if (parsecProcs.Any(proc => proc.Id == session.GetProcessID))
                    {
                        return session;
                    }
                }
            }

            return null;
        }

        private class ParsecAudioSessionEventsHandler : IAudioSessionEventsHandler
        {
            private readonly Action checkConditionMet;

            public ParsecAudioSessionEventsHandler(Action checkConditionMet)
            {
                this.checkConditionMet = checkConditionMet ?? throw new ArgumentNullException(nameof(checkConditionMet));
            }

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
                this.checkConditionMet();
            }

            public void OnVolumeChanged(float volume, bool isMuted)
            {
            }
        }
    }
}
