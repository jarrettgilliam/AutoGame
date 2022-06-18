namespace AutoGame.Core.Interfaces;

public interface IProcess : IDisposable
{
    int Id { get; }
}