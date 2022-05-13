namespace AutoGame.Infrastructure.Services;

using System;
using System.IO;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;
using Newtonsoft.Json;

public class ConfigService : IConfigService
{
    public ConfigService(IAppInfoService appInfo)
    {
        this.AppInfo = appInfo;
    }
        
    private IAppInfoService AppInfo { get; }

    public Config? GetConfigOrNull()
    {
        Config? config = null;

        try
        {
            using var sr = new StreamReader(this.AppInfo.ConfigFilePath);
            using var reader = new JsonTextReader(sr);

            config = JsonSerializer.CreateDefault().Deserialize<Config>(reader);
            config!.IsDirty = false;
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
        {
        }

        return config;
    }

    public void Save(Config config)
    {
        Directory.CreateDirectory(this.AppInfo.AppDataFolder);

        using (var sw = new StreamWriter(this.AppInfo.ConfigFilePath))
        using (var writer = new JsonTextWriter(sw))
        {
            writer.Formatting = Formatting.Indented;
            JsonSerializer.CreateDefault().Serialize(writer, config);
        }

        config.IsDirty = false;
    }
}