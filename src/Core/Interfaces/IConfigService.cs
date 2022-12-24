namespace AutoGame.Core.Interfaces;

using AutoGame.Core.Models;

public interface IConfigService
{
    Config? GetConfigOrNull();

    void Save(Config config);

    Config CreateDefault();

    void Validate(Config config);

    void Upgrade(Config config);
}