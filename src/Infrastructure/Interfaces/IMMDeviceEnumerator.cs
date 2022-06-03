namespace AutoGame.Infrastructure.Interfaces;

using NAudio.CoreAudioApi;

internal interface IMMDeviceEnumerator : IDisposable
{
    IMMDevice GetDefaultAudioEndpoint(DataFlow dataFlow, Role role);

    IEnumerable<IMMDevice> EnumerateAudioEndPoints(
        DataFlow dataFlow,
        DeviceState dwStateMask);
}