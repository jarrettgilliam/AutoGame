namespace AutoGame.Core.Interfaces;

public interface IProcessService
{
    IProcess NewProcess();

    IProcess Start(string fileName);
    
    IProcess Start(string fileName, string arguments);

    IProcess[] GetProcessesByName(string? processName);
}