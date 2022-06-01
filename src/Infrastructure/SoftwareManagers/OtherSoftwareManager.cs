namespace AutoGame.Infrastructure.SoftwareManagers;

using System.IO.Abstractions;
using AutoGame.Core.Interfaces;

internal sealed class OtherSoftwareManager : ISoftwareManager
{
    public OtherSoftwareManager(
        IProcessService processService,
        IFileSystem fileSystem)
    {
        this.ProcessService = processService;
        this.FileSystem = fileSystem;
    }

    private IProcessService ProcessService { get; }
    private IFileSystem FileSystem { get; }
    
    public string Key => "Other";
    
    public string Description => "Other";
    
    public bool IsRunning(string softwarePath)
    {
        string software = this.FileSystem.Path.GetFileName(softwarePath);

        if (software.EndsWith(".exe"))
        {
            software = this.FileSystem.Path.ChangeExtension(software, null);
        }

        return this.ProcessService.GetProcessesByName(software).Any();
    }

    public void Start(string softwarePath)
    {
        this.ProcessService.Start(softwarePath);
    }

    public string FindSoftwarePathOrDefault() => "";
}