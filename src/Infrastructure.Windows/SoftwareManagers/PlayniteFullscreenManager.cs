namespace AutoGame.Infrastructure.Windows.SoftwareManagers;

using System;
using System.IO.Abstractions;
using System.Linq;
using AutoGame.Core.Interfaces;
using AutoGame.Infrastructure.Windows.Interfaces;

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

    public string DefaultArguments => "--startfullscreen";

    public bool IsRunning(string softwarePath)
    {
        using IDisposableList<IProcess> procs = this.ProcessService.GetProcessesByName(PLAYNITE_FULLSCREEN_APP);
        return procs.Any();
    }

    public void Start(string softwarePath, string? softwareArguments)
    {
        this.ProcessService.Start(softwarePath, softwareArguments).Dispose();
        this.WindowService.RepeatTryForceForegroundWindowByTitle("Playnite", TimeSpan.FromSeconds(5));
    }

    public string FindSoftwarePathOrDefault() =>
        this.FileSystem.Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Playnite",
            $"{PLAYNITE_FULLSCREEN_APP}.exe");
}