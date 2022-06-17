namespace AutoGame.Infrastructure.Services;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AutoGame.Core.Exceptions;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;
using AutoGame.Infrastructure.Enums;
using AutoGame.Infrastructure.Interfaces;
using AutoGame.Infrastructure.Models;

// https://docs.microsoft.com/en-us/windows/win32/api/Iphlpapi/nf-iphlpapi-getextendedudptable
// http://pinvoke.net/default.aspx/iphlpapi.GetExtendedTcpTable
// http://pinvoke.net/default.aspx/user32/GetExtendedUdpTable.html
internal sealed class NetStatPortsService : INetStatPortsService
{
    public NetStatPortsService(IIpHelperApiService ipHelperApiService)
    {
        this.IpHelperApiService = ipHelperApiService;
    }

    private IIpHelperApiService IpHelperApiService { get; }

    public IList<Port> GetUdpPorts()
    {
        List<Port> ports = new();

        ports.AddRange(this.GetUdpV4Ports().Cast<IPortRow>().Select(this.ToPort));
        ports.AddRange(this.GetUdpV6Ports().Cast<IPortRow>().Select(this.ToPort));

        return ports;
    }

    private Port ToPort(IPortRow r) => new("UDP", r.LocalAddress, r.LocalPort, r.ProcessId);

    private IEnumerable<MibUdpRowOwnerPid> GetUdpV4Ports() =>
        this.InternalGetPorts<MibUdpTableOwnerPid, MibUdpRowOwnerPid>(IpVersion.AF_INET);

    private IEnumerable<MibUdp6RowOwnerPid> GetUdpV6Ports() =>
        this.InternalGetPorts<MibUdp6TableOwnerPid, MibUdp6RowOwnerPid>(IpVersion.AF_INET6);

    private IEnumerable<TRow> InternalGetPorts<TTable, TRow>(IpVersion ipVersion)
        where TTable : IPortTable<TRow>
    {
        List<TRow> tableRows = new();
        int buffSize = 0;

        // how much memory do we need?
        this.IpHelperApiService.GetExtendedUdpTable(
            IntPtr.Zero, ref buffSize, true, ipVersion, UdpTableClass.UDP_TABLE_OWNER_PID);

        IntPtr tcpTablePtr = Marshal.AllocHGlobal(buffSize);

        try
        {
            uint ret = this.IpHelperApiService.GetExtendedUdpTable(
                tcpTablePtr, ref buffSize, true, ipVersion, UdpTableClass.UDP_TABLE_OWNER_PID);

            if (ret != 0)
            {
                throw new NetstatException($"Unable to get load open ports. Return code: {ret}");
            }

            // get the number of entries in the table
            var table = (TTable)Marshal.PtrToStructure(tcpTablePtr, typeof(TTable))!;
            int rowStructSize = Marshal.SizeOf(typeof(TRow));
            uint numEntries = table.NumEntries;

            var rowPtr = (IntPtr)((long)tcpTablePtr + 4);
            for (int i = 0; i < numEntries; i++)
            {
                var tcpRow = (TRow)Marshal.PtrToStructure(rowPtr, typeof(TRow))!;
                tableRows.Add(tcpRow);
                rowPtr = (IntPtr)((long)rowPtr + rowStructSize); // next entry
            }
        }
        finally
        {
            // Free the Memory
            Marshal.FreeHGlobal(tcpTablePtr);
        }

        return tableRows;
    }
}