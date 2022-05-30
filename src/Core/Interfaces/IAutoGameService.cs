namespace AutoGame.Core.Interfaces;

using AutoGame.Core.Models;

public interface IAutoGameService : IDisposable
{
    IEnumerable<ISoftwareManager> AvailableSoftware { get; }

    ISoftwareManager? GetSoftwareByKeyOrNull(string? softwareKey);

    void ApplyConfiguration(Config config);
}