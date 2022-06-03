namespace AutoGame.Infrastructure.Interfaces;

internal interface IMMDevice : IDisposable
{
    IAudioEndpointVolume AudioEndpointVolume { get; }
    
    IAudioSessionManager AudioSessionManager { get; }
}