namespace AutoGame.Infrastructure.macOS.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.RegularExpressions;
using AutoGame.Core.Exceptions;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;

internal sealed class NetStatPortsService : INetStatPortsService
{
    private static readonly Regex netstatRegex =
        new(@"^(?<protocol>udp\S*)(\s+\S+){2}\s+(?<ip>\S+)\.(?<port>\S+)(\s+\S+){3}\s+(?<pid>\S+)");

    public NetStatPortsService(IProcessService processService)
    {
        this.ProcessService = processService;
    }

    private IProcessService ProcessService { get; }

    public IList<Port> GetUdpPorts()
    {
        var ports = new List<Port>();

        using IProcess process = this.ProcessService.NewProcess();

        process.StartInfo = new ProcessStartInfo
        {
            FileName = "netstat",
            Arguments = "-p udp -anv",
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        string errorText = process.StandardError.ReadToEnd();
        
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new NetstatException(string.IsNullOrEmpty(errorText)
                ? "An unknown error occurred while calling netstat"
                : errorText);
        }

        foreach (string row in output.Split(Environment.NewLine))
        {
            if (this.TryParsePort(row, out Port port))
            {
                ports.Add(port);
            }
        }

        return ports;
    }

    private bool TryParsePort(string s, out Port port)
    {
        var match = netstatRegex.Match(s);

        if (match.Success &&
            this.TryParseIpAddress(match.Groups["ip"].Value, out IPAddress? localAddress) &&
            uint.TryParse(match.Groups["port"].Value, out uint localPort) &&
            uint.TryParse(match.Groups["pid"].Value, out uint processId))
        {
            port = new Port(
                "UDP",
                localAddress,
                localPort,
                processId);

            return true;
        }

        port = default;
        return false;
    }
    
    private bool TryParseIpAddress(string s, [NotNullWhen(true)] out IPAddress? ipAddress)
    {
        if (s == "*")
        {
            s = "0.0.0.0";
        }

        return IPAddress.TryParse(s, out ipAddress);
    }
}