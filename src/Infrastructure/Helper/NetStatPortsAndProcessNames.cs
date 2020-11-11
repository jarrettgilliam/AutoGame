using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace AutoGame.Infrastructure.Helper
{
    // From: https://gist.github.com/cheynewallace/5971686
    internal static class NetStatPortsAndProcessNames
    {
        public static IList<Port> GetNetStatPorts()
        {
            var Ports = new List<Port>();

            using (Process p = new Process())
            {
                ProcessStartInfo ps = new ProcessStartInfo
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

                p.StartInfo = ps;
                p.Start();

                StreamReader stdOutput = p.StandardOutput;
                StreamReader stdError = p.StandardError;

                string content = stdOutput.ReadToEnd() + stdError.ReadToEnd();
                string exitStatus = p.ExitCode.ToString();

                if (exitStatus != "0")
                {
                    // Command Errored. Handle Here If Need Be
                }

                //Get The Rows
                string[] rows = Regex.Split(content, "\r\n");
                foreach (string row in rows)
                {
                    //Split it baby
                    string[] tokens = Regex.Split(row, "\\s+");
                    if (tokens.Length > 4 && (tokens[1].Equals("UDP") || tokens[1].Equals("TCP")))
                    {
                        string localAddress = Regex.Replace(tokens[2], @"\[(.*?)\]", "1.1.1.1");
                        Ports.Add(new Port
                        {
                            Protocol = localAddress.Contains("1.1.1.1") ? String.Format("{0}v6", tokens[1]) : String.Format("{0}v4", tokens[1]),
                            PortNumber = localAddress.Split(':')[1],
                            ProcessId = tokens[1] == "UDP" ? Convert.ToInt16(tokens[4]) : Convert.ToInt16(tokens[5])
                        });
                    }
                }
            }

            return Ports;
        }

        public static string LookupProcess(int pid)
        {
            string procName;

            try
            {
                procName = Process.GetProcessById(pid).ProcessName;
            }
            catch (Exception)
            {
                procName = "-";
            }

            return procName;
        }

        public class Port
        {
            public string PortNumber { get; set; }

            public string Protocol { get; set; }

            public int ProcessId { get; set; }
        }
    }
}
