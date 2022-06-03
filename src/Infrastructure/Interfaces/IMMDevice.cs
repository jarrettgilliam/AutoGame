namespace AutoGame.Infrastructure.Interfaces;

public interface IMMDevice : IDisposable
{
    IAudioEndpointVolume AudioEndpointVolume { get; }
    
    IAudioSessionManager AudioSessionManager { get; }
}