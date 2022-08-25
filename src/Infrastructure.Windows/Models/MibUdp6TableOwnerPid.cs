namespace AutoGame.Infrastructure.Windows.Models;

using System.Runtime.InteropServices;
using AutoGame.Infrastructure.Windows.Interfaces;

[StructLayout(LayoutKind.Sequential)]
internal struct MibUdp6TableOwnerPid : IPortTable<MibUdp6RowOwnerPid>
{
    public uint dwNumEntries;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 1)]
    public MibUdp6RowOwnerPid[] table;

    public uint NumEntries => this.dwNumEntries;
    public MibUdp6RowOwnerPid[] Table => this.table;
}