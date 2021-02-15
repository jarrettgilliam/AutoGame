using AutoGame.Infrastructure.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace AutoGame.Infrastructure.Helper
{
    // From: https://gist.github.com/cheynewallace/5971686
    internal static class NetStatPorts
    {
        public static IList<Port> GetNetStatPorts()
        {
            var ports = new List<Port>();

            using (var process = new Process())
            {
                var ps = new ProcessStartInfo()
                {
                    Arguments = "-a -n -o",
                    FileName = "netstat.exe",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                process.StartInfo = ps;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string errorText = process.StandardError.ReadToEnd();

                if (process.ExitCode != 0)
                {
                    throw new NetstatException(string.IsNullOrEmpty(errorText)
                        ? "An unknown exception occurred while calling netstat.exe"
                        : errorText);
                }

                foreach (string row in Regex.Split(output, Environment.NewLine))
                {
                    if (Port.TryParse(row, out Port port))
                    {
                        ports.Add(port);
                    }
                }
            }

            return ports;
        }

        public class Port
        {
            public string Protocol;

            public string LocalAddress;

            public string ForeignAddress;

            public string State;

            public int ProcessId;

            public static bool TryParse(string s, out Port port)
            {
                var tokens = new Stack<string>(Regex.Split(s, "\\s+").Reverse());

                if (string.IsNullOrEmpty(tokens.Peek()))
                {
                    tokens.Pop();
                }

                if (tokens.Count < 1)
                {
                    port = null;
                    return false;
                }

                port = new Port();

                port.Protocol = tokens.Pop();

                if (port.Protocol == "TCP")
                {
                    if (tokens.Count < 4)
                    {
                        port = null;
                        return false;
                    }
                }
                else if (port.Protocol == "UDP")
                {
                    if (tokens.Count < 3)
                    {
                        port = null;
                        return false;
                    }
                }
                else
                {
                    port = null;
                    return false;
                }

                port.LocalAddress = tokens.Pop();
                port.ForeignAddress = tokens.Pop();

                if (port.Protocol != "UDP")
                {
                    port.State = tokens.Pop();
                }

                if (!int.TryParse(tokens.Pop(), out port.ProcessId))
                {
                    port = null;
                    return false;
                }

                return true;
            }
        }
    }
}
