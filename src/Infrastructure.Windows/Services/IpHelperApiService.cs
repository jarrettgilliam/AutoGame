namespace AutoGame.Infrastructure.Windows.Services;

using System;
using System.Runtime.InteropServices;
using AutoGame.Infrastructure.Windows.Enums;
using AutoGame.Infrastructure.Windows.Interfaces;

internal sealed class IpHelperApiService : IIpHelperApiService
{
    public uint GetExtendedUdpTable(
        IntPtr pUdpTable,
        ref int pdwSize,
        bool sort,
        IpVersion ipVersion,
        UdpTableClass tableClass,
        uint reserved = 0) =>
        NativeMethods.GetExtendedUdpTable(
            pUdpTable,
            ref pdwSize,
            sort,
            ipVersion,
            tableClass,
            reserved);

    private static class NativeMethods
    {
        [DllImport("iphlpapi.dll", SetLastError = true)]
        public static extern uint GetExtendedUdpTable(
            IntPtr pUdpTable,
            ref int pdwSize,
            bool sort,
            IpVersion ipVersion,
            UdpTableClass tableClass,
            uint reserved);
    }
}