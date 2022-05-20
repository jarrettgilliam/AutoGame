namespace AutoGame.Core.Services;

using System;
using System.IO;
using System.IO.Abstractions;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;
using Newtonsoft.Json;

public class ConfigService : IConfigService
{
    public ConfigService(
        IAppInfoService appInfo,
        IFileSystem fileSystem)
    {
        this.AppInfo = appInfo;
        this.FileSystem = fileSystem;
    }
        
    private IAppInfoService AppInfo { get; }
    private IFileSystem FileSystem { get; }

    public Config? GetConfigOrNull()
    {
        Config? config = null;

        try
        {
            using StreamReader sr = this.FileSystem.File.OpenText(this.AppInfo.ConfigFilePath);
            using JsonTextReader reader = new(sr);

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
        this.FileSystem.Directory.CreateDirectory(this.AppInfo.AppDataFolder);

        using StreamWriter sw = this.FileSystem.File.CreateText(this.AppInfo.ConfigFilePath);
        using JsonTextWriter writer = new(sw);
        
        writer.Formatting = Formatting.Indented;
        JsonSerializer.CreateDefault().Serialize(writer, config);

        config.IsDirty = false;
    }
}