namespace AutoGame.Infrastructure.Interfaces;

using NAudio.CoreAudioApi;

internal interface IAudioEndpointVolume
{
    event AudioEndpointVolumeNotificationDelegate? OnVolumeNotification;
    bool Mute { get; }
}