using System;
using System.Collections.Generic;
using AutoGame.Infrastructure.Models;

namespace AutoGame.Infrastructure.Interfaces
{
    public interface IAutoGameService : IDisposable
    {
        IList<ISoftwareManager> AvailableSoftware { get; }

        Config CreateDefaultConfiguration();

        ISoftwareManager? GetSoftwareByKeyOrNull(string? softwareKey);

        bool TryApplyConfiguration(Config config);
    }
}
