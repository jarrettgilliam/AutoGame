namespace AutoGame.Infrastructure.Windows.Models;

using System;
using System.Net;
using System.Runtime.InteropServices;
using AutoGame.Infrastructure.Windows.Interfaces;

[StructLayout(LayoutKind.Sequential)]
internal struct MibUdpRowOwnerPid : IPortRow
{
    public uint dwLocalAddr;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] dwLocalPort;

    public uint dwOwningPid;

    public IPAddress LocalAddress => new(this.dwLocalAddr);
    public ushort LocalPort => BitConverter.ToUInt16(new byte[] { this.dwLocalPort[1], this.dwLocalPort[0] }, 0);
    public uint ProcessId => this.dwOwningPid;
}