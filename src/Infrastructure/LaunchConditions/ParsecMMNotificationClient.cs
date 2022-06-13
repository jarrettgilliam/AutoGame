namespace AutoGame.Infrastructure.LaunchConditions;

using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

internal sealed class ParsecMMNotificationClient : IMMNotificationClient
{
    public event EventHandler<DeviceStateChangedArgs>? DeviceStateChanged;

    public void OnDeviceStateChanged(string deviceId, DeviceState newState) =>
        this.DeviceStateChanged?.Invoke(this, new DeviceStateChangedArgs(deviceId, newState));

    public record DeviceStateChangedArgs(string DeviceId, DeviceState NewState);

    #region Not Implemented

    public void OnDeviceAdded(string pwstrDeviceId)
    {
    }

    public void OnDeviceRemoved(string deviceId)
    {
    }

    public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
    {
    }

    public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
    {
    }

    #endregion
}