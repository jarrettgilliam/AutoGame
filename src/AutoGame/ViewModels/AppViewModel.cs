namespace AutoGame.ViewModels;

using AutoGame.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class AppViewModel : ObservableObject
{
    private IClassicDesktopStyleApplicationLifetime? Lifetime =>
        Application.Current?.ApplicationLifetime
            as IClassicDesktopStyleApplicationLifetime;

    [RelayCommand]
    private void Exit() => this.Lifetime?.Shutdown();

    [RelayCommand]
    private void ToggleWindow()
    {
        if (this.Lifetime?.MainWindow is MainWindow window)
        {
            window.ShowWindow = !window.ShowWindow;
        }
    }
}