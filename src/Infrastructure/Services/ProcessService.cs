namespace AutoGame.Infrastructure.Services;

using System.Diagnostics;
using System.Linq;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;

internal sealed class ProcessService : IProcessService
{
    public IProcess Start(string fileName, string? arguments) =>
        new ProcessWrapper(Process.Start(fileName, arguments ?? ""));

    public IDisposableList<IProcess> GetProcessesByName(string? processName) =>
        new DisposableList<IProcess>(
            Process.GetProcessesByName(processName)
                .Select(p => new ProcessWrapper(p)));
    
    private sealed class ProcessWrapper : IProcess
    {
        private readonly Process process;
        
        public ProcessWrapper(Process process)
        {
            this.process = process;
        }

        public int Id => this.process.Id;

        public void Dispose() => this.process.Dispose();
    }
}