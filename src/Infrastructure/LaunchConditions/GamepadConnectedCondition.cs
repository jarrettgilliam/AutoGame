namespace AutoGame.Infrastructure.LaunchConditions;

using System;
using AutoGame.Core.Interfaces;
using AutoGame.Infrastructure.Interfaces;

public class GamepadConnectedCondition : ILaunchCondition
{
    private readonly object checkConditionLock = new object();

    public GamepadConnectedCondition(
        ILoggingService loggingService,
        IRawGameControllerService rawGameControllerService)
    {
        this.LoggingService = loggingService;
        this.RawGameControllerService = rawGameControllerService;
    }

    public event EventHandler? ConditionMet;

    private ILoggingService LoggingService { get; }
    private IRawGameControllerService RawGameControllerService { get; }

    public void StartMonitoring()
    {
        this.RawGameControllerService.RawGameControllerAdded += this.RawGameController_RawGameControllerAdded;
        this.CheckConditionMet();
    }

    private void RawGameController_RawGameControllerAdded(object? sender, EventArgs e)
    {
        try
        {
            this.CheckConditionMet();
        }
        catch (Exception ex)
        {
            this.LoggingService.LogException("handling gamepad added", ex);
        }
    }

    public void StopMonitoring()
    {
        this.RawGameControllerService.RawGameControllerAdded -= this.RawGameController_RawGameControllerAdded;
    }

    private void CheckConditionMet()
    {
        lock (this.checkConditionLock)
        {
            if (this.RawGameControllerService.HasAnyRawGameControllers)
            {
                this.ConditionMet?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}