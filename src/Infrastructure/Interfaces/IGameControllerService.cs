namespace AutoGame.Infrastructure.Interfaces;

public interface IGameControllerService
{
    event EventHandler? GameControllerAdded;

    bool HasAnyGameControllers { get; }
}