namespace AutoGame.Infrastructure.Windows.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using AutoGame.Core.Enums;
using AutoGame.Core.Exceptions;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;
using AutoGame.Infrastructure.Windows.Enums;
using AutoGame.Infrastructure.Windows.Interfaces;
using AutoGame.Infrastructure.Windows.Models;

// https://docs.microsoft.com/en-us/windows/win32/api/Iphlpapi/nf-iphlpapi-getextendedudptable
// http://pinvoke.net/default.aspx/iphlpapi.GetExtendedTcpTable
// http://pinvoke.net/default.aspx/user32/GetExtendedUdpTable.html
internal sealed class WindowsNetStatPortsService : INetStatPortsService
{
    public WindowsNetStatPortsService(IIpHelperApiService ipHelperApiService)
    {
        this.IpHelperApiService = ipHelperApiService;
    }

    private IIpHelperApiService IpHelperApiService { get; }

    public IList<Port> GetUdpPorts()
    {
        IList<MibUdpRowOwnerPid> v4ports = this.GetUdpV4Ports();
        IList<MibUdp6RowOwnerPid> v6ports = this.GetUdpV6Ports();

        List<Port> ports = new(v4ports.Count + v6ports.Count);

        ports.AddRange(v4ports.Select(this.ToPort));
        ports.AddRange(v6ports.Select(this.ToPort));

        return ports;
    }

    private Port ToPort<T>(T r) where T : IPortRow =>
        new(NetworkProtocol.UDP, r.LocalAddress, r.LocalPort, r.ProcessId);

    private IList<MibUdpRowOwnerPid> GetUdpV4Ports() =>
        this.InternalGetPorts<MibUdpTableOwnerPid, MibUdpRowOwnerPid>(IpVersion.AF_INET);

    private IList<MibUdp6RowOwnerPid> GetUdpV6Ports() =>
        this.InternalGetPorts<MibUdp6TableOwnerPid, MibUdp6RowOwnerPid>(IpVersion.AF_INET6);

    private IList<TRow> InternalGetPorts<TTable, TRow>(IpVersion ipVersion)
        where TTable : IPortTable<TRow>
    {
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
            List<TRow> tableRows = new List<TRow>((int)table.NumEntries);

            var rowPtr = (IntPtr)((long)tcpTablePtr + 4);
            for (int i = 0; i < table.NumEntries; i++)
            {
                var tcpRow = (TRow)Marshal.PtrToStructure(rowPtr, typeof(TRow))!;
                tableRows.Add(tcpRow);
                rowPtr = (IntPtr)((long)rowPtr + rowStructSize); // next entry
            }

            return tableRows;
        }
        finally
        {
            // Free the Memory
            Marshal.FreeHGlobal(tcpTablePtr);
        }
    }
}