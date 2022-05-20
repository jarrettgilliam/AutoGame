namespace AutoGame.Infrastructure.Interfaces;

using NAudio.CoreAudioApi;

public interface IAudioSessionManager
{
    event AudioSessionManager.SessionCreatedDelegate? OnSessionCreated;
    IReadOnlyList<AudioSessionControl> Sessions { get; }
}