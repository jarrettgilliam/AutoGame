using AutoGame.Infrastructure.Helper;
using AutoGame.Infrastructure.Interfaces;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AutoGame.Infrastructure.Services
{
    public class ParsecConnectedCondition : ILaunchCondition
    {
        private readonly object checkConditionLock = new object();

        private bool wasConnected;
        private bool wasMuted;

        private MMDeviceEnumerator mmDeviceEnumerator;
        private MMDevice mmDevice;

        public ParsecConnectedCondition()
        {
        }

        public event EventHandler ConditionMet;

        public void StartCheckingConditions()
        {
            // Listen for display setting changes
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += this.SystemEvents_DisplaySettingsChanged;

            // Listen for mute/unmute changes
            // From: https://stackoverflow.com/q/27650935/987968
            this.mmDeviceEnumerator = new MMDeviceEnumerator();
            this.mmDevice = this.mmDeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            this.mmDevice.AudioEndpointVolume.OnVolumeNotification += this.AudioEndpointVolume_OnVolumeNotification;
            this.wasMuted = this.mmDevice.AudioEndpointVolume.Mute;

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

        public void Dispose()
        {
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged -= this.SystemEvents_DisplaySettingsChanged;

            if (this.mmDevice != null)
            {
                this.mmDevice.AudioEndpointVolume.OnVolumeNotification -= this.AudioEndpointVolume_OnVolumeNotification;
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
            Process[] parsecProcs = Process.GetProcessesByName("parsecd");

            return this.HasAnyActiveUDPPorts(parsecProcs) &&
                this.HasAnyActiveAudioSessions(parsecProcs);
        }

        private bool HasAnyActiveUDPPorts(Process[] parsecProcs)
        {
            IList<NetStatPorts.Port> ports = NetStatPorts.GetNetStatPorts();

            return ports.Any(p => this.IsParsecUDPPort(p, parsecProcs));
        }

        private bool IsParsecUDPPort(NetStatPorts.Port port, Process[] parsecProcs)
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
            foreach (MMDevice mmDevice in this.mmDeviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                for (int i = 0; i < mmDevice.AudioSessionManager.Sessions.Count; i++)
                {
                    AudioSessionControl session = mmDevice.AudioSessionManager.Sessions[i];

                    if (session.State == AudioSessionState.AudioSessionStateActive &&
                        parsecProcs.Any(proc => proc.Id == session.GetProcessID))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
