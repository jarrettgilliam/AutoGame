namespace AutoGame.Infrastructure.Windows.Models;

using System.Runtime.InteropServices;
using AutoGame.Infrastructure.Windows.Interfaces;

[StructLayout(LayoutKind.Sequential)]
internal struct MibUdpTableOwnerPid : IPortTable<MibUdpRowOwnerPid>
{
    public uint dwNumEntries;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 1)]
    public MibUdpRowOwnerPid[] table;

    public uint NumEntries => this.dwNumEntries;
    public MibUdpRowOwnerPid[] Table => this.table;
}