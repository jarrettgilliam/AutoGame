namespace AutoGame.Infrastructure.Models;

using System.Runtime.InteropServices;
using AutoGame.Infrastructure.Interfaces;

[StructLayout(LayoutKind.Sequential)]
internal struct MibUdpTableOwnerPid : IPortTable<MibUdpRowOwnerPid>
{
    public uint dwNumEntries;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 1)]
    public MibUdpRowOwnerPid[] table;

    public uint NumEntries => this.dwNumEntries;
    public MibUdpRowOwnerPid[] Table => this.table;
}