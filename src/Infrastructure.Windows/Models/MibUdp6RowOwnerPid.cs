namespace AutoGame.Infrastructure.Windows.Models;

using System;
using System.Net;
using System.Runtime.InteropServices;
using AutoGame.Infrastructure.Windows.Interfaces;

[StructLayout(LayoutKind.Sequential)]
internal struct MibUdp6RowOwnerPid : IPortRow, IEquatable<MibUdp6RowOwnerPid>
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] localAddr;

    public uint dwLocalScopeId;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] dwLocalPort;

    public uint dwOwningPid;

    public IPAddress LocalAddress => new(this.localAddr, this.dwLocalScopeId);
    public ushort LocalPort => BitConverter.ToUInt16([this.dwLocalPort[1], this.dwLocalPort[0]], 0);
    public uint ProcessId => this.dwOwningPid;

    public bool Equals(MibUdp6RowOwnerPid other) =>
        this.localAddr.Equals(other.localAddr) &&
        this.dwLocalScopeId == other.dwLocalScopeId &&
        this.dwLocalPort.Equals(other.dwLocalPort) &&
        this.dwOwningPid == other.dwOwningPid;

    public override bool Equals(object? obj) =>
        obj is MibUdp6RowOwnerPid other && this.Equals(other);

    public override int GetHashCode() =>
        HashCode.Combine(this.localAddr, this.dwLocalScopeId, this.dwLocalPort, this.dwOwningPid);
}