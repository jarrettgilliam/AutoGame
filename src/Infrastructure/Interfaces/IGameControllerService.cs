namespace AutoGame.Infrastructure.Interfaces;

using System;

public interface IGameControllerService
{
    event EventHandler? GameControllerAdded;

    bool HasAnyGameControllers { get; }
}