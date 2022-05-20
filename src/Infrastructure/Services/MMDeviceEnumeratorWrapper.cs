namespace AutoGame.Infrastructure.Services;

using System.Collections.Generic;
using System.Linq;
using AutoGame.Infrastructure.Interfaces;
using NAudio.CoreAudioApi;

public sealed class MMDeviceEnumeratorWrapper : IMMDeviceEnumerator
{
    private readonly MMDeviceEnumerator mmDeviceEnumerator;

    public MMDeviceEnumeratorWrapper(MMDeviceEnumerator mmDeviceEnumerator)
    {
        this.mmDeviceEnumerator = mmDeviceEnumerator;
    }

    public IMMDevice GetDefaultAudioEndpoint(DataFlow dataFlow, Role role) =>
        new MMDeviceWrapper(this.mmDeviceEnumerator.GetDefaultAudioEndpoint(dataFlow, role));

    public IEnumerable<IMMDevice> EnumerateAudioEndPoints(DataFlow dataFlow, DeviceState dwStateMask) =>
        this.mmDeviceEnumerator.EnumerateAudioEndPoints(dataFlow, dwStateMask).Select(d => new MMDeviceWrapper(d));

    public void Dispose() =>
        this.mmDeviceEnumerator.Dispose();

    private class MMDeviceWrapper : IMMDevice
    {
        private readonly MMDevice mmDevice;

        public MMDeviceWrapper(MMDevice mmDevice)
        {
            this.mmDevice = mmDevice;
        }

        public IAudioEndpointVolume AudioEndpointVolume =>
            new AudioEndpointVolumeWrapper(this.mmDevice.AudioEndpointVolume);

        public IAudioSessionManager AudioSessionManager =>
            new AudioSessionManagerWrapper(this.mmDevice.AudioSessionManager);

        public void Dispose() => this.mmDevice.Dispose();

        private class AudioEndpointVolumeWrapper : IAudioEndpointVolume
        {
            private readonly AudioEndpointVolume audioEndpointVolume;

            public AudioEndpointVolumeWrapper(AudioEndpointVolume audioEndpointVolume)
            {
                this.audioEndpointVolume = audioEndpointVolume;
            }

            public event AudioEndpointVolumeNotificationDelegate? OnVolumeNotification
            {
                add => this.audioEndpointVolume.OnVolumeNotification += value;
                remove => this.audioEndpointVolume.OnVolumeNotification -= value;
            }

            public bool Mute => this.audioEndpointVolume.Mute;
        }

        private class AudioSessionManagerWrapper : IAudioSessionManager
        {
            private readonly AudioSessionManager audioSessionManager;
            public AudioSessionManagerWrapper(AudioSessionManager audioSessionManager)
            {
                this.audioSessionManager = audioSessionManager;
            }

            public event AudioSessionManager.SessionCreatedDelegate? OnSessionCreated
            {
                add => this.audioSessionManager.OnSessionCreated += value;
                remove => this.audioSessionManager.OnSessionCreated -= value;
            }

            public IReadOnlyList<AudioSessionControl> Sessions
            {
                get
                {
                    List<AudioSessionControl> sessions = new();

                    for (int i = 0; i < this.audioSessionManager.Sessions.Count; i++)
                    {
                        sessions.Add(this.audioSessionManager.Sessions[i]);
                    }

                    return sessions;
                }
            }
        }
    }
}