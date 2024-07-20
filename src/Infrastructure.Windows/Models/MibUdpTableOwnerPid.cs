namespace AutoGame.Infrastructure.Windows.Models;

using System;
using System.Runtime.InteropServices;
using AutoGame.Infrastructure.Windows.Interfaces;

[StructLayout(LayoutKind.Sequential)]
internal struct MibUdpTableOwnerPid : IPortTable<MibUdpRowOwnerPid>, IEquatable<MibUdpTableOwnerPid>
{
    public uint dwNumEntries;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 1)]
    public MibUdpRowOwnerPid[] table;

    public uint NumEntries => this.dwNumEntries;
    public MibUdpRowOwnerPid[] Table => this.table;

    public bool Equals(MibUdpTableOwnerPid other) =>
        this.dwNumEntries == other.dwNumEntries &&
        this.table.Equals(other.table);

    public override bool Equals(object? obj) =>
        obj is MibUdpTableOwnerPid other && this.Equals(other);

    public override int GetHashCode() =>
        HashCode.Combine(this.dwNumEntries, this.table);
}