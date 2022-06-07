namespace AutoGame.IntegrationTests;

using AutoGame.Core.Interfaces;
using AutoGame.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using OpenTK.Windowing.Desktop;

public class AppTests
{
    private static readonly App sut = new();

    static AppTests()
    {
        GLFWProvider.CheckForMainThread = false;
    }

    [Fact]
    public void ConfigureServices_MainWindowViewModel_IsBuildable()
    {
        Assert.NotNull(sut.serviceProvider.GetService<MainWindowViewModel>());
    }

    [Fact]
    public void ConfigureServices_SoftwareManagers_HasAny()
    {
        Assert.True(sut.serviceProvider.GetService<IEnumerable<ISoftwareManager>>()?.Any());
    }
}