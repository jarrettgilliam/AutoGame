namespace AutoGame.Infrastructure.Tests.Services;

using AutoGame.Core.Interfaces;
using AutoGame.Infrastructure.Services;

public class SoftwareCollectionTests
{
    private readonly Mock<ISoftwareManager> softwareMock1 = new();
    private readonly Mock<ISoftwareManager> softwareMock2 = new();

    private readonly SoftwareCollection sut;

    public SoftwareCollectionTests()
    {
        this.softwareMock1
            .SetupGet(x => x.Key)
            .Returns(nameof(this.softwareMock1));

        this.softwareMock2
            .SetupGet(x => x.Key)
            .Returns(nameof(this.softwareMock2));

        this.sut = new SoftwareCollection(new[] { this.softwareMock1.Object, this.softwareMock2.Object });
    }

    [Fact]
    public void Constructor_StoresPassedSoftware()
    {
        Assert.Collection(this.sut,
            s => Assert.Equal(s, this.softwareMock1.Object),
            s => Assert.Equal(s, this.softwareMock2.Object));
    }

    [Fact]
    public void GetSoftwareByKeyOrNull_ValidKey_ReturnsMatchingSoftware()
    {
        ISoftwareManager? actual = this.sut.GetSoftwareByKeyOrNull(this.softwareMock2.Object.Key);

        Assert.Equal(this.softwareMock2.Object, actual);
    }

    [Fact]
    public void GetSoftwareByKeyOrNull_InvalidKey_ReturnsNull()
    {
        ISoftwareManager? actual = this.sut.GetSoftwareByKeyOrNull("badKey");

        Assert.Null(actual);
    }
}