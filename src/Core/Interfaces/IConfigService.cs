namespace AutoGame.Core.Interfaces;

using System.Collections.Generic;
using AutoGame.Core.Models;

public interface IConfigService
{
    Config? GetConfigOrNull();

    void Save(Config config);

    Config CreateDefault(ISoftwareManager? software);

    void Validate(Config config, IEnumerable<ISoftwareManager> knownSoftware);

    void Upgrade(Config config, ISoftwareManager? software);
}