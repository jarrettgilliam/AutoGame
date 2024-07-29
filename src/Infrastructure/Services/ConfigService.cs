namespace AutoGame.Infrastructure.Services;

using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;

public class ConfigService : IConfigService
{
    public ConfigService(
        IAppInfoService appInfo,
        IFileSystem fileSystem,
        IRuntimeInformation runtimeInformation,
        ISoftwareCollection availableSoftware)
    {
        this.AppInfo = appInfo;
        this.FileSystem = fileSystem;
        this.RuntimeInformation = runtimeInformation;
        this.AvailableSoftware = availableSoftware;
    }

    private IAppInfoService AppInfo { get; }
    private IFileSystem FileSystem { get; }
    private IRuntimeInformation RuntimeInformation { get; }
    private ISoftwareCollection AvailableSoftware { get; }

    public Config? GetConfigOrNull()
    {
        Config? config = null;

        try
        {
            using Stream s = this.FileSystem.File.OpenRead(this.AppInfo.ConfigFilePath);
            config = JsonSerializer.Deserialize(s, ConfigJsonSerializerContext.Default.Config);
            config!.IsDirty = false;
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
        {
            // No config found, return null
        }

        return config;
    }

    public void Save(Config config)
    {
        this.FileSystem.Directory.CreateDirectory(this.AppInfo.AppDataFolder);

        using Stream s = this.FileSystem.File.Create(this.AppInfo.ConfigFilePath);
        JsonSerializer.Serialize(s, config, ConfigJsonSerializerContext.Default.Config);

        config.IsDirty = false;
    }

    public Config CreateDefault()
    {
        ISoftwareManager? software = this.AvailableSoftware.FirstOrDefault();

        return new()
        {
            Version = 1,
            StartMinimized = true,
            CheckForUpdates = true,
            SoftwareKey = software?.Key,
            SoftwarePath = software?.FindSoftwarePathOrDefault(),
            SoftwareArguments = software?.DefaultArguments,
            LaunchWhenGameControllerConnected = true,
            LaunchWhenParsecConnected = true
        };
    }

    public void Validate(Config config)
    {
        config.ClearAllErrors();

        ISoftwareManager? softwareManager = this.AvailableSoftware.GetSoftwareByKeyOrNull(config.SoftwareKey);

        if (softwareManager is null)
        {
            config.AddError(nameof(config.SoftwareKey), "Unknown software");
        }
        else if (string.IsNullOrEmpty(config.SoftwarePath))
        {
            config.AddError(nameof(config.SoftwarePath), "Required");
        }
        else if (!this.ExecutableExists(config.SoftwarePath))
        {
            config.AddError(nameof(config.SoftwarePath), "File not found");
        }
        else if (!this.ExecutableNameMatches(config.SoftwarePath, softwareManager))
        {
            config.AddError(nameof(config.SoftwarePath), "Wrong software");
        }
    }

    private bool ExecutableExists(string softwarePath) =>
        this.RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            ? softwarePath.EndsWith(".app") && this.FileSystem.Directory.Exists(softwarePath)
            : this.FileSystem.File.Exists(softwarePath);

    private bool ExecutableNameMatches(string? softwarePath, ISoftwareManager softwareManager)
    {
        if (string.IsNullOrEmpty(softwarePath))
        {
            return false;
        }

        string defaultPath = softwareManager.FindSoftwarePathOrDefault();
        string defaultExecutable = this.FileSystem.Path.GetFileName(defaultPath);

        if (string.IsNullOrEmpty(defaultExecutable))
        {
            return true;
        }

        string softwareExecutable = this.FileSystem.Path.GetFileName(softwarePath);
        return string.Equals(defaultExecutable, softwareExecutable);
    }

    public void Upgrade(Config config)
    {
        if (config.Version == 0)
        {
            if (string.IsNullOrEmpty(config.SoftwareArguments))
            {
                ISoftwareManager? software = this.AvailableSoftware.GetSoftwareByKeyOrNull(config.SoftwareKey);
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