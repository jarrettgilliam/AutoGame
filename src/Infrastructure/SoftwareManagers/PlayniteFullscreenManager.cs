namespace AutoGame.Infrastructure.SoftwareManagers;

using AutoGame.Core.Interfaces;
using System;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;

public class PlayniteFullscreenManager : ISoftwareManager
{
    public PlayniteFullscreenManager(
        IWindowService windowService,
        IFileSystem fileSystem)
    {
        this.WindowService = windowService;
        this.FileSystem = fileSystem;
    }
        
    private IWindowService WindowService { get; }
    private IFileSystem FileSystem { get; }
        
    private const string PLAYNITE_FULLSCREEN_APP = "Playnite.FullscreenApp";

    public string Key => "PlayniteFullscreen";

    public string Description => "Playnite Fullscreen";

    public bool IsRunning => Process.GetProcessesByName(PLAYNITE_FULLSCREEN_APP).Any();

    public void Start(string softwarePath)
    {
        Process.Start(softwarePath);
        this.WindowService.RepeatTryForceForegroundWindowByTitle("Playnite", TimeSpan.FromSeconds(5));
    }

    public string FindSoftwarePathOrDefault()
    {
        return this.FileSystem.Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Playnite",
            $"{PLAYNITE_FULLSCREEN_APP}.exe");
    }
}