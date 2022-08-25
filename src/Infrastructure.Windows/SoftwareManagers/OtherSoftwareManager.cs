namespace AutoGame.Infrastructure.Windows.SoftwareManagers;

using System.IO.Abstractions;
using System.Linq;
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

    public string DefaultArguments => "";

    public bool IsRunning(string softwarePath)
    {
        string software = this.FileSystem.Path.GetFileName(softwarePath);

        if (software.EndsWith(".exe"))
        {
            software = this.FileSystem.Path.ChangeExtension(software, null);
        }

        using IDisposableList<IProcess> procs = this.ProcessService.GetProcessesByName(software);

        return procs.Any();
    }

    public void Start(string softwarePath, string? softwareArguments)
    {
        this.ProcessService.Start(softwarePath, softwareArguments).Dispose();
    }

    public string FindSoftwarePathOrDefault() => "";
}