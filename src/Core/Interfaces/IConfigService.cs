using AutoGame.Core.Models;

namespace AutoGame.Core.Interfaces;

public interface IConfigService
{
    Config? GetConfigOrNull();

    void Save(Config config);
}