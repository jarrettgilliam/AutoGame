namespace AutoGame.Infrastructure.Windows.SoftwareManagers;

using System;
using System.IO;
using AutoGame.Core.Interfaces;
using AutoGame.Infrastructure.Windows.Interfaces;

internal sealed class OneGameLauncherManager : ISoftwareManager
{
    public OneGameLauncherManager(
        IUser32Service user32Service,
        IProcessService processService,
        IWindowService windowService)
    {
        this.User32Service = user32Service;
        this.ProcessService = processService;
        this.WindowService = windowService;
    }

    private IUser32Service User32Service { get; }
    private IProcessService ProcessService { get; }
    private IWindowService WindowService { get; }

    public string Key => "OneGameLauncher";

    public string Description => "One Game Launcher";

    public string DefaultArguments => "shell:appsfolder\\62269AlexShats.OneGameLauncherBeta_gghb1w55myjr2!App";

    private IntPtr Window => this.User32Service.FindWindow("ApplicationFrameWindow", "One Game Launcher (Free)");

    public bool IsRunning(string softwarePath) => this.Window == this.User32Service.GetForegroundWindow();

    public void Start(string softwarePath, string? softwareArguments)
    {
        this.ProcessService.Start(softwarePath, softwareArguments).Dispose();
        this.WindowService.RepeatTryMaximizeWindow(() => this.Window, TimeSpan.FromSeconds(5));
    }

    public string FindSoftwarePathOrDefault() =>
        Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.Windows),
            "explorer.exe");
}