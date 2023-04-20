namespace AutoGame.Infrastructure.Windows.SoftwareManagers;

using System;
using System.IO;
using System.IO.Abstractions;
using AutoGame.Core.Interfaces;
using AutoGame.Infrastructure.Windows.Interfaces;
using AutoGame.Infrastructure.Windows.Services;
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

    private nint Window => this.User32Service.FindWindow("ApplicationFrameWindow", "One Game Launcher (Free)");

    public void MaximizeWindow()
    {
        if (this.User32Service is WindowsUser32Service s && Window != IntPtr.Zero)
        {
            s.SetForegroundWindow(Window);
            s.ShowWindowAsync(Window, 3);
        }
    }

    public bool IsRunning(string softwarePath) => Window == this.User32Service.GetForegroundWindow();

    public void Start(string softwarePath, string? softwareArguments)
    {
        ProcessService.Start(softwarePath, softwareArguments).Dispose();
        MaximizeWindow();
    }

    public string FindSoftwarePathOrDefault() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");
}