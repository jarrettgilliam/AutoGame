namespace AutoGame.Infrastructure.Windows.Interfaces;

using System.Net;

public interface IPortRow
{
    IPAddress LocalAddress { get; }
    ushort LocalPort { get; }
    uint ProcessId { get; }
}