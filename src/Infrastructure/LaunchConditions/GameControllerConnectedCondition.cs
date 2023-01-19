namespace AutoGame.Infrastructure.LaunchConditions;

using System;
using AutoGame.Core.Interfaces;
using Serilog;

internal sealed class GameControllerConnectedCondition : IGameControllerConnectedCondition
{
    private readonly object checkConditionLock = new();

    public GameControllerConnectedCondition(
        ILogger logger,
        IGameControllerService gameControllerService)
    {
        this.Logger = logger;
        this.GameControllerService = gameControllerService;
    }

    public event EventHandler? ConditionMet;

    private ILogger Logger { get; }
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
            this.Logger.Error(ex, "handling game controller added");
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