namespace AutoGame.Infrastructure.Windows.Models;

using System;
using System.Net;
using System.Runtime.InteropServices;
using AutoGame.Infrastructure.Windows.Interfaces;

[StructLayout(LayoutKind.Sequential)]
internal struct MibUdp6RowOwnerPid : IPortRow
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] localAddr;

    public uint dwLocalScopeId;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] dwLocalPort;

    public uint dwOwningPid;

    public IPAddress LocalAddress => new(this.localAddr, this.dwLocalScopeId);
    public ushort LocalPort => BitConverter.ToUInt16(new byte[] { this.dwLocalPort[1], this.dwLocalPort[0] }, 0);
    public uint ProcessId => this.dwOwningPid;
}