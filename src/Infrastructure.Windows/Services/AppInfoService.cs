// ReSharper disable once CheckNamespace

namespace AutoGame.Infrastructure;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;

public sealed class AppInfoService : IAppInfoService
{
    public AppInfoService()
    {
        this.AppDataFolder =
            Path.Join(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                nameof(AutoGame));

        this.ConfigFilePath =
            Path.Join(
                this.AppDataFolder,
                nameof(Config) + ".json");

        this.LogFilePath =
            Path.Join(
                this.AppDataFolder,
                "Log.txt");

        this.ParsecLogDirectories = new[]
        {
            Path.Join(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Parsec"),
            Path.Join(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "Parsec")
        };

        this.CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version
            ?? throw new InvalidOperationException("Unable get current application version");
    }

    public string AppDataFolder { get; }
    public string ConfigFilePath { get; }
    public string LogFilePath { get; }
    public IEnumerable<string> ParsecLogDirectories { get; }
    public Version CurrentVersion { get; }
}