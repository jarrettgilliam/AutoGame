namespace AutoGame.Core.Tests.Services;

using System;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;
using AutoGame.Core.Services;

public class AutoGameServiceTests
{
    private readonly AutoGameService sut;
    private readonly Mock<ILoggingService> loggingServiceMock = new();
    private readonly Mock<ISoftwareManager> softwareMock1 = new();
    private readonly Mock<ISoftwareManager> softwareMock2 = new();
    private readonly Mock<IGameControllerConnectedCondition> gameControllerConnectedConditionMock = new();
    private readonly Mock<IParsecConnectedCondition> parsecConnectedConditionMock = new();

    private readonly Config configMock;

    public AutoGameServiceTests()
    {
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
            SoftwareArguments = "--custom-arguments",
            LaunchWhenGameControllerConnected = true,
            LaunchWhenParsecConnected = true
        };

        this.sut = new AutoGameService(
            this.loggingServiceMock.Object,
            new SoftwareCollection(
                new ISoftwareManager[]
                {
                    this.softwareMock1.Object,
                    this.softwareMock2.Object
                }),
            this.gameControllerConnectedConditionMock.Object,
            this.parsecConnectedConditionMock.Object);
    }

    [Fact]
    public void ApplyConfiguration_LaunchWhenGameControllerConnectedTrue_Subscribes()
    {
        this.configMock.LaunchWhenGameControllerConnected = true;

        this.sut.ApplyConfiguration(this.configMock);

        this.gameControllerConnectedConditionMock.VerifyAdd(x => x.ConditionMet += It.IsAny<EventHandler>(), Times.Once);
        this.gameControllerConnectedConditionMock.Verify(x => x.StartMonitoring(), Times.Once);
    }

    [Fact]
    public void ApplyConfiguration_LaunchWhenGameControllerConnectedFalse_NoSubscribe()
    {
        this.configMock.LaunchWhenGameControllerConnected = false;

        this.sut.ApplyConfiguration(this.configMock);

        this.gameControllerConnectedConditionMock.VerifyAdd(x => x.ConditionMet += It.IsAny<EventHandler>(), Times.Never);
        this.gameControllerConnectedConditionMock.Verify(x => x.StartMonitoring(), Times.Never);
    }

    [Fact]
    public void ApplyConfiguration_LaunchWhenParsecConnectedTrue_Subscribes()
    {
        this.configMock.LaunchWhenParsecConnected = true;

        this.sut.ApplyConfiguration(this.configMock);

        this.parsecConnectedConditionMock.VerifyAdd(x => x.ConditionMet += It.IsAny<EventHandler>(), Times.Once);
        this.parsecConnectedConditionMock.Verify(x => x.StartMonitoring(), Times.Once);
    }

    [Fact]
    public void ApplyConfiguration_LaunchWhenParsecConnectedFalse_NoSubscribe()
    {
        this.configMock.LaunchWhenParsecConnected = false;

        this.sut.ApplyConfiguration(this.configMock);

        this.parsecConnectedConditionMock.VerifyAdd(x => x.ConditionMet += It.IsAny<EventHandler>(), Times.Never);
        this.parsecConnectedConditionMock.Verify(x => x.StartMonitoring(), Times.Never);
    }

    [Fact]
    public void ApplyConfiguration_UnsubscribesConditionMet()
    {
        this.configMock.LaunchWhenGameControllerConnected = true;
        this.configMock.LaunchWhenParsecConnected = true;

        this.sut.ApplyConfiguration(this.configMock);

        this.configMock.LaunchWhenGameControllerConnected = false;
        this.configMock.LaunchWhenParsecConnected = false;

        this.sut.ApplyConfiguration(this.configMock);

        this.gameControllerConnectedConditionMock.VerifyRemove(x => x.ConditionMet -= It.IsAny<EventHandler>(), Times.Once);
        this.gameControllerConnectedConditionMock.Verify(x => x.StopMonitoring(), Times.Once);

        this.parsecConnectedConditionMock.VerifyRemove(x => x.ConditionMet -= It.IsAny<EventHandler>(), Times.Once);
        this.parsecConnectedConditionMock.Verify(x => x.StopMonitoring(), Times.Once);
    }

    [Fact]
    public void OnLaunchConditionMet_SoftwareNotRunning_Starts()
    {
        this.sut.ApplyConfiguration(this.configMock);

        this.gameControllerConnectedConditionMock
            .Raise(x => x.ConditionMet += null, EventArgs.Empty);

        this.softwareMock1.Verify(
            x => x.Start(
                this.configMock.SoftwarePath!,
                this.configMock.SoftwareArguments!),
            Times.Once);
    }

    [Fact]
    public void OnLaunchConditionMet_SoftwareRunning_DoesntStart()
    {
        this.softwareMock1.Setup(x => x.IsRunning(It.IsAny<string>())).Returns(true);

        this.sut.ApplyConfiguration(this.configMock);

        this.softwareMock1.Verify(
            x => x.Start(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Dispose_Works()
    {
        this.sut.ApplyConfiguration(this.configMock);
        this.sut.Dispose();

        this.gameControllerConnectedConditionMock.VerifyRemove(x => x.ConditionMet -= It.IsAny<EventHandler>(), Times.Once);
        this.gameControllerConnectedConditionMock.Verify(x => x.StopMonitoring(), Times.Once);

        this.parsecConnectedConditionMock.VerifyRemove(x => x.ConditionMet -= It.IsAny<EventHandler>(), Times.Once);
        this.parsecConnectedConditionMock.Verify(x => x.StopMonitoring(), Times.Once);
    }
}