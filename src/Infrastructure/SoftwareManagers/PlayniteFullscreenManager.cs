namespace AutoGame.Infrastructure.SoftwareManagers;

using AutoGame.Core.Interfaces;
using System;
using System.IO.Abstractions;
using System.Linq;

internal sealed class PlayniteFullscreenManager : ISoftwareManager
{
    private const string PLAYNITE_FULLSCREEN_APP = "Playnite.FullscreenApp";

    public PlayniteFullscreenManager(
        IWindowService windowService,
        IFileSystem fileSystem,
        IProcessService processService)
    {
        this.WindowService = windowService;
        this.FileSystem = fileSystem;
        this.ProcessService = processService;
    }
        
    private IWindowService WindowService { get; }
    private IFileSystem FileSystem { get; }
    private IProcessService ProcessService { get; }

    public string Key => "PlayniteFullscreen";

    public string Description => "Playnite Fullscreen";

    public bool IsRunning(string softwarePath) =>
        this.ProcessService.GetProcessesByName(PLAYNITE_FULLSCREEN_APP).Any();

    public void Start(string softwarePath)
    {
        this.ProcessService.Start(softwarePath, "--startfullscreen");
        this.WindowService.RepeatTryForceForegroundWindowByTitle("Playnite", TimeSpan.FromSeconds(5));
    }

    public string FindSoftwarePathOrDefault() =>
        this.FileSystem.Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Playnite",
            $"{PLAYNITE_FULLSCREEN_APP}.exe");
}