namespace AutoGame.Infrastructure.macOS.Tests.Services;

using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using AutoGame.Core.Exceptions;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;
using AutoGame.Infrastructure.macOS.Services;

public class NetStatPortsServiceTests
{
    private readonly NetStatPortsService sut;
    private readonly Mock<IProcessService> processServiceMock = new();
    private readonly Mock<IProcess> processMock = new();

    private readonly StringBuilder standardOutputMock = new();
    private readonly StringBuilder standardErrorMock = new();

    private ProcessStartInfo? startInfo;

    public NetStatPortsServiceTests()
    {
        this.processServiceMock
            .Setup(x => x.NewProcess())
            .Returns(this.processMock.Object);

        this.processMock
            .SetupSet(x => x.StartInfo = It.IsAny<ProcessStartInfo>())
            .Callback<ProcessStartInfo>(i => this.startInfo = i);

        this.processMock
            .SetupGet(x => x.StandardOutput)
            .Returns(() =>
                new StreamReader(
                    new MemoryStream(
                        Encoding.UTF8.GetBytes(
                            this.standardOutputMock.ToString()))));

        this.processMock
            .SetupGet(x => x.StandardError)
            .Returns(() =>
                new StreamReader(
                    new MemoryStream(
                        Encoding.UTF8.GetBytes(
                            this.standardErrorMock.ToString()))));

        this.sut = new NetStatPortsService(this.processServiceMock.Object);
    }

    [Fact]
    public void GetNetStatPorts_StartsProcessCorrectly()
    {
        this.sut.GetUdpPorts();

        Assert.Equal("netstat", this.startInfo?.FileName);
        Assert.Equal("-p udp -anv", this.startInfo?.Arguments);
        Assert.False(this.startInfo?.UseShellExecute);
        Assert.True(this.startInfo?.CreateNoWindow);
        Assert.Equal(ProcessWindowStyle.Hidden, this.startInfo?.WindowStyle);
        Assert.True(this.startInfo?.RedirectStandardInput);
        Assert.True(this.startInfo?.RedirectStandardOutput);
        Assert.True(this.startInfo?.RedirectStandardError);

        this.processMock.Verify(x => x.Start(), Times.Once);
    }

    [Fact]
    public void GetNetStatPorts_Failure_ThrowsNetstatException()
    {
        this.standardErrorMock.AppendLine("This is some error text");
        this.processMock.SetupGet(x => x.ExitCode).Returns(1);

        var exception = Assert.Throws<NetstatException>(
            () => this.sut.GetUdpPorts());

        Assert.Equal(this.standardErrorMock.ToString(), exception.Message);
    }

    [Fact]
    public void GetUdpPorts_Success_ReturnsPorts()
    {
        Port inputPort = new()
        {
            Protocol = "UDP",
            LocalAddress = IPAddress.Parse("127.0.0.1"),
            LocalPort = 1120,
            ProcessId = 9999
        };

        this.standardOutputMock.AppendLine(
            $"{inputPort.Protocol.ToLower()} 0 0 {inputPort.LocalAddress}.{inputPort.LocalPort} *.* 0 0 {inputPort.ProcessId}");

        this.standardOutputMock.AppendLine("This is not parsable");

        var ports = this.sut.GetUdpPorts();

        Assert.Collection(ports, p => Assert.Equal(p, inputPort));
    }

    [Theory]
    //           Proto Recv-Q Send-Q  Local Address          Foreign Address        (state)     rhiwat shiwat    pid   epid  state    options
    [InlineData("udp4       0      0  *.52272                *.*                                786896   9216  12525      0 0x0000 0x00000004")]
    [InlineData("udp4       0      0  *.54397                *.*                                786896   9216  12525      0 0x0000 0x00000004")]
    [InlineData("udp46      0      0  *.21424                *.*                                5242880 5242880  12525      0 0x0100 0x00000000")]
    public void CanParsePorts(string line)
    {
        this.standardOutputMock.AppendLine(line);
        var ports = this.sut.GetUdpPorts();
        Assert.Equal(1, ports.Count);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("udp46")]
    public void CannotParsePorts(string line)
    {
        this.standardOutputMock.AppendLine(line);
        var ports = this.sut.GetUdpPorts();
        Assert.Equal(0, ports.Count);
    }
}