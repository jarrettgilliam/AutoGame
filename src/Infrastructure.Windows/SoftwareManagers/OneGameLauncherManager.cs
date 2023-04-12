namespace AutoGame.Infrastructure.Windows.SoftwareManagers;

using System;
using System.IO;
using System.IO.Abstractions;
using AutoGame.Core.Interfaces;
using AutoGame.Infrastructure.Windows.Interfaces;
using Serilog;

internal sealed class OneGameLauncherManager : ISoftwareManager
{
    public OneGameLauncherManager(
        IUser32Service user32Service,
        IProcessService processService
       )
    {
        this.User32Service = user32Service;
        this.ProcessService = processService;

    }


    private IUser32Service User32Service { get; }
    private IProcessService ProcessService { get; }


    public string Key => "OneGameLauncher";

    public string Description => "One Game Launcher";

    public string DefaultArguments => "shell:appsfolder\\62269AlexShats.OneGameLauncherBeta_gghb1w55myjr2!App";

    public bool IsRunning(string softwarePath) => this.User32Service.FindWindow("ApplicationFrameWindow", "One Game Launcher (Free)") != IntPtr.Zero;

    public void Start(string softwarePath, string? softwareArguments) => this.ProcessService.Start(softwarePath, softwareArguments).Dispose();

    public string FindSoftwarePathOrDefault() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");
}