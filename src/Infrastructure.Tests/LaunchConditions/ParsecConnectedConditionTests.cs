namespace AutoGame.Infrastructure.Tests.LaunchConditions;

using System;
using System.Collections.Generic;
using System.Linq;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;
using AutoGame.Infrastructure.LaunchConditions;
using Moq;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using Xunit;
using IDisposable = ABI.System.IDisposable;

public class ParsecConnectedConditionTests
{
    private const int PARSECD_PROC_ID = 9999;

    private readonly ParsecConnectedCondition sut;
    private readonly Mock<ILoggingService> loggingServiceMock = new();
    private readonly Mock<INetStatPortsService> netStatPortsServiceMock = new();
    private readonly Mock<ISleepService> sleepServiceMock = new();
    private readonly Mock<IProcessService> processServiceMock = new();
    private readonly Mock<IProcess> processMock = new();
    private readonly Mock<ISystemEventsService> systemEventsServiceMock = new();

    private readonly Mock<IMMDeviceEnumerator> mmDeviceEnumeratorMock = new();
    private readonly Mock<IMMDevice> mmDeviceMock = new();
    private readonly Mock<IAudioSessionManager> audioSessionManagerMock = new();
    private readonly Mock<IAudioEndpointVolume> audioEndpointVolumeMock = new();

    private readonly List<Port> netstatPorts;
    private readonly List<AudioSessionControl> audioSessionControls;
    private readonly AudioSessionControlMock audioSessionControlMock;

    public ParsecConnectedConditionTests()
    {
        this.netstatPorts = new List<Port>
        {
            new() { Protocol = "UDP", ProcessId = PARSECD_PROC_ID }
        };

        this.netStatPortsServiceMock
            .Setup(x => x.GetNetStatPorts())
            .Returns(this.netstatPorts);

        this.processMock
            .SetupGet(x => x.Id)
            .Returns(PARSECD_PROC_ID);

        this.processServiceMock
            .Setup(x => x.GetProcessesByName(It.IsAny<string?>()))
            .Returns(new[] { this.processMock.Object });

        this.audioSessionControlMock = new AudioSessionControlMock
        {
            State = AudioSessionState.AudioSessionStateActive,
            ProcessId = PARSECD_PROC_ID
        };
        
        this.audioSessionControls = new List<AudioSessionControl>
        {
            new(this.audioSessionControlMock)
        };

        this.audioSessionManagerMock
            .SetupGet(x => x.Sessions)
            .Returns(this.audioSessionControls);

        this.mmDeviceMock
            .SetupGet(x => x.AudioEndpointVolume)
            .Returns(this.audioEndpointVolumeMock.Object);

        this.mmDeviceMock
            .SetupGet(x => x.AudioSessionManager)
            .Returns(this.audioSessionManagerMock.Object);

        this.mmDeviceEnumeratorMock
            .Setup(x => x.GetDefaultAudioEndpoint(It.IsAny<DataFlow>(), It.IsAny<Role>()))
            .Returns(this.mmDeviceMock.Object);

        this.mmDeviceEnumeratorMock
            .Setup(x => x.EnumerateAudioEndPoints(It.IsAny<DataFlow>(), It.IsAny<DeviceState>()))
            .Returns(new[] { this.mmDeviceMock.Object });

        this.sut = new ParsecConnectedCondition(
            this.loggingServiceMock.Object,
            this.netStatPortsServiceMock.Object,
            this.sleepServiceMock.Object,
            this.processServiceMock.Object,
            this.systemEventsServiceMock.Object,
            this.mmDeviceEnumeratorMock.Object);
    }

    [Fact]
    public void ActiveUDPPortAndAudioSession_Fires_ConditionMet()
    {
        using var helper = new ConditionMetHelper(this);
        
        helper.AssertFired();
    }

    [Fact]
    public void NoActiveUDPPort_DoesntFire_ConditionMet()
    {
        this.netstatPorts.Clear();

        using var helper = new ConditionMetHelper(this);

        helper.AssertNotFired();
    }

    [Fact]
    public void NoAudioSession_DoesntFire_ConditionMet()
    {
        this.audioSessionControls.Clear();

        using var helper = new ConditionMetHelper(this);

        helper.AssertNotFired();
    }

    [Fact]
    public void InactiveAudioSession_DoesntFire_ConditionMet()
    {
        this.audioSessionControlMock.State = AudioSessionState.AudioSessionStateInactive;

        using var helper = new ConditionMetHelper(this);
        
        helper.AssertNotFired();
    }

    [Fact]
    public void AudioSessionOnStateChanged_Fires_ConditionMet()
    {
        this.audioSessionControlMock.State = AudioSessionState.AudioSessionStateInactive;
        
        using var helper = new ConditionMetHelper(this);
        
        helper.AssertNotFired();
        
        this.audioSessionControlMock.State = AudioSessionState.AudioSessionStateActive;
        this.audioSessionControlMock.EventClient.OnStateChanged(AudioSessionState.AudioSessionStateActive);
        
        helper.AssertFired();
    }

    [Fact]
    public void AudioSessionCreated_Fires_ConditionMet()
    {
        this.audioSessionControls.Clear();
        
        using var helper = new ConditionMetHelper(this);
        
        helper.AssertNotFired();
        
        this.audioSessionControls.Add(new AudioSessionControl(this.audioSessionControlMock));
        this.audioSessionManagerMock.Raise(
            x => x.OnSessionCreated += null,
            this.audioSessionManagerMock.Object,
            this.audioSessionControlMock);
        
        helper.AssertFired();
    }

    [Fact]
    public void CreatedAudioSessionStateChange_Fires_ConditionMet()
    {
        this.audioSessionControls.Clear();

        using var helper = new ConditionMetHelper(this);
        
        helper.AssertNotFired();

        this.audioSessionControlMock.State = AudioSessionState.AudioSessionStateInactive;
        this.audioSessionControlMock.EventClient = null;
        this.audioSessionControls.Add(new AudioSessionControl(this.audioSessionControlMock));
        this.audioSessionManagerMock.Raise(
            x => x.OnSessionCreated += null,
            this.audioSessionManagerMock.Object,
            this.audioSessionControlMock);
        
        helper.AssertNotFired();
        
        this.audioSessionControlMock.State = AudioSessionState.AudioSessionStateActive;
        this.audioSessionControlMock.EventClient!.OnStateChanged(AudioSessionState.AudioSessionStateActive);
        
        helper.AssertFired();
    }

    [Fact]
    public void DisplaySettingsChanged_Fires_ConditionMet()
    {
        // We have to keep the event from firing when StartMonitoring() is called;
        Port p = this.netstatPorts[0];
        this.netstatPorts.Clear();

        using var helper = new ConditionMetHelper(this);
        
        helper.AssertNotFired();

        this.netstatPorts.Add(p);
        this.systemEventsServiceMock.Raise(x => x.DisplaySettingsChanged += null, EventArgs.Empty);

        helper.AssertFired();
    }

    [Fact]
    public void MuteChanged_Fires_ConditionMet()
    {
        // Have to keep the event from firing when StartMonitoring() is called;
        Port p = this.netstatPorts[0];
        this.netstatPorts.Clear();

        using var helper = new ConditionMetHelper(this);
        
        helper.AssertNotFired();

        this.netstatPorts.Add(p);

        this.audioEndpointVolumeMock.Raise(
            x => x.OnVolumeNotification += null,
            new AudioVolumeNotificationData(
                eventContext: Guid.Empty,
                muted: true,
                masterVolume: 0f,
                channelVolume: Array.Empty<float>(),
                guid: Guid.Empty));

        helper.AssertFired();
    }

    [Fact]
    public void OtherVolumePropertyChanged_DoesntFire_ConditionMet()
    {
        // Have to keep the event from firing when StartMonitoring() is called;
        Port p = this.netstatPorts[0];
        this.netstatPorts.Clear();
        
        using var helper = new ConditionMetHelper(this);
        
        helper.AssertNotFired();

        this.netstatPorts.Add(p);
        
        this.audioEndpointVolumeMock.Raise(
            x => x.OnVolumeNotification += null,
            new AudioVolumeNotificationData(
                eventContext: Guid.Empty,
                muted: false,
                masterVolume: 1f,
                channelVolume: Array.Empty<float>(),
                guid: Guid.Empty));
        
        helper.AssertNotFired();
    }

    [Fact]
    public void StopMonitoring_Works()
    {
        this.sut.StartMonitoring();
        this.sut.StopMonitoring();
        
        this.systemEventsServiceMock
            .VerifyRemove(
                x => x.DisplaySettingsChanged -= It.IsAny<EventHandler>(),
                Times.Once);
        
        this.audioEndpointVolumeMock
            .VerifyRemove(
                x => x.OnVolumeNotification -= It.IsAny<AudioEndpointVolumeNotificationDelegate>(),
                Times.Once);
        
        this.audioSessionManagerMock
            .VerifyRemove(
                x => x.OnSessionCreated -= It.IsAny<AudioSessionManager.SessionCreatedDelegate>(),
                Times.Once);

        Assert.Null(this.audioSessionControlMock.EventClient);
        
        this.mmDeviceMock.Verify(x => x.Dispose(), Times.Once);
    }

    private class ConditionMetHelper : IDisposable
    {
        private readonly ParsecConnectedConditionTests parent;
        private bool fired;
        
        public ConditionMetHelper(ParsecConnectedConditionTests parent)
        {
            this.parent = parent;
            this.parent.sut.ConditionMet += this.OnConditionMet;
            this.parent.sut.StartMonitoring();
        }

        public void Dispose()
        {
            this.parent.sut.StopMonitoring();
            this.parent.sut.ConditionMet -= this.OnConditionMet;
        }

        private void OnConditionMet(object? sender, EventArgs e) => this.fired = true;

        public void AssertNotFired() =>
            Assert.False(this.fired, "The ConditionMet event was fired");

        public void AssertFired() =>
            Assert.True(this.fired, "The ConditionMet event was not fired");
    }

    private class AudioSessionControlMock : IAudioSessionControl2
    {
        public AudioSessionState State { get; set; }

        public uint ProcessId { get; set; }
        
        public IAudioSessionEvents? EventClient { get; set; }

        int IAudioSessionControl.GetState(out AudioSessionState state)
        {
            state = this.State;
            return 0;
        }

        public int GetProcessId(out uint retVal)
        {
            retVal = this.ProcessId;
            return 0;
        }

        int IAudioSessionControl.RegisterAudioSessionNotification(IAudioSessionEvents? client)
        {
            this.EventClient = client;
            return 0;
        }

        int IAudioSessionControl.UnregisterAudioSessionNotification(IAudioSessionEvents? client)
        {
            this.EventClient = null;
            return 0;
        }

        #region NotImplemented

        int IAudioSessionControl2.GetDisplayName(out string displayName) => throw new NotImplementedException();

        int IAudioSessionControl2.SetDisplayName(string displayName, Guid eventContext) => throw new NotImplementedException();

        int IAudioSessionControl2.GetIconPath(out string iconPath) => throw new NotImplementedException();

        int IAudioSessionControl2.SetIconPath(string iconPath, Guid eventContext) => throw new NotImplementedException();

        int IAudioSessionControl2.GetGroupingParam(out Guid groupingId) => throw new NotImplementedException();

        int IAudioSessionControl2.SetGroupingParam(Guid groupingId, Guid eventContext) => throw new NotImplementedException();

        int IAudioSessionControl2.RegisterAudioSessionNotification(IAudioSessionEvents client) => throw new NotImplementedException();

        int IAudioSessionControl2.UnregisterAudioSessionNotification(IAudioSessionEvents client) => throw new NotImplementedException();

        public int GetSessionIdentifier(out string retVal) => throw new NotImplementedException();

        public int GetSessionInstanceIdentifier(out string retVal) => throw new NotImplementedException();

        public int IsSystemSoundsSession() => throw new NotImplementedException();

        public int SetDuckingPreference(bool optOut) => throw new NotImplementedException();

        int IAudioSessionControl2.GetState(out AudioSessionState state) => throw new NotImplementedException();

        int IAudioSessionControl.GetDisplayName(out string displayName) => throw new NotImplementedException();

        int IAudioSessionControl.SetDisplayName(string displayName, Guid eventContext) => throw new NotImplementedException();

        int IAudioSessionControl.GetIconPath(out string iconPath) => throw new NotImplementedException();

        int IAudioSessionControl.SetIconPath(string iconPath, Guid eventContext) => throw new NotImplementedException();

        int IAudioSessionControl.GetGroupingParam(out Guid groupingId) => throw new NotImplementedException();

        int IAudioSessionControl.SetGroupingParam(Guid groupingId, Guid eventContext) => throw new NotImplementedException();

        #endregion NotImplemented
    }
}