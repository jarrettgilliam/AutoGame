namespace AutoGame.Core.Interfaces;

using AutoGame.Core.Models;

public interface INetStatPortsService
{
    IList<Port> GetNetStatPorts();
}