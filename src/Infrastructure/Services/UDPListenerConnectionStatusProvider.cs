using Parscript.Infrastructure.Enums;
using Parscript.Infrastructure.Helper;
using Parscript.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parscript.Infrastructure.Services
{
    public class UDPListenerConnectionStatusProvider : IConnectionStatusProvider, IDisposable
    {
        public void Dispose()
        {
        }

        public Task<ConnectionStatus> ProvideStatus(ConnectionStatus currentStatus)
        {
            Process[] parsecProcs = Process.GetProcessesByName("parsecd");

            IList<NetStatPortsAndProcessNames.Port> ports = NetStatPortsAndProcessNames.GetNetStatPorts();

            ConnectionStatus result = ports.Any(p => this.IsParsecUDPPort(p, parsecProcs))
                ? ConnectionStatus.Connected
                : ConnectionStatus.Disconnected;

            return Task.FromResult(result);
        }

        private bool IsParsecUDPPort(NetStatPortsAndProcessNames.Port port, Process[] parsecProcs)
        {
            if (port?.Protocol?.StartsWith("UDP") != true)
            {
                return false;
            }

            foreach (Process proc in parsecProcs)
            {
                if (proc.Id == port.ProcessId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
