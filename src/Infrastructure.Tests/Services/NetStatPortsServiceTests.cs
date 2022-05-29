namespace AutoGame.Infrastructure.Tests.Services;

using System.Diagnostics;
using System.IO;
using System.Text;
using AutoGame.Core.Exceptions;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;
using AutoGame.Infrastructure.Services;

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
        this.sut.GetNetStatPorts();
        
        Assert.Equal("netstat.exe", this.startInfo!.FileName);
        Assert.Equal("-a -n -o", this.startInfo.Arguments);
        Assert.False(this.startInfo.UseShellExecute);
        Assert.True(this.startInfo.CreateNoWindow);
        Assert.Equal(ProcessWindowStyle.Hidden, this.startInfo.WindowStyle);
        Assert.True(this.startInfo.RedirectStandardInput);
        Assert.True(this.startInfo.RedirectStandardOutput);
        Assert.True(this.startInfo.RedirectStandardError);
        
        this.processMock.Verify(x => x.Start(), Times.Once);
    }

    [Fact]
    public void GetNetStatPorts_Failure_ThrowsNetstatException()
    {
        this.standardErrorMock.AppendLine("This is some error text");
        this.processMock.SetupGet(x => x.ExitCode).Returns(1);

        var exception = Assert.Throws<NetstatException>(
            () => this.sut.GetNetStatPorts());
        
        Assert.Equal(this.standardErrorMock.ToString(), exception.Message);
    }

    [Fact]
    public void GetNetStatPorts_Success_ReturnsPorts()
    {
        Port inputPort = new()
        {
            Protocol = "TCP",
            LocalAddress = "127.0.0.1:1120",
            ForeignAddress = "127.0.0.1:52095",
            State = "TIME_WAIT",
            ProcessId = 9999
        };

        this.standardOutputMock.AppendLine(
            $"{inputPort.Protocol} {inputPort.LocalAddress} {inputPort.ForeignAddress} {inputPort.State} {inputPort.ProcessId}");

        this.standardOutputMock.AppendLine("This is not parsable");
        
        var ports = this.sut.GetNetStatPorts();
        
        Assert.Collection(ports, p => Assert.Equal(p, inputPort));
    }
}