using AutoGame.Infrastructure.Models;

namespace AutoGame.Infrastructure.Interfaces
{
    public interface IConfigService
    {
        Config? GetConfigOrNull();

        void Save(Config config);
    }
}
