namespace AutoGame.Infrastructure.Tests.LaunchConditions;

using System;
using System.Collections.Generic;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;
using AutoGame.Infrastructure.Interfaces;
using AutoGame.Infrastructure.LaunchConditions;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

public class ParsecConnectedConditionTests
{
    private const int PARSECD_PROC_ID = 9999;
    private const int OTHER_PROC_ID = 8888;

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

    private readonly List<Port> netstatPorts;
    private readonly List<AudioSessionControl> audioSessionControls;

    private readonly AudioSessionControlMock audioSessionControlMock;
    private IMMNotificationClient? mmNotificationClient;

    private bool suppressConditionMet;

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
            .Returns(() => this.suppressConditionMet
                ? new DisposableList<IProcess>()
                : new DisposableList<IProcess> { this.processMock.Object });

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
            .SetupGet(x => x.AudioSessionManager)
            .Returns(this.audioSessionManagerMock.Object);

        this.mmDeviceEnumeratorMock
            .Setup(x => x.EnumerateAudioEndPoints(It.IsAny<DataFlow>(), It.IsAny<DeviceState>()))
            .Returns(() => new[] { this.mmDeviceMock.Object });

        this.mmDeviceEnumeratorMock
            .Setup(x => x.RegisterEndpointNotificationCallback(It.IsAny<IMMNotificationClient>()))
            .Callback<IMMNotificationClient>(x => this.mmNotificationClient = x)
            .Returns(0);

        this.mmDeviceEnumeratorMock
            .Setup(x => x.GetDevice(It.IsAny<string>()))
            .Returns(this.mmDeviceMock.Object);

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
        using var helper = new LaunchConditionTestHelper(this.sut);

        Assert.Equal(1, helper.FiredCount);
    }

    [Fact]
    public void ActiveUDPPortAndAudioSession_Disposes_ParsecdProcesses()
    {
        using var helper = new LaunchConditionTestHelper(this.sut);

        this.processMock.Verify(x => x.Dispose(), Times.Exactly(2));
    }

    [Fact]
    public void NetStatNoPorts_DoesntFire_ConditionMet()
    {
        this.netstatPorts.Clear();

        using var helper = new LaunchConditionTestHelper(this.sut);

        Assert.Equal(0, helper.FiredCount);
    }

    [Fact]
    public void NetStatProcessIdMismatch_DoesntFire_ConditionMet()
    {
        this.netstatPorts[0].ProcessId = 8888;

        using var helper = new LaunchConditionTestHelper(this.sut);

        Assert.Equal(0, helper.FiredCount);
    }

    [Fact]
    public void NetStatNoUDPPorts_DoesntFire_ConditionMet()
    {
        this.netstatPorts[0].Protocol = "TCP";

        using var helper = new LaunchConditionTestHelper(this.sut);

        Assert.Equal(0, helper.FiredCount);
    }

    [Fact]
    public void NoAudioSession_DoesntFire_ConditionMet()
    {
        this.audioSessionControls.Clear();

        using var helper = new LaunchConditionTestHelper(this.sut);

        Assert.Equal(0, helper.FiredCount);
    }

    [Fact]
    public void HasAnyActiveAudioSessions_TriesThreeTimes()
    {
        this.audioSessionControls.Clear();

        using var helper = new LaunchConditionTestHelper(this.sut);

        this.sleepServiceMock.Verify(
            x => x.Sleep(It.IsAny<TimeSpan>()),
            Times.Exactly(3));
    }

    [Fact]
    public void HasAnyActiveAudioSessions_DoesntMatchParsecdId_ReturnsFalse()
    {
        this.audioSessionControlMock.ProcessId = OTHER_PROC_ID;
        
        using var helper = new LaunchConditionTestHelper(this.sut);
        
        Assert.Equal(0, helper.FiredCount);
    }

    [Fact]
    public void ConnectionAlreadyDetected_DoesntFire_ConditionMet()
    {
        using var helper = new LaunchConditionTestHelper(this.sut);

        Assert.Equal(1, helper.FiredCount);

        this.systemEventsServiceMock.Raise(x => x.DisplaySettingsChanged += null, EventArgs.Empty);

        Assert.Equal(1, helper.FiredCount);
    }

    [Fact]
    public void InactiveAudioSession_DoesntFire_ConditionMet()
    {
        this.audioSessionControlMock.State = AudioSessionState.AudioSessionStateInactive;

        using var helper = new LaunchConditionTestHelper(this.sut);

        Assert.Equal(0, helper.FiredCount);
    }

    [Fact]
    public void RegisterAudioSessionEventClient_StoresAudioSessionControl()
    {
        using var helper = new LaunchConditionTestHelper(this.sut);
        
        Assert.Single(this.sut.registeredAudioSessions);
    }

    [Theory]
    [InlineData(DeviceState.NotPresent, DataFlow.Render)]
    [InlineData(DeviceState.Unplugged, DataFlow.Render)]
    [InlineData(DeviceState.Active, DataFlow.Capture)]
    public void OnMMDeviceStateChanged_BadStateOrFlow_DoesNothing(DeviceState newState, DataFlow flow)
    {
        this.mmDeviceMock.SetupGet(x => x.DataFlow).Returns(flow);

        using var helper = new LaunchConditionTestHelper(this.sut);

        this.audioSessionManagerMock
            .VerifyAdd(
                x => x.OnSessionCreated += It.IsAny<AudioSessionManager.SessionCreatedDelegate>(),
                Times.Once);
        
        this.mmNotificationClient!.OnDeviceStateChanged("", newState);
        
        this.audioSessionManagerMock
            .VerifyAdd(
                x => x.OnSessionCreated += It.IsAny<AudioSessionManager.SessionCreatedDelegate>(),
                Times.Once);
    }

    [Theory]
    [InlineData(DeviceState.Active, DataFlow.Render)]
    [InlineData(DeviceState.Disabled, DataFlow.Render)]
    public void MMDeviceStateChanged_GoodStateAndFlow_RegistersMMDeviceEvents(DeviceState newState, DataFlow flow)
    {
        this.mmDeviceMock.SetupGet(x => x.DataFlow).Returns(flow);

        using var helper = new LaunchConditionTestHelper(this.sut);

        this.audioSessionManagerMock
            .VerifyAdd(
                x => x.OnSessionCreated += It.IsAny<AudioSessionManager.SessionCreatedDelegate>(),
                Times.Once);
        
        this.mmNotificationClient!.OnDeviceStateChanged("", newState);
        
        this.audioSessionManagerMock
            .VerifyAdd(
                x => x.OnSessionCreated += It.IsAny<AudioSessionManager.SessionCreatedDelegate>(),
                Times.Exactly(2));
    }

    [Fact]
    public void AudioSessionCreated_Fires_ConditionMet()
    {
        this.audioSessionControls.Clear();

        using var helper = new LaunchConditionTestHelper(this.sut);

        Assert.Equal(0, helper.FiredCount);

        this.audioSessionControls.Add(new AudioSessionControl(this.audioSessionControlMock));
        this.audioSessionManagerMock.Raise(
            x => x.OnSessionCreated += null,
            this.audioSessionManagerMock.Object,
            this.audioSessionControlMock);

        Assert.Equal(1, helper.FiredCount);
    }

    [Fact]
    public void AudioSessionOnStateChanged_Fires_ConditionMet()
    {
        this.audioSessionControlMock.State = AudioSessionState.AudioSessionStateInactive;

        using var helper = new LaunchConditionTestHelper(this.sut);

        Assert.Equal(0, helper.FiredCount);

        this.audioSessionControlMock.State = AudioSessionState.AudioSessionStateActive;
        this.audioSessionControlMock.EventClient!.OnStateChanged(AudioSessionState.AudioSessionStateActive);

        Assert.Equal(1, helper.FiredCount);
    }

    [Fact]
    public void AudioSessionCreated_Disposes_ParsecdProcesses()
    {
        this.audioSessionControls.Clear();
        this.suppressConditionMet = true;

        using var helper = new LaunchConditionTestHelper(this.sut);

        this.processMock.Verify(x => x.Dispose(), Times.Never);

        this.suppressConditionMet = false;
        this.audioSessionControls.Add(new AudioSessionControl(this.audioSessionControlMock));
        this.audioSessionManagerMock.Raise(
            x => x.OnSessionCreated += null,
            this.audioSessionManagerMock.Object,
            this.audioSessionControlMock);

        this.processMock.Verify(x => x.Dispose(), Times.Exactly(2));
    }

    [Fact]
    public void CreatedAudioSessionStateChange_Fires_ConditionMet()
    {
        this.audioSessionControls.Clear();

        using var helper = new LaunchConditionTestHelper(this.sut);

        Assert.Equal(0, helper.FiredCount);

        this.audioSessionControlMock.State = AudioSessionState.AudioSessionStateInactive;
        this.audioSessionControlMock.EventClient = null;
        this.audioSessionControls.Add(new AudioSessionControl(this.audioSessionControlMock));
        this.audioSessionManagerMock.Raise(
            x => x.OnSessionCreated += null,
            this.audioSessionManagerMock.Object,
            this.audioSessionControlMock);

        Assert.Equal(0, helper.FiredCount);

        this.audioSessionControlMock.State = AudioSessionState.AudioSessionStateActive;
        this.audioSessionControlMock.EventClient!.OnStateChanged(AudioSessionState.AudioSessionStateActive);

        Assert.Equal(1, helper.FiredCount);
    }

    [Fact]
    public void DisplaySettingsChanged_Fires_ConditionMet()
    {
        this.suppressConditionMet = true;
        using var helper = new LaunchConditionTestHelper(this.sut);
        this.suppressConditionMet = false;

        Assert.Equal(0, helper.FiredCount);

        this.systemEventsServiceMock.Raise(x => x.DisplaySettingsChanged += null, EventArgs.Empty);

        Assert.Equal(1, helper.FiredCount);
    }

    [Fact]
    public void AudioSessionProcessIdMismatch_DoesntFire_ConditionMet()
    {
        this.audioSessionControlMock.ProcessId = 8888;

        using var helper = new LaunchConditionTestHelper(this.sut);

        Assert.Null(this.audioSessionControlMock.EventClient);

        this.audioSessionManagerMock.Raise(
            x => x.OnSessionCreated += null,
            this.audioSessionManagerMock.Object,
            this.audioSessionControlMock);

        Assert.Null(this.audioSessionControlMock.EventClient);
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

        this.mmDeviceEnumeratorMock
            .Verify(
                x => x.UnregisterEndpointNotificationCallback(It.IsAny<IMMNotificationClient>()),
                Times.Once);

        this.audioSessionManagerMock
            .VerifyRemove(
                x => x.OnSessionCreated -= It.IsAny<AudioSessionManager.SessionCreatedDelegate>(),
                Times.Once);

        Assert.Null(this.audioSessionControlMock.EventClient);

        this.mmDeviceMock.Verify(x => x.Dispose(), Times.Once);
    }

    [Fact]
    public void StopMonitoring_ResetsWasConnected()
    {
        using var helper = new LaunchConditionTestHelper(this.sut);

        Assert.Equal(1, helper.FiredCount);

        this.sut.StopMonitoring();
        this.sut.StartMonitoring();

        Assert.Equal(2, helper.FiredCount);
    }

    private class AudioSessionControlMock : IAudioSessionControl2
    {
        public AudioSessionState State { get; set; }

        public uint ProcessId { get; set; }

        public IAudioSessionEvents? EventClient { get; set; }

        public string? SessionIdentifier { get; set; }

        public string? DisplayName { get; set; }

        public int GetState(out AudioSessionState state)
        {
            state = this.State;
            return 0;
        }

        public int GetProcessId(out uint retVal)
        {
            retVal = this.ProcessId;
            return 0;
        }

        public int GetSessionIdentifier(out string retVal)
        {
            retVal = this.SessionIdentifier ?? "";
            return 0;
        }

        public int GetDisplayName(out string displayName)
        {
            displayName = this.DisplayName ?? "";
            return 0;
        }

        public int RegisterAudioSessionNotification(IAudioSessionEvents? client)
        {
            this.EventClient = client;
            return 0;
        }

        public int UnregisterAudioSessionNotification(IAudioSessionEvents? client)
        {
            this.EventClient = null;
            return 0;
        }

        #region NotImplemented

        public int SetDisplayName(string displayName, Guid eventContext) => throw new NotImplementedException();

        public int GetIconPath(out string iconPath) => throw new NotImplementedException();

        public int SetIconPath(string iconPath, Guid eventContext) => throw new NotImplementedException();

        public int GetGroupingParam(out Guid groupingId) => throw new NotImplementedException();

        public int SetGroupingParam(Guid groupingId, Guid eventContext) => throw new NotImplementedException();

        public int GetSessionInstanceIdentifier(out string retVal) => throw new NotImplementedException();

        public int IsSystemSoundsSession() => throw new NotImplementedException();

        public int SetDuckingPreference(bool optOut) => throw new NotImplementedException();

        #endregion NotImplemented
    }
}