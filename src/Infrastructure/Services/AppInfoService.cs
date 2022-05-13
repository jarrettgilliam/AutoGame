namespace AutoGame.Infrastructure.Services;

using System;
using System.IO;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;

public class AppInfoService : IAppInfoService
{
    public AppInfoService()
    {
        this.AppDataFolder =
            Path.Join(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                nameof(AutoGame));
        
        this.ConfigFilePath = 
            Path.Join(
                this.AppDataFolder, 
                nameof(Config) + ".json");
        
        this.LogFilePath = 
            Path.Join(
                this.AppDataFolder,
                "Log.txt");
    }
    
    public string AppDataFolder { get; }

    public string ConfigFilePath { get; }
    
    public string LogFilePath { get; }
}