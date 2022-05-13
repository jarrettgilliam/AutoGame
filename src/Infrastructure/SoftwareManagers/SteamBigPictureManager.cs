namespace AutoGame.Infrastructure.SoftwareManagers;

using AutoGame.Core.Interfaces;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;

public class SteamBigPictureManager : ISoftwareManager
{
    public SteamBigPictureManager(ILoggingService loggingService, IUser32Service user32Service)
    {
        this.LoggingService = loggingService;
        this.User32Service = user32Service;
    }

    private ILoggingService LoggingService { get; }
    
    private IUser32Service User32Service { get; }

    public string Key => "SteamBigPicture";

    public string Description => "Steam Big Picture";

    // From: https://www.displayfusion.com/ScriptedFunctions/View/?ID=b21d08ca-438a-41e5-8b9d-0125b07a2abc
    public bool IsRunning => this.User32Service.FindWindow("CUIEngineWin32", "Steam") != IntPtr.Zero;

    public void Start(string softwarePath)
    {
        Process.Start(softwarePath, "-start steam://open/bigpicture -fulldesktopres");
    }

    public string FindSoftwarePathOrDefault()
    {
        string defaultSteamPath = Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            "Steam",
            "steam.exe");

        try
        {
            string? registryPath = (string?)Registry.GetValue(
                keyName: @"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam",
                valueName: "SteamExe",
                defaultValue: defaultSteamPath);

            if (registryPath is not null)
            {
                return Path.GetFullPath(registryPath);
            }
        }
        catch (Exception ex)
        {
            this.LoggingService.LogException("finding the path to steam", ex);
        }

        return defaultSteamPath;
    }
}