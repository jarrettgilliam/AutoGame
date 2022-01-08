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

        public Config GetConfigOrNull()
        {
            Config config = null;

            try
            {
                using (StreamReader sr = new StreamReader(ConfigPath))
                using (JsonTextReader reader = new JsonTextReader(sr))
                {
                    config = JsonSerializer.CreateDefault().Deserialize<Config>(reader);
                    config.IsDirty = false;
                }
            }
            catch (Exception ex) when (
                ex is FileNotFoundException ||
                ex is DirectoryNotFoundException)
            {
            }

            return config;
        }

        public void Save(Config config)
        {
            Directory.CreateDirectory(Strings.AppDataFolder);

            using (StreamWriter sw = new StreamWriter(ConfigPath))
            using (JsonTextWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;
                JsonSerializer.CreateDefault().Serialize(writer, config);
            }

            config.IsDirty = false;
        }
    }
}
