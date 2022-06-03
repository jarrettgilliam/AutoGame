using AutoGame.Core.Models;

namespace AutoGame.Core.Interfaces;

public interface IConfigService
{
    Config? GetConfigOrNull();

    void Save(Config config);

    Config CreateDefault(ISoftwareManager? software);

    void Validate(Config config, IEnumerable<ISoftwareManager> knownSoftware);

    void Upgrade(Config config, ISoftwareManager? software);
}