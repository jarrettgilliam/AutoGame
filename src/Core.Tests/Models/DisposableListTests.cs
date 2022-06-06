namespace AutoGame.Core.Tests.Models;

using AutoGame.Core.Models;

public class DisposableListTests
{
    private readonly Mock<IDisposable> disposableMock = new();

    [Fact]
    public void Constructor_PassedEnumerable_Matches()
    {
        List<IDisposable> disposables = new() { this.disposableMock.Object };

        var sut = new DisposableList<IDisposable>(disposables);

        Assert.Collection(sut, x => Assert.Equal(x, this.disposableMock.Object));
    }

    [Fact]
    public void Dispose_Works()
    {
        DisposableList<IDisposable> sut = new() { this.disposableMock.Object };

        sut.Dispose();

        this.disposableMock.Verify(x => x.Dispose(), Times.Once);
    }
}