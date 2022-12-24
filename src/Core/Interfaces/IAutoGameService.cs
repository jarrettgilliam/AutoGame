namespace AutoGame.Core.Interfaces;

using System;
using AutoGame.Core.Models;

public interface IAutoGameService : IDisposable
{
    void ApplyConfiguration(Config config);
}