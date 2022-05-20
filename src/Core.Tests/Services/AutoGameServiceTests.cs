namespace AutoGame.Core.Tests.Services;

using System.IO.Abstractions;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Services;

public class AutoGameServiceTests
{
    private Mock<ISoftwareManager> software0;
    private Mock<ISoftwareManager> software1;
    private IAutoGameService sut;

    public AutoGameServiceTests()
    {
        var loggingService = new Mock<ILoggingService>();

        this.software0 = new Mock<ISoftwareManager>();
        this.software0.SetupGet(x => x.Key).Returns("key1");

        this.software1 = new Mock<ISoftwareManager>();
        this.software1.SetupGet(x => x.Key).Returns("key2");

        var fileSystem = new Mock<IFileSystem>();

        var condition1 = new Mock<ILaunchCondition>();
        var condition2 = new Mock<ILaunchCondition>();

        this.sut = new AutoGameService(
            loggingService.Object,
            fileSystem.Object,
            new ISoftwareManager[]
            {
                this.software0.Object, this.software1.Object
            },
            condition1.Object,
            condition2.Object);
    }

    [Fact]
    public void AvailableSoftware_PassedList_Matches()
    {
        Assert.Equal(2, this.sut.AvailableSoftware.Count);
        Assert.Equal(this.software0.Object, this.sut.AvailableSoftware[0]);
        Assert.Equal(this.software1.Object, this.sut.AvailableSoftware[1]);
    }

    [Fact]
    public void CreateDefaultConfiguration_NotNull()
    {
        Assert.NotNull(this.sut.CreateDefaultConfiguration());
    }

    [Fact]
    public void GetSoftwareByKey_ValidKey_MatchingSoftwareManager()
    {
        ISoftwareManager actual = this.sut.GetSoftwareByKeyOrNull(this.software0.Object.Key);

        Assert.Equal(this.software0.Object, actual);
    }

    [Fact]
    public void GetSoftwareByKey_InvalidKey_FirstSoftwareManager()
    {
        ISoftwareManager actual = this.sut.GetSoftwareByKeyOrNull("badKey");

        Assert.Equal(this.software0.Object, actual);
    }
}