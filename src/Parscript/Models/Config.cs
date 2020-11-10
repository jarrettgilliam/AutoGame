using System;
using System.Collections.Generic;
using System.Text;

namespace Parscript.Infrastructure.Models
{
    public sealed class Config
    {
        public List<string> ConnectionChangedDetectionMethods { get; set; } // Polling, ResolutionChange

        public string ConnectionStatusProvider { get; set; } // WebApi, UDPListeners

        public Dictionary<string, string> Parameters { get; set; }

        public string ConnectCommand { get; set; }

        public string DisconnectCommand { get; set; }
    }
}
