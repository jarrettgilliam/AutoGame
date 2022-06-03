namespace AutoGame.Infrastructure.Interfaces;

internal interface IGameControllerService
{
    event EventHandler? GameControllerAdded;

    bool HasAnyGameControllers { get; }
}