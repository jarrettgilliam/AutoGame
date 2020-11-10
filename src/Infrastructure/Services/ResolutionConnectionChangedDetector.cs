using Parscript.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parscript.Infrastructure.Services
{
    public class ResolutionConnectionChangedDetector : IConnectionChangedDetector, IDisposable
    {
        public ResolutionConnectionChangedDetector()
        {
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += this.SystemEvents_DisplaySettingsChanged;
        }

        public event EventHandler ConnectionChanged;

        public void Dispose()
        {
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged -= this.SystemEvents_DisplaySettingsChanged;
        }

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            this.ConnectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
