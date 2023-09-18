namespace AutoGame.Views;

using System;
using AutoGame.ViewModels;
using Avalonia;
using Avalonia.Threading;
using FluentAvalonia.UI.Windowing;

public partial class MainWindow : AppWindow
{
    public static readonly StyledProperty<bool> ShowWindowProperty =
        AvaloniaProperty.Register<MainWindow, bool>(nameof(ShowWindow), true);

    public MainWindow()
    {
        this.Opened += this.OnOpened;
        this.InitializeComponent();
    }

    public MainWindow(MainWindowViewModel viewModel) : this()
    {
        this.DataContext = viewModel;
    }

    public bool ShowWindow
    {
        get => this.GetValue(ShowWindowProperty);
        set => this.SetValue(ShowWindowProperty, value);
    }

    private MainWindowViewModel? ViewModel => this.DataContext as MainWindowViewModel;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ShowWindowProperty && change.NewValue is bool showWindow)
        {
            if (showWindow)
            {
                this.Show();
            }
            else
            {
                this.Hide();
            }
        }
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        this.Opened -= this.OnOpened;

        this.SetOrHideCustomTitleBar();

        // If `LoadedCommand` is called immediately in macOS, the app crashes.
        // If `LoadedCommand` is called on application idle in Windows, the UI doesn't render correctly
        if (this.IsWindows)
        {
            this.ViewModel?.LoadedCommand.Execute(null);
        }
        else
        {
            Dispatcher.UIThread.Post(
                () => this.ViewModel?.LoadedCommand.Execute(null),
                DispatcherPriority.ApplicationIdle);
        }
    }

    /// <summary>
    /// Sets the custom title bar on Windows, hides it for other OS's. The custom title bar
    /// adds an icon to the top left of the application.
    /// </summary>
    private void SetOrHideCustomTitleBar()
    {
        if (this.IsWindows)
        {
            this.TitleBar.ExtendsContentIntoTitleBar = true;
        }
        else
        {
            this.TitleBarHost.IsVisible = false;
        }
    }
}