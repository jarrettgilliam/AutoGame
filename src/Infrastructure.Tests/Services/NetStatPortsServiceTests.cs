namespace AutoGame.Infrastructure.Tests.Services;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using AutoGame.Core.Exceptions;
using AutoGame.Core.Models;
using AutoGame.Infrastructure.Enums;
using AutoGame.Infrastructure.Interfaces;
using AutoGame.Infrastructure.Models;
using AutoGame.Infrastructure.Services;

public class NetStatPortsServiceTests
{
    private readonly NetStatPortsService sut;
    private readonly Mock<IIpHelperApiService> ipHelperApiServiceMock = new();

    private readonly MibUdpTableOwnerPid UdpPortsIPv4 = new()
    {
        dwNumEntries = 1,
        table = new[]
        {
            new MibUdpRowOwnerPid
            {
                dwLocalAddr = 16777343, // 127.0.0.1
                dwLocalPort = new byte[] { 7, 108, 0, 0 }, // 1900
                dwOwningPid = 4580
            }
        }
    };

    private readonly MibUdp6TableOwnerPid UdpPortsIPv6 = new()
    {
        dwNumEntries = 1,
        table = new[]
        {
            new MibUdp6RowOwnerPid
            {
                localAddr = new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 }, // ::1
                dwLocalScopeId = 0,
                dwLocalPort = new byte[] { 7, 108, 0, 0 }, // 1900
                dwOwningPid = 4580
            }
        }
    };

    private uint getExtendedUdpTableReturnValue = 0;

    public NetStatPortsServiceTests()
    {
        this.ipHelperApiServiceMock
            .Setup(x => x.GetExtendedUdpTable(
                It.IsAny<IntPtr>(),
                ref It.Ref<int>.IsAny,
                It.IsAny<bool>(),
                It.IsAny<IpVersion>(),
                It.IsAny<UdpTableClass>(),
                It.IsAny<uint>()))
            .Callback(this.GetExtendedUdpTableCallback)
            .Returns(() => this.getExtendedUdpTableReturnValue);

        this.sut = new NetStatPortsService(
            this.ipHelperApiServiceMock.Object);
    }

    [Fact]
    public void GetUdpPorts_GetsIpV4Ports()
    {
        IList<Port> ports = this.sut.GetUdpPorts();

        Port port = ports[0];
        
        Assert.Equal(AddressFamily.InterNetwork, port.LocalAddress.AddressFamily);
        Assert.Equal(IPAddress.Parse("127.0.0.1"), port.LocalAddress);
        Assert.Equal(1900u, port.LocalPort);
        Assert.Equal(4580u, port.ProcessId);
    }

    [Fact]
    public void GetUdpPorts_GetsIpV6Ports()
    {
        IList<Port> ports = this.sut.GetUdpPorts();

        Port port = ports[1];
        
        Assert.Equal(AddressFamily.InterNetworkV6, port.LocalAddress.AddressFamily);
        Assert.Equal(IPAddress.Parse("::1"), port.LocalAddress);
        Assert.Equal(1900u, port.LocalPort);
        Assert.Equal(4580u, port.ProcessId);
    }

    [Fact]
    public void GetUdpPorts_Success_ReturnsAllMockedPorts()
    {
        IList<Port> ports = this.sut.GetUdpPorts();
        Assert.Equal(2, ports.Count);
    }

    [Fact]
    public void GetUdpPorts_Failure_ThrowsNetstatException()
    {
        this.getExtendedUdpTableReturnValue = 1;
        Assert.Throws<NetstatException>(() => this.sut.GetUdpPorts());
    }

    private void GetExtendedUdpTableCallback(
        IntPtr pUdpTable,
        ref int pdwSize,
        bool sort,
        IpVersion ipVersion,
        UdpTableClass tableClass,
        uint reserved)
    {
        if (tableClass != UdpTableClass.UDP_TABLE_OWNER_PID)
        {
            throw new NotSupportedException();
        }
        
        if (ipVersion == IpVersion.AF_INET)
        {
            pdwSize = Marshal.SizeOf(this.UdpPortsIPv4);

            if (pUdpTable != IntPtr.Zero)
            {
                Marshal.StructureToPtr(this.UdpPortsIPv4, pUdpTable, false);
            }
        }
        else
        {
            pdwSize = Marshal.SizeOf(this.UdpPortsIPv6);

            if (pUdpTable != IntPtr.Zero)
            {
                Marshal.StructureToPtr(this.UdpPortsIPv6, pUdpTable, false);
            }
        }
    }
}