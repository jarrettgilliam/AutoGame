namespace AutoGame.Core.Interfaces;

using AutoGame.Core.Models;

public interface IAutoGameService : IDisposable
{
    IList<ISoftwareManager> AvailableSoftware { get; }

    Config CreateDefaultConfiguration();

    ISoftwareManager? GetSoftwareByKeyOrNull(string? softwareKey);

    bool TryApplyConfiguration(Config config);
}