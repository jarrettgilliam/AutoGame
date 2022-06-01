namespace AutoGame.Infrastructure.LaunchConditions;

using System;
using AutoGame.Core.Interfaces;
using AutoGame.Infrastructure.Interfaces;

internal sealed class GameControllerConnectedCondition : IGameControllerConnectedCondition
{
    private readonly object checkConditionLock = new();

    public GameControllerConnectedCondition(
        ILoggingService loggingService,
        IGameControllerService gameControllerService)
    {
        this.LoggingService = loggingService;
        this.GameControllerService = gameControllerService;
    }

    public event EventHandler? ConditionMet;

    private ILoggingService LoggingService { get; }
    private IGameControllerService GameControllerService { get; }

    public void StartMonitoring()
    {
        this.GameControllerService.GameControllerAdded += this.GameControllerGameControllerAdded;
        this.CheckConditionMet();
    }

    private void GameControllerGameControllerAdded(object? sender, EventArgs e)
    {
        try
        {
            this.CheckConditionMet();
        }
        catch (Exception ex)
        {
            this.LoggingService.LogException("handling game controller added", ex);
        }
    }

    public void StopMonitoring()
    {
        this.GameControllerService.GameControllerAdded -= this.GameControllerGameControllerAdded;
    }

    private void CheckConditionMet()
    {
        lock (this.checkConditionLock)
        {
            if (this.GameControllerService.HasAnyGameControllers)
            {
                this.ConditionMet?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}