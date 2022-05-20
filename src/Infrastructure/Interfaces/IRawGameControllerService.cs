namespace AutoGame.Infrastructure.Interfaces;

public interface IRawGameControllerService
{
    event EventHandler? RawGameControllerAdded;

    bool HasAnyRawGameControllers { get; }
}