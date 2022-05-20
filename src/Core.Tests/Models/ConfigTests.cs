namespace AutoGame.Core.Tests.Models;

using AutoGame.Core.Models;

public class ConfigTests
{
    [Fact]
    public void SetIsDirty_ChangeAProperty_IsDirtyShouldBeTrue()
    {
        Config config = new Config();

        Assert.False(config.IsDirty);
        config.SoftwarePath = "New software path";
        Assert.True(config.IsDirty);
    }
}