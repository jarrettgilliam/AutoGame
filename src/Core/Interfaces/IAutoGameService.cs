namespace AutoGame.Core.Interfaces;

using System;
using System.Collections.Generic;
using AutoGame.Core.Models;

public interface IAutoGameService : IDisposable
{
    IEnumerable<ISoftwareManager> AvailableSoftware { get; }

    ISoftwareManager? GetSoftwareByKeyOrNull(string? softwareKey);

    void ApplyConfiguration(Config config);
}