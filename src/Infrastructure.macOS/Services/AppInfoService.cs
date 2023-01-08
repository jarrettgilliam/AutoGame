namespace AutoGame.Infrastructure.macOS.Services;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Reflection;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;

internal sealed class AppInfoService : IAppInfoService
{
    public AppInfoService(IFileSystem fileSystem)
    {
        this.FileSystem = fileSystem;

        this.AppDataFolder =
            this.FileSystem.Path.Join(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.Personal),
                "Library",
                "Application Support",
                nameof(AutoGame));

        this.ConfigFilePath =
            this.FileSystem.Path.Join(
                this.AppDataFolder,
                nameof(Config) + ".json");

        this.LogFilePath =
            this.FileSystem.Path.Join(
                this.AppDataFolder,
                "Log.txt");

        this.ParsecLogDirectories = new[]
        {
            this.FileSystem.Path.Join(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.Personal),
                ".parsec"),
            "/Users/Shared/.parsec"
        };

        this.CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version
            ?? throw new InvalidOperationException("Unable get current application version");
    }

    private IFileSystem FileSystem { get; }

    public string AppDataFolder { get; }
    public string ConfigFilePath { get; }
    public string LogFilePath { get; }
    public IEnumerable<string> ParsecLogDirectories { get; }
    public Version CurrentVersion { get; }
}