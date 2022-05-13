namespace AutoGame.Core.Interfaces;

public interface IRawGameControllerService
{
    event EventHandler? RawGameControllerAdded;

    bool HasAnyRawGameControllers { get; }
}