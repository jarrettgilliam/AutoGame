namespace AutoGame.Infrastructure.Services;

using System.Diagnostics;
using System.IO;
using System.Linq;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;

internal sealed class ProcessService : IProcessService
{
    public IProcess NewProcess() =>
        new ProcessWrapper(new Process());

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

        public ProcessStartInfo StartInfo
        {
            get => this.process.StartInfo;
            set => this.process.StartInfo = value;
        }

        public StreamReader StandardOutput => this.process.StandardOutput;

        public StreamReader StandardError => this.process.StandardError;

        public int Id => this.process.Id;

        public int ExitCode => this.process.ExitCode;

        public bool Start() => this.process.Start();

        public void WaitForExit() => this.process.WaitForExit();

        public void Dispose() => this.process.Dispose();
    }
}