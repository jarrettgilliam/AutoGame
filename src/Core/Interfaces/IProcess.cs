namespace AutoGame.Core.Interfaces;

using System.Diagnostics;

public interface IProcess : IDisposable
{
    ProcessStartInfo StartInfo { get; set; }
    
    StreamReader StandardOutput { get; }
    
    StreamReader StandardError { get; }

    int Id { get; }
    
    int ExitCode { get; }

    bool Start();
}