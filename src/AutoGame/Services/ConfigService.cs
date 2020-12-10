using AutoGame.Models;
using Newtonsoft.Json;
using System;
using System.IO;

namespace AutoGame.Services
{
    internal class ConfigService
    {
        private readonly string configPath =
            Path.Join(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                nameof(AutoGame),
                nameof(Config) + ".json");

        public Config Load(Func<Config> defaultConfigFactory)
        {
            Config config;

            try
            {
                using (StreamReader sr = new StreamReader(this.configPath))
                using (JsonTextReader reader = new JsonTextReader(sr))
                {
                    config = JsonSerializer.CreateDefault().Deserialize<Config>(reader);
                }
            }
            catch (Exception ex) when (
                ex is FileNotFoundException ||
                ex is DirectoryNotFoundException)
            {
                config = defaultConfigFactory.Invoke();
            }

            config.IsDirty = false;

            return config;
        }

        public void Save(Config config)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(this.configPath));

            using (StreamWriter sw = new StreamWriter(this.configPath))
            using (JsonTextWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;
                JsonSerializer.CreateDefault().Serialize(writer, config);
            }

            config.IsDirty = false;
        }
    }
}
