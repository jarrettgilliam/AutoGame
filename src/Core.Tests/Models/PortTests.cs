namespace AutoGame.Core.Tests.Models;

using AutoGame.Core.Models;

public class PortTests
{
    [Theory]
    [InlineData((string)null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Active Connections")]
    [InlineData("  Proto  Local Address          Foreign Address        State           PID")]
    public void TryParse_InvalidValue_ReturnFalse(string s)
    {
        Assert.False(Port.TryParse(s, out _));
    }

    [Theory]
    [InlineData("  TCP    0.0.0.0:135            0.0.0.0:0              LISTENING       1304")]
    [InlineData("TCP    127.0.0.1:1120         127.0.0.1:52095        TIME_WAIT       0")]
    [InlineData("  TCP    127.0.0.1:3213         0.0.0.0:0              LISTENING       6208")]
    [InlineData("  TCP    [::]:135               [::]:0                 LISTENING       1304")]
    [InlineData("  TCP    [fe80::a8c2:3d64:a5e6:878f%21]:22000  [fe80::f3c7:bff:2e5:c024%21]:22000  ESTABLISHED     6300")]
    [InlineData("  UDP    0.0.0.0:3389           *:*                                    1640")]
    [InlineData("  UDP    [::]:3389              *:*                                    1640")]
    [InlineData("  UDP    [fe80::a8c2:3d64:a5e6:878f%21]:1900  *:*                                    3852")]
    public void TryParse_ValidValue_ReturnTrue(string s)
    {
        Assert.True(Port.TryParse(s, out _));
    }
}