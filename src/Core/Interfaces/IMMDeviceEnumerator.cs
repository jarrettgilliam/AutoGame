namespace AutoGame.Core.Interfaces;

using NAudio.CoreAudioApi;

public interface IMMDeviceEnumerator : IDisposable
{
    IMMDevice GetDefaultAudioEndpoint(DataFlow dataFlow, Role role);

    IEnumerable<IMMDevice> EnumerateAudioEndPoints(
        DataFlow dataFlow,
        DeviceState dwStateMask);
}