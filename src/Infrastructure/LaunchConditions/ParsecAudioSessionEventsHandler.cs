namespace AutoGame.Infrastructure.LaunchConditions;

using NAudio.CoreAudioApi.Interfaces;

internal sealed class ParsecAudioSessionEventsHandler : IAudioSessionEventsHandler
{
    public event EventHandler<AudioSessionState>? StateChanged;

    public void OnStateChanged(AudioSessionState state) =>
        this.StateChanged?.Invoke(this, state);

    #region Not Implemented

    public void OnChannelVolumeChanged(uint channelCount, IntPtr newVolumes, uint channelIndex)
    {
    }

    public void OnDisplayNameChanged(string displayName)
    {
    }

    public void OnGroupingParamChanged(ref Guid groupingId)
    {
    }

    public void OnIconPathChanged(string iconPath)
    {
    }

    public void OnSessionDisconnected(AudioSessionDisconnectReason disconnectReason)
    {
    }

    public void OnVolumeChanged(float volume, bool isMuted)
    {
    }

    #endregion
}