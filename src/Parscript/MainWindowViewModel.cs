using Parscript.Infrastructure.Interfaces;
using Parscript.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Parscript
{
    class MainWindowViewModel : IDisposable
    {
        private List<IDisposable> disposables = new List<IDisposable>();
        private ParscriptService parscript;

        public MainWindowViewModel()
        {
            try
            {
                var changeDetectors = new IConnectionChangedDetector[]
                {
                    new ResolutionConnectionChangedDetector(),
                    new AudioMuteConnectionChangedDetector()
                };

                var statusProvider = new UDPListenerConnectionStatusProvider();
                ////var softwareManager = new SteamBigPictureSoftwareManager();
                var softwareManager = new PlayniteSoftwareManager();
                
                this.parscript = new ParscriptService(changeDetectors, statusProvider, softwareManager);

                this.disposables.AddRange(changeDetectors);
                this.disposables.Add(statusProvider);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
        }

        public void Dispose()
        {
            this.parscript?.Dispose();
            this.parscript = null;

            this.disposables?.ForEach(d => d.Dispose());
            this.disposables = null;
        }
    }
}
