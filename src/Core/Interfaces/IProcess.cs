namespace AutoGame.Core.Interfaces;

using System;

public interface IProcess : IDisposable
{
    int Id { get; }
}