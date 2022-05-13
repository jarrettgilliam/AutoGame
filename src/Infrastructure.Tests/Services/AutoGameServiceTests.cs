namespace AutoGame.Infrastructure.Tests.Services;

using System.IO.Abstractions;
using AutoGame.Core.Interfaces;
using Moq;
using Xunit;
using AutoGame.Core.Services;

public class AutoGameServiceTests
{
    [Fact]
    public void AvailableSoftware_PassedList_Matches()
    {
        this.ArrangeAutoGameService(
            out var software0,
            out var software1,
            out var autoGameService);

        Assert.Equal(2, autoGameService.AvailableSoftware.Count);
        Assert.Equal(software0.Object, autoGameService.AvailableSoftware[0]);
        Assert.Equal(software1.Object, autoGameService.AvailableSoftware[1]);
    }

    [Fact]
    public void CreateDefaultConfiguration_NotNull()
    {
        this.ArrangeAutoGameService(out _, out _, out var autoGameService);
        Assert.NotNull(autoGameService.CreateDefaultConfiguration());
    }

    [Fact]
    public void GetSoftwareByKey_ValidKey_MatchingSoftwareManager()
    {
        this.ArrangeAutoGameService(
            out var expected,
            out _,
            out var autoGameService);

        ISoftwareManager actual = autoGameService.GetSoftwareByKeyOrNull(expected.Object.Key);

        Assert.Equal(expected.Object, actual);
    }

    [Fact]
    public void GetSoftwareByKey_InvalidKey_FirstSoftwareManager()
    {
        this.ArrangeAutoGameService(
            out var expected,
            out _,
            out var autoGameService);

        ISoftwareManager actual = autoGameService.GetSoftwareByKeyOrNull("badKey");

        Assert.Equal(expected.Object, actual);
    }

    private void ArrangeAutoGameService(
        out Mock<ISoftwareManager> software1,
        out Mock<ISoftwareManager> software2,
        out IAutoGameService autoGameService)
    {
        var loggingService = new Mock<ILoggingService>();

        software1 = new Mock<ISoftwareManager>();
        software1.SetupGet(x => x.Key).Returns("key1");

        software2 = new Mock<ISoftwareManager>();
        software2.SetupGet(x => x.Key).Returns("key2");

        var fileSystem = new Mock<IFileSystem>();
        
        var condition1 = new Mock<ILaunchCondition>();
        var condition2 = new Mock<ILaunchCondition>();

        autoGameService = new AutoGameService(
            loggingService.Object,
            fileSystem.Object,
            new ISoftwareManager[] {
                software1.Object,
                software2.Object
            },
            condition1.Object,
            condition2.Object);
    }
}