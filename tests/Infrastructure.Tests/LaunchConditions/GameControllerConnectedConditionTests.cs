namespace AutoGame.Infrastructure.Tests.LaunchConditions;

using System;
using AutoGame.Core.Interfaces;
using AutoGame.Infrastructure.Interfaces;
using AutoGame.Infrastructure.LaunchConditions;

public class GameControllerConnectedConditionTests
{
    private readonly GameControllerConnectedCondition sut;
    private readonly Mock<ILoggingService> loggingServiceMock = new();
    private readonly Mock<IGameControllerService> gameControllerServiceMock = new();

    private bool hasAnyGameControllers = true;

    public GameControllerConnectedConditionTests()
    {
        this.gameControllerServiceMock
            .SetupGet(x => x.HasAnyGameControllers)
            .Returns(() => this.hasAnyGameControllers);

        this.sut = new GameControllerConnectedCondition(
            this.loggingServiceMock.Object,
            this.gameControllerServiceMock.Object);
    }

    [Fact]
    public void ConnectedGameController_Fires_ConditionMet()
    {
        using var helper = new LaunchConditionTestHelper(this.sut);

        Assert.Equal(1, helper.FiredCount);
    }

    [Fact]
    public void NoConnectedGameController_DoesntFire_ConditionMet()
    {
        this.hasAnyGameControllers = false;

        using var helper = new LaunchConditionTestHelper(this.sut);

        Assert.Equal(0, helper.FiredCount);
    }

    [Fact]
    public void GameControllerAdded_Fires_ConditionMet()
    {
        using var helper = new LaunchConditionTestHelper(this.sut);

        this.gameControllerServiceMock.Raise(x => x.GameControllerAdded += null, EventArgs.Empty);

        Assert.Equal(2, helper.FiredCount);
    }

    [Fact]
    public void StopMonitoring_Works()
    {
        this.sut.StartMonitoring();
        this.sut.StopMonitoring();

        this.gameControllerServiceMock
            .VerifyRemove(
                x => x.GameControllerAdded -= It.IsAny<EventHandler>(),
                Times.Once);
    }
}