namespace AutoGame.Infrastructure.Windows.Interfaces;

using System;
using AutoGame.Infrastructure.Windows.Enums;

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