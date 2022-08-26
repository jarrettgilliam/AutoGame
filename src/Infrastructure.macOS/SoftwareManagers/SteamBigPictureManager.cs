namespace AutoGame.Infrastructure.macOS.SoftwareManagers;

using AutoGame.Core.Interfaces;

internal sealed class SteamBigPictureManager : ISoftwareManager
{
    public SteamBigPictureManager(
        IProcessService processService)
    {
        this.ProcessService = processService;
    }

    private IProcessService ProcessService { get; }

    public string Key => "SteamBigPicture";

    public string Description => "Steam Big Picture";

    public string DefaultArguments => "-start steam://open/bigpicture";
    
    public bool IsRunning(string softwarePath) => false;

    public void Start(string softwarePath, string? softwareArguments)
    {
        this.ProcessService.Start("open", $"-a \"{softwarePath}\" {softwareArguments}");
    }

    public string FindSoftwarePathOrDefault() => "/Applications/Steam.app";
}