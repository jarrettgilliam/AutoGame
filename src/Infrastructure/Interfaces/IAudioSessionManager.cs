namespace AutoGame.Infrastructure.Interfaces;

using NAudio.CoreAudioApi;

internal interface IAudioSessionManager
{
    event AudioSessionManager.SessionCreatedDelegate? OnSessionCreated;
    IReadOnlyList<AudioSessionControl> Sessions { get; }
}