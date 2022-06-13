namespace AutoGame.Infrastructure.Interfaces;

using NAudio.CoreAudioApi;

public interface IMMDevice : IDisposable
{
    string ID { get; }
    
    DataFlow DataFlow { get; }
    
    string FriendlyName { get; }
    
    IAudioSessionManager AudioSessionManager { get; }
}