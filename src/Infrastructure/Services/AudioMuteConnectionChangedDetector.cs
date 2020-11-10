using NAudio.CoreAudioApi;
using Parscript.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parscript.Infrastructure.Services
{
    public class AudioMuteConnectionChangedDetector : IConnectionChangedDetector, IDisposable
    {
        public AudioMuteConnectionChangedDetector()
        {
            // From: https://stackoverflow.com/q/27650935/987968
            this.MMDeviceEnumerator = new MMDeviceEnumerator();
            this.MMDevice = this.MMDeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            this.MMDevice.AudioEndpointVolume.OnVolumeNotification += this.AudioEndpointVolume_OnVolumeNotification;
            this.IsMuted = this.MMDevice.AudioEndpointVolume.Mute;
        }

        public event EventHandler ConnectionChanged;

        private MMDeviceEnumerator MMDeviceEnumerator { get; set; }

        private MMDevice MMDevice { get; set; }

        private bool IsMuted { get; set; }

        private void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data)
        {
            if (this.IsMuted != data.Muted)
            {
                this.ConnectionChanged?.Invoke(this, EventArgs.Empty);
                this.IsMuted = data.Muted;
            }
        }

        public void Dispose()
        {
            if (this.MMDevice != null)
            {
                this.MMDevice.AudioEndpointVolume.OnVolumeNotification -= this.AudioEndpointVolume_OnVolumeNotification;
                this.MMDevice.Dispose();
                this.MMDevice = null;
            }

            if (this.MMDeviceEnumerator != null)
            {
                this.MMDeviceEnumerator.Dispose();
                this.MMDeviceEnumerator = null;
            }
        }
    }
}
