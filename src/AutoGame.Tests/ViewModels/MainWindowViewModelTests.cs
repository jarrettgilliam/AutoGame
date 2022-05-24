namespace AutoGame.Tests.ViewModels;

using System.IO.Abstractions;
using AutoGame.Core.Interfaces;
using AutoGame.Infrastructure.Interfaces;
using AutoGame.ViewModels;

public class MainWindowViewModelTests
{
    private readonly MainWindowViewModel sut;
    
    public MainWindowViewModelTests()
    {
        var loggingService = new Mock<ILoggingService>();
        var configService = new Mock<IConfigService>();
        var autoGameService = new Mock<IAutoGameService>();
        var fileSystem = new Mock<IFileSystem>();
        var dialogService = new Mock<IDialogService>();
        
        this.sut = new MainWindowViewModel(
            loggingService.Object,
            configService.Object,
            autoGameService.Object,
            fileSystem.Object,
            dialogService.Object);
    }
    
    [Fact]
    public void AutoGameService_IsPublic()
    {
        Assert.NotNull(this.sut.AutoGameService);
    }
}