using System;
using AutoGame.Infrastructure.Models;

namespace AutoGame.Infrastructure.Interfaces
{
    public interface IConfigService
    {
        Config Load(Func<Config> defaultConfigFactory);

        void Save(Config config);
    }
}
