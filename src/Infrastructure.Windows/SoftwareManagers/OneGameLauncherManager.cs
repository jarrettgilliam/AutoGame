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

    public string Key => "OneGameLauncher";

    public string Description => "One Game Launcher";

    public string DefaultArguments => "shell:appsfolder\\62269AlexShats.OneGameLauncherBeta_gghb1w55myjr2!App";

    public bool IsRunning(string softwarePath)
    {
         return   this.User32Service.FindWindow("CUIEngineWin32", "GameLauncherWidget") != IntPtr.Zero;

    }

    public void Start(string softwarePath, string? softwareArguments) =>
        this.ProcessService.Start(softwarePath, softwareArguments).Dispose();

    public string FindSoftwarePathOrDefault()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");
    }
}