using AutoGame.Infrastructure.Models;
using Xunit;

namespace AutoGame.Infrastructure.Tests.Models
{
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
}
