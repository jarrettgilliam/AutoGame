namespace AutoGame.Infrastructure.Windows.SoftwareManagers;

using System;
using System.IO.Abstractions;
using AutoGame.Core.Interfaces;
using AutoGame.Infrastructure.Windows.Interfaces;
using Serilog;

internal sealed class SteamBigPictureManager : ISoftwareManager
{
    public SteamBigPictureManager(
        ILogger logger,
        IUser32Service user32Service,
        IFileSystem fileSystem,
        IProcessService processService,
        IRegistryService registryService)
    {
        this.Logger = logger;
        this.User32Service = user32Service;
        this.FileSystem = fileSystem;
        this.ProcessService = processService;
        this.RegistryService = registryService;
    }

    private ILogger Logger { get; }
    private IUser32Service User32Service { get; }
    private IFileSystem FileSystem { get; }
    private IProcessService ProcessService { get; }
    private IRegistryService RegistryService { get; }

    public string Key => "SteamBigPicture";

    public string Description => "Steam Big Picture";

    public string DefaultArguments => "-start steam://open/bigpicture -fulldesktopres";

    public bool IsRunning(string softwarePath)
    {
        // Original Steam Big Picture
        // From: https://www.displayfusion.com/ScriptedFunctions/View/?ID=b21d08ca-438a-41e5-8b9d-0125b07a2abc
        if (this.User32Service.FindWindow("CUIEngineWin32", "Steam") != IntPtr.Zero)
        {
            return true;
        }

        // New Steam Big Picture (Steam Deck UI)
        // Found using Spy++ https://learn.microsoft.com/en-us/visualstudio/debugger/how-to-start-spy-increment
        if (this.User32Service.FindWindow("SDL_app", "SP") != IntPtr.Zero)
        {
            return true;
        }

        return false;
    }

    public void Start(string softwarePath, string? softwareArguments) =>
        this.ProcessService.Start(softwarePath, softwareArguments).Dispose();

    public string FindSoftwarePathOrDefault()
    {
        string defaultSteamPath = this.FileSystem.Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            "Steam",
            "steam.exe");

        try
        {
            string? registryPath = (string?)this.RegistryService.GetValue(
                @"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam",
                "SteamExe",
                defaultSteamPath);

            if (registryPath is not null)
            {
                return this.FileSystem.Path.GetFullPath(registryPath);
            }
        }
        catch (Exception ex)
        {
            this.Logger.Error(ex, "finding the path to steam");
        }

        return defaultSteamPath;
    }
}