namespace AutoGame.Infrastructure.Interfaces;

using AutoGame.Infrastructure.Enums;

public interface IIpHelperApiService
{
    uint GetExtendedUdpTable(
        IntPtr pUdpTable,
        ref int pdwSize,
        bool sort,
        IpVersion ipVersion,
        UdpTableClass tableClass,
        uint reserved = 0);
}