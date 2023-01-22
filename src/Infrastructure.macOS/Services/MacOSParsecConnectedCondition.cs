namespace AutoGame.Infrastructure.macOS.Services;

using System.IO.Abstractions;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;
using AutoGame.Infrastructure.LaunchConditions;
using Serilog;

internal sealed class MacOSParsecConnectedCondition : ParsecConnectedCondition
{
    public MacOSParsecConnectedCondition(
        ILogger logger,
        INetStatPortsService netStatPortsService,
        IProcessService processService,
        IFileSystem fileSystem,
        IAppInfoService appInfoService)
        : base(logger, netStatPortsService, processService, fileSystem, appInfoService)
    {
    }

    protected override bool HasCorrectNumberOfActiveUdpPorts(Port[] parsecPorts) =>
        parsecPorts.Length == 1;
}