namespace AutoGame.Infrastructure.Tests.LaunchConditions;

using System;
using AutoGame.Core.Interfaces;
using AutoGame.Infrastructure.Interfaces;
using AutoGame.Infrastructure.LaunchConditions;

public class GamepadConnectedConditionTests
{
    private readonly GamepadConnectedCondition sut;
    private readonly Mock<ILoggingService> loggingServiceMock = new();
    private readonly Mock<IRawGameControllerService> rawGameControllerServiceMock = new();

    private bool hasAnyRawGameControllers = true;

    public GamepadConnectedConditionTests()
    {
        this.rawGameControllerServiceMock
            .SetupGet(x => x.HasAnyRawGameControllers)
            .Returns(() => this.hasAnyRawGameControllers);
        
        this.sut = new GamepadConnectedCondition(
            this.loggingServiceMock.Object,
            this.rawGameControllerServiceMock.Object);
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
        this.hasAnyRawGameControllers = false;
        
        using var helper = new LaunchConditionTestHelper(this.sut);
        
        Assert.Equal(0, helper.FiredCount);
    }
    
    [Fact]
    public void GameControllerAdded_Fires_ConditionMet()
    {
        using var helper = new LaunchConditionTestHelper(this.sut);

        this.rawGameControllerServiceMock.Raise(x => x.RawGameControllerAdded += null, EventArgs.Empty);
        
        Assert.Equal(2, helper.FiredCount);
    }
    
    [Fact]
    public void StopMonitoring_Works()
    {
        this.sut.StartMonitoring();
        this.sut.StopMonitoring();
        
        this.rawGameControllerServiceMock
            .VerifyRemove(
                x => x.RawGameControllerAdded -= It.IsAny<EventHandler>(),
                Times.Once);
    }
}