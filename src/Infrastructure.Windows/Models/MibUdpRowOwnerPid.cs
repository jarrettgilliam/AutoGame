namespace AutoGame.Infrastructure.Windows.Models;

using System;
using System.Net;
using System.Runtime.InteropServices;
using AutoGame.Infrastructure.Windows.Interfaces;

[StructLayout(LayoutKind.Sequential)]
internal struct MibUdpRowOwnerPid : IPortRow, IEquatable<MibUdpRowOwnerPid>
{
    public uint dwLocalAddr;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] dwLocalPort;

    public uint dwOwningPid;

    public IPAddress LocalAddress => new(this.dwLocalAddr);
    public ushort LocalPort => BitConverter.ToUInt16([this.dwLocalPort[1], this.dwLocalPort[0]], 0);
    public uint ProcessId => this.dwOwningPid;

    public bool Equals(MibUdpRowOwnerPid other) =>
        this.dwLocalAddr == other.dwLocalAddr &&
        this.dwLocalPort.Equals(other.dwLocalPort) &&
        this.dwOwningPid == other.dwOwningPid;

    public override bool Equals(object? obj) =>
        obj is MibUdpRowOwnerPid other && this.Equals(other);

    public override int GetHashCode() =>
        HashCode.Combine(this.dwLocalAddr, this.dwLocalPort, this.dwOwningPid);
}