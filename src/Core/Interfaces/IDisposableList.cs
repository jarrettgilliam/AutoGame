namespace AutoGame.Core.Interfaces;

using System;
using System.Collections.Generic;

public interface IDisposableList<T> : IList<T>, IDisposable
    where T : IDisposable
{
}