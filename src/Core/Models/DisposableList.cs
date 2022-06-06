namespace AutoGame.Core.Models;

using System.Collections.ObjectModel;
using AutoGame.Core.Interfaces;

public sealed class DisposableList<T> : Collection<T>, IDisposableList<T>
    where T : IDisposable
{
    public DisposableList()
    {
    }

    public DisposableList(IEnumerable<T> enumerable)
    {
        foreach (T item in enumerable)
        {
            this.Add(item);
        }
    }

    public void Dispose()
    {
        foreach (T item in this.Items)
        {
            item.Dispose();
        }
    }
}