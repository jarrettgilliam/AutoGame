namespace AutoGame.Infrastructure.Services;

using System.Collections.Generic;
using System.Linq;
using AutoGame.Infrastructure.Interfaces;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

internal sealed class MMDeviceEnumeratorWrapper : IMMDeviceEnumerator
{
    private readonly MMDeviceEnumerator mmDeviceEnumerator = new();

    public IEnumerable<IMMDevice> EnumerateAudioEndPoints(DataFlow dataFlow, DeviceState dwStateMask) =>
        this.mmDeviceEnumerator.EnumerateAudioEndPoints(dataFlow, dwStateMask).Select(d => new MMDeviceWrapper(d));

    public int RegisterEndpointNotificationCallback(IMMNotificationClient client) =>
        this.mmDeviceEnumerator.RegisterEndpointNotificationCallback(client);

    public int UnregisterEndpointNotificationCallback(IMMNotificationClient client) =>
        this.mmDeviceEnumerator.UnregisterEndpointNotificationCallback(client);

    public IMMDevice GetDevice(string id) =>
        new MMDeviceWrapper(this.mmDeviceEnumerator.GetDevice(id));

    public void Dispose() =>
        this.mmDeviceEnumerator.Dispose();

    private class MMDeviceWrapper : IMMDevice
    {
        private readonly MMDevice mmDevice;

        public MMDeviceWrapper(MMDevice mmDevice)
        {   
            this.mmDevice = mmDevice;
        }

        public string ID => this.mmDevice.ID;

        public DataFlow DataFlow => this.mmDevice.DataFlow;

        public string FriendlyName => this.mmDevice.FriendlyName;

        public IAudioSessionManager AudioSessionManager =>
            new AudioSessionManagerWrapper(this.mmDevice.AudioSessionManager);

        public void Dispose() => this.mmDevice.Dispose();

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