namespace AutoGame.Infrastructure.Services;

using System.Runtime.InteropServices;
using AutoGame.Infrastructure.Enums;
using AutoGame.Infrastructure.Interfaces;

internal sealed class IpHelperApiService : IIpHelperApiService
{
    public uint GetExtendedUdpTable(
        IntPtr pUdpTable,
        ref int pdwSize,
        bool sort,
        IpVersion ipVersion,
        UdpTableClass tableClass,
        uint reserved = 0)
    {
        return NativeMethods.GetExtendedUdpTable(
            pUdpTable,
            ref pdwSize,
            sort,
            ipVersion,
            tableClass,
            reserved);
    }

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