namespace AutoGame.Core.Models;

using System.Net;

public readonly record struct Port(
    string Protocol,
    IPAddress LocalAddress,
    uint LocalPort,
    uint ProcessId);