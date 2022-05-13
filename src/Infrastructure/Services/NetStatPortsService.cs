namespace AutoGame.Infrastructure.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using AutoGame.Core.Exceptions;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;

// From: https://gist.github.com/cheynewallace/5971686
public class NetStatPortsService : INetStatPortsService
{
    public IList<Port> GetNetStatPorts()
    {
        var ports = new List<Port>();

        using var process = new Process();
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

        return ports;
    }
}