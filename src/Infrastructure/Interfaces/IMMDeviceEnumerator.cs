namespace AutoGame.Infrastructure.Interfaces;

using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

public interface IMMDeviceEnumerator : IDisposable
{
    IEnumerable<IMMDevice> EnumerateAudioEndPoints(
        DataFlow dataFlow,
        DeviceState dwStateMask);

    int RegisterEndpointNotificationCallback(IMMNotificationClient client);

    int UnregisterEndpointNotificationCallback(IMMNotificationClient client);

    IMMDevice GetDevice(string id);
}