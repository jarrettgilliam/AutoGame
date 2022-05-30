namespace AutoGame.Core.Services;

using System;
using System.IO;
using System.IO.Abstractions;
using System.Text.Json;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;

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
            using Stream s = this.FileSystem.File.OpenRead(this.AppInfo.ConfigFilePath);
            config = JsonSerializer.Deserialize<Config>(s);
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

        using Stream s = this.FileSystem.File.Create(this.AppInfo.ConfigFilePath);
        var options = new JsonSerializerOptions { WriteIndented = true };
        JsonSerializer.Serialize(s, config, options);

        config.IsDirty = false;
    }

    public Config CreateDefault(ISoftwareManager? software) =>
        new()
        {
            SoftwareKey = software?.Key,
            SoftwarePath = software?.FindSoftwarePathOrDefault(),
            LaunchWhenGamepadConnected = true,
            LaunchWhenParsecConnected = true
        };
}