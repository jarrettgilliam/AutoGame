namespace AutoGame.Core.Tests.Services;

using System.IO.Abstractions;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;
using AutoGame.Core.Services;

public class AutoGameServiceTests
{
    private readonly AutoGameService sut;
    private readonly Mock<ILoggingService> loggingServiceMock = new();
    private readonly Mock<IFileSystem> fileSystemMock = new();
    private readonly Mock<IFile> fileMock = new();
    private readonly Mock<ISoftwareManager> softwareMock1 = new();
    private readonly Mock<ISoftwareManager> softwareMock2 = new();
    private readonly Mock<ILaunchCondition> gamepadConnectedConditionMock = new();
    private readonly Mock<ILaunchCondition> parsecConnectedConditionMock = new();

    private readonly Config configMock;

    public AutoGameServiceTests()
    {
        this.fileMock
            .Setup(x => x.Exists(It.IsAny<string>()))
            .Returns(true);
        
        this.fileSystemMock
            .SetupGet(x => x.File)
            .Returns(this.fileMock.Object);
        
        this.softwareMock1
            .SetupGet(x => x.Key)
            .Returns(nameof(this.softwareMock1));

        this.softwareMock1
            .Setup(x => x.FindSoftwarePathOrDefault())
            .Returns($"/path/to/{nameof(this.softwareMock1)}");
        
        this.softwareMock2
            .SetupGet(x => x.Key)
            .Returns(nameof(this.softwareMock2));

        this.softwareMock2
            .Setup(x => x.FindSoftwarePathOrDefault())
            .Returns($"/path/to/{nameof(this.softwareMock2)}");
        
        this.configMock = new Config
        {
            EnableTraceLogging = false,
            SoftwareKey = this.softwareMock1.Object.Key,
            SoftwarePath = this.softwareMock1.Object.FindSoftwarePathOrDefault(),
            LaunchWhenGamepadConnected = true,
            LaunchWhenParsecConnected = true
        };

        this.sut = new AutoGameService(
            this.loggingServiceMock.Object,
            this.fileSystemMock.Object,
            new ISoftwareManager[]
            {
                this.softwareMock1.Object,
                this.softwareMock2.Object
            },
            this.gamepadConnectedConditionMock.Object,
            this.parsecConnectedConditionMock.Object);
    }

    [Fact]
    public void AvailableSoftware_PassedList_Matches()
    {
        Assert.Collection(this.sut.AvailableSoftware,
            s => Assert.Equal(s, this.softwareMock1.Object),
            s => Assert.Equal(s, this.softwareMock2.Object));
    }

    [Fact]
    public void TryApplyConfiguration_LaunchWhenGamepadConnectedTrue_Subscribes()
    {
        this.configMock.LaunchWhenGamepadConnected = true;
        
        this.sut.ApplyConfiguration(this.configMock);
        
        this.gamepadConnectedConditionMock.VerifyAdd(x => x.ConditionMet += It.IsAny<EventHandler>(), Times.Once);
        this.gamepadConnectedConditionMock.Verify(x => x.StartMonitoring(), Times.Once);
    }

    [Fact]
    public void TryApplyConfiguration_LaunchWhenGamepadConnectedFalse_NoSubscribe()
    {
        this.configMock.LaunchWhenGamepadConnected = false;
        
        this.sut.ApplyConfiguration(this.configMock);
        
        this.gamepadConnectedConditionMock.VerifyAdd(x => x.ConditionMet += It.IsAny<EventHandler>(), Times.Never);
        this.gamepadConnectedConditionMock.Verify(x => x.StartMonitoring(), Times.Never);
    }

    [Fact]
    public void TryApplyConfiguration_LaunchWhenParsecConnectedTrue_Subscribes()
    {
        this.configMock.LaunchWhenParsecConnected = true;
        
        this.sut.ApplyConfiguration(this.configMock);
        
        this.parsecConnectedConditionMock.VerifyAdd(x => x.ConditionMet += It.IsAny<EventHandler>(), Times.Once);
        this.parsecConnectedConditionMock.Verify(x => x.StartMonitoring(), Times.Once);
    }

    [Fact]
    public void TryApplyConfiguration_LaunchWhenParsecConnectedFalse_NoSubscribe()
    {
        this.configMock.LaunchWhenParsecConnected = false;
        
        this.sut.ApplyConfiguration(this.configMock);
        
        this.parsecConnectedConditionMock.VerifyAdd(x => x.ConditionMet += It.IsAny<EventHandler>(), Times.Never);
        this.parsecConnectedConditionMock.Verify(x => x.StartMonitoring(), Times.Never);
    }

    [Fact]
    public void TryApplyConfiguration_UnsubscribesConditionMet()
    {
        this.configMock.LaunchWhenGamepadConnected = true;
        this.configMock.LaunchWhenParsecConnected = true;

        this.sut.ApplyConfiguration(this.configMock);
        
        this.configMock.LaunchWhenGamepadConnected = false;
        this.configMock.LaunchWhenParsecConnected = false;

        this.sut.ApplyConfiguration(this.configMock);
        
        this.gamepadConnectedConditionMock.VerifyRemove(x => x.ConditionMet -= It.IsAny<EventHandler>(), Times.Once);
        this.gamepadConnectedConditionMock.Verify(x => x.StopMonitoring(), Times.Once);
        
        this.parsecConnectedConditionMock.VerifyRemove(x => x.ConditionMet -= It.IsAny<EventHandler>(), Times.Once);
        this.parsecConnectedConditionMock.Verify(x => x.StopMonitoring(), Times.Once);
    }

    [Fact]
    public void GetSoftwareByKeyOrNull_ValidKey_ReturnsMatchingSoftware()
    {
        ISoftwareManager? actual = this.sut.GetSoftwareByKeyOrNull(this.softwareMock2.Object.Key);

        Assert.Equal(this.softwareMock2.Object, actual);
    }

    [Fact]
    public void GetSoftwareByKeyOrNull_InvalidKey_ReturnsFirstSoftware()
    {
        ISoftwareManager? actual = this.sut.GetSoftwareByKeyOrNull("badKey");

        Assert.Equal(this.softwareMock1.Object, actual);
    }


    [Fact]
    public void OnLaunchConditionMet_SoftwareNotRunning_Starts()
    {
        this.sut.ApplyConfiguration(this.configMock);
        
        this.gamepadConnectedConditionMock
            .Raise(x => x.ConditionMet += null, EventArgs.Empty);
        
        this.softwareMock1.Verify(
            x => x.Start(this.configMock.SoftwarePath!), Times.Once);
    }

    [Fact]
    public void OnLaunchConditionMet_SoftwareRunning_DoesntStart()
    {
        this.softwareMock1.SetupGet(x => x.IsRunning).Returns(true);
        
        this.sut.ApplyConfiguration(this.configMock);
        
        this.softwareMock1.Verify(
            x => x.Start(It.IsAny<string>()), Times.Never);
    }

    // [Fact]
    // public void OnLaunchConditionMet_BadSoftwareKey_DoesntStart()
    // {
    //     throw new NotEmptyException();
    // }

    [Fact]
    public void Dispose_Works()
    {
        this.sut.ApplyConfiguration(this.configMock);
        this.sut.Dispose();
        
        this.gamepadConnectedConditionMock.VerifyRemove(x => x.ConditionMet -= It.IsAny<EventHandler>(), Times.Once);
        this.gamepadConnectedConditionMock.Verify(x => x.StopMonitoring(), Times.Once);
        
        this.parsecConnectedConditionMock.VerifyRemove(x => x.ConditionMet -= It.IsAny<EventHandler>(), Times.Once);
        this.parsecConnectedConditionMock.Verify(x => x.StopMonitoring(), Times.Once);
    }
}