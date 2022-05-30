namespace AutoGame.Tests;

using AutoGame.Core.Interfaces;
using AutoGame.ViewModels;
using Microsoft.Extensions.DependencyInjection;

public class AppTests
{
    private static readonly App sut = new();

    [Fact]
    public void ConfigureServices_MainWindowViewModel_IsBuildable()
    {
        ServiceCollection services = new();

        sut.ConfigureServices(services);

        Assert.NotNull(services.BuildServiceProvider().GetService<MainWindowViewModel>());
    }

    [Fact]
    public void ConfigureServices_SoftwareManagers_HasAny()
    {
        ServiceCollection services = new();

        sut.ConfigureServices(services);

        Assert.True(services.BuildServiceProvider().GetService<IEnumerable<ISoftwareManager>>()?.Any());
    }
}