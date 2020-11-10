using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Parscript.Infrastructure.Enums;
using Parscript.Infrastructure.Interfaces;

namespace Parscript.Infrastructure.Services
{
    public class ParscriptService : IDisposable
    {
        private readonly SemaphoreSlim connectionChangedMutex = new SemaphoreSlim(1, 1);

        public ParscriptService(
            IReadOnlyList<IConnectionChangedDetector> changedDetectors, 
            IConnectionStatusProvider statusProvider,
            ISoftwareManager softwareManager)
        {
            this.ChangedDetectors = changedDetectors;
            this.StatusProvider = statusProvider;
            this.Software = softwareManager;

            foreach (IConnectionChangedDetector detector in this.ChangedDetectors)
            {
                detector.ConnectionChanged += this.HandleConnectionChanged;
            }

            this.HandleConnectionChanged(this, EventArgs.Empty);
        }

        private IReadOnlyList<IConnectionChangedDetector> ChangedDetectors { get; }

        private IConnectionStatusProvider StatusProvider { get; }

        private ISoftwareManager Software { get; }

        private ConnectionStatus CurrentStatus { get; set; }

        public void Dispose()
        {
            foreach (IConnectionChangedDetector detector in this.ChangedDetectors)
            {
                detector.ConnectionChanged -= this.HandleConnectionChanged;
            }
        }

        private async void HandleConnectionChanged(object sender, EventArgs e)
        {
            await this.connectionChangedMutex.WaitAsync();

            try
            {
                ConnectionStatus newStatus = await this.StatusProvider.ProvideStatus(this.CurrentStatus);

                if (newStatus != this.CurrentStatus)
                {
                    if (newStatus == ConnectionStatus.Disconnected && this.Software.IsRunning)
                    {
                        this.Software.Stop();
                    }
                    else if (newStatus == ConnectionStatus.Connected && !this.Software.IsRunning)
                    {
                        this.Software.Start();
                    }

                    this.CurrentStatus = newStatus;
                }
            }
            finally
            {
                this.connectionChangedMutex.Release();
            }
        }
    }
}
