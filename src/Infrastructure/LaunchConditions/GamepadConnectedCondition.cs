namespace AutoGame.Infrastructure.LaunchConditions;

using System;
using System.Linq;
using AutoGame.Core.Interfaces;
using Windows.Gaming.Input;

public class GamepadConnectedCondition : ILaunchCondition
{
    private readonly object checkConditionLock = new object();

    public GamepadConnectedCondition(ILoggingService loggingService)
    {
        this.LoggingService = loggingService;
    }

    public event EventHandler? ConditionMet;

    private ILoggingService LoggingService { get; }

    public void StartMonitoring()
    {
        RawGameController.RawGameControllerAdded += this.RawGameController_RawGameControllerAdded;
        this.CheckConditionMet();
    }

    private void RawGameController_RawGameControllerAdded(object? sender, RawGameController e)
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
        RawGameController.RawGameControllerAdded -= this.RawGameController_RawGameControllerAdded;
    }

    private void CheckConditionMet()
    {
        lock (this.checkConditionLock)
        {
            if (RawGameController.RawGameControllers.Any())
            {
                this.ConditionMet?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}