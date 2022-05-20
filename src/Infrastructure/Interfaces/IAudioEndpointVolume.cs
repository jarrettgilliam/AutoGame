namespace AutoGame.Infrastructure.Interfaces;

using NAudio.CoreAudioApi;

public interface IAudioEndpointVolume
{
    event AudioEndpointVolumeNotificationDelegate? OnVolumeNotification;
    bool Mute { get; }
}