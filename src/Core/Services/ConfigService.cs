namespace AutoGame.Core.Services;

using System;
using System.IO;
using System.IO.Abstractions;
using System.Text.Json;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;

internal sealed class ConfigService : IConfigService
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
            LaunchWhenGameControllerConnected = true,
            LaunchWhenParsecConnected = true
        };

    public void Validate(Config config, IEnumerable<ISoftwareManager> knownSoftware)
    {
        config.ClearAllErrors();

        if (string.IsNullOrEmpty(config.SoftwarePath))
        {
            config.AddError(nameof(config.SoftwarePath), "Required");
        }
        else if (!this.FileSystem.File.Exists(config.SoftwarePath))
        {
            config.AddError(nameof(config.SoftwarePath), "File not found");
        }

        if (!knownSoftware.Any(x => x.Key == config.SoftwareKey))
        {
            config.AddError(nameof(config.SoftwareKey), "Unknown software");
        }
    }

    public void Upgrade(Config config, ISoftwareManager? software)
    {
        if (config.Version == 0)
        {
            if (string.IsNullOrEmpty(config.SoftwareArguments))
            {
                config.SoftwareArguments = software?.DefaultArguments;
            }

            const string oldPropertyName = "LaunchWhenGamepadConnected";
            if (config.JsonExtensionData?.TryGetValue(oldPropertyName, out JsonElement value) == true)
            {
                config.LaunchWhenGameControllerConnected = value.ValueKind == JsonValueKind.True;
                config.JsonExtensionData.Remove(oldPropertyName);
            }
            
            config.Version++;
        }
    }
}