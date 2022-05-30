﻿namespace AutoGame.Infrastructure.SoftwareManagers;

using AutoGame.Core.Interfaces;
using System;
using System.IO.Abstractions;

internal sealed class SteamBigPictureManager : ISoftwareManager
{
    public SteamBigPictureManager(
        ILoggingService loggingService,
        IUser32Service user32Service,
        IFileSystem fileSystem,
        IProcessService processService,
        IRegistryService registryService)
    {
        this.LoggingService = loggingService;
        this.User32Service = user32Service;
        this.FileSystem = fileSystem;
        this.ProcessService = processService;
        this.RegistryService = registryService;
    }

    private ILoggingService LoggingService { get; }
    private IUser32Service User32Service { get; }
    private IFileSystem FileSystem { get; }
    private IProcessService ProcessService { get; }
    private IRegistryService RegistryService { get; }

    public string Key => "SteamBigPicture";

    public string Description => "Steam Big Picture";

    // From: https://www.displayfusion.com/ScriptedFunctions/View/?ID=b21d08ca-438a-41e5-8b9d-0125b07a2abc
    public bool IsRunning => this.User32Service.FindWindow("CUIEngineWin32", "Steam") != IntPtr.Zero;

    public void Start(string softwarePath)
    {
        this.ProcessService.Start(softwarePath, "-start steam://open/bigpicture -fulldesktopres");
    }

    public string FindSoftwarePathOrDefault()
    {
        string defaultSteamPath = this.FileSystem.Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            "Steam",
            "steam.exe");

        try
        {
            string? registryPath = (string?)this.RegistryService.GetValue(
                keyName: @"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam",
                valueName: "SteamExe",
                defaultValue: defaultSteamPath);

            if (registryPath is not null)
            {
                return this.FileSystem.Path.GetFullPath(registryPath);
            }
        }
        catch (Exception ex)
        {
            this.LoggingService.LogException("finding the path to steam", ex);
        }

        return defaultSteamPath;
    }
}