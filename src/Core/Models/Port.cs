namespace AutoGame.Core.Models;

using System.Net;
using AutoGame.Core.Enums;

public readonly record struct Port(
    NetworkProtocol Protocol,
    IPAddress LocalAddress,
    uint LocalPort,
    uint ProcessId);