namespace AutoGame.Core.Interfaces;

public interface IDisposableList<T> : IList<T>, IDisposable
    where T : IDisposable
{
}