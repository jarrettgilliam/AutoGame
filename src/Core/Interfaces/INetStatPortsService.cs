namespace AutoGame.Core.Interfaces;

using System.Collections.Generic;
using AutoGame.Core.Models;

public interface INetStatPortsService
{
    IList<Port> GetUdpPorts();
}