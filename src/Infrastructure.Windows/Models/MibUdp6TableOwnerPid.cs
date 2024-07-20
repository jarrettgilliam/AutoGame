namespace AutoGame.Infrastructure.Windows.Models;

using System;
using System.Runtime.InteropServices;
using AutoGame.Infrastructure.Windows.Interfaces;

[StructLayout(LayoutKind.Sequential)]
internal struct MibUdp6TableOwnerPid : IPortTable<MibUdp6RowOwnerPid>, IEquatable<MibUdp6TableOwnerPid>
{
    public uint dwNumEntries;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 1)]
    public MibUdp6RowOwnerPid[] table;

    public uint NumEntries => this.dwNumEntries;
    public MibUdp6RowOwnerPid[] Table => this.table;

    public bool Equals(MibUdp6TableOwnerPid other) =>
        this.dwNumEntries == other.dwNumEntries &&
        this.table.Equals(other.table);

    public override bool Equals(object? obj) =>
        obj is MibUdp6TableOwnerPid other && this.Equals(other);

    public override int GetHashCode() =>
        HashCode.Combine(this.dwNumEntries, this.table);
}