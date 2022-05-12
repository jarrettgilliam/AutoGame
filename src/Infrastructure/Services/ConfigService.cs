using System;
using System.IO;
using AutoGame.Infrastructure.Constants;
using AutoGame.Infrastructure.Interfaces;
using AutoGame.Infrastructure.Models;
using Newtonsoft.Json;

namespace AutoGame.Infrastructure.Services
{
    public class ConfigService : IConfigService
    {
        private static readonly string ConfigPath =
            Path.Join(Strings.AppDataFolder, nameof(Config) + ".json");

        public Config? GetConfigOrNull()
        {
            Config? config = null;

            try
            {
                using (var sr = new StreamReader(ConfigPath))
                using (var reader = new JsonTextReader(sr))
                {
                    config = JsonSerializer.CreateDefault().Deserialize<Config>(reader);
                    config!.IsDirty = false;
                }
            }
            catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
            {
            }

            return config;
        }

        public void Save(Config config)
        {
            Directory.CreateDirectory(Strings.AppDataFolder);

            using (var sw = new StreamWriter(ConfigPath))
            using (var writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;
                JsonSerializer.CreateDefault().Serialize(writer, config);
            }

            config.IsDirty = false;
        }
    }
}
