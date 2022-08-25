namespace AutoGame.Views;

using System;
using AutoGame.ViewModels;
using Avalonia;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;

public partial class MainWindow : CoreWindow
{
    public static readonly StyledProperty<bool> ShowWindowProperty =
        AvaloniaProperty.Register<Window, bool>(nameof(ShowWindow), true);

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

    protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ShowWindowProperty && change.NewValue.Value is bool showWindow)
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
        this.ViewModel?.LoadedCommand.Execute(null);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        // Copied from Window.cs
        SizeToContent sizeToContent = this.SizeToContent;
        Size clientSize = this.ClientSize;
        Size constraint = clientSize;
        Size maxAutoSize = this.PlatformImpl?.MaxAutoSizeHint ?? Size.Infinity;

        if (this.MaxWidth > 0 && this.MaxWidth < maxAutoSize.Width)
        {
            maxAutoSize = maxAutoSize.WithWidth(this.MaxWidth);
        }

        if (this.MaxHeight > 0 && this.MaxHeight < maxAutoSize.Height)
        {
            maxAutoSize = maxAutoSize.WithHeight(this.MaxHeight);
        }

        if (sizeToContent.HasAllFlags(SizeToContent.Width))
        {
            constraint = constraint.WithWidth(maxAutoSize.Width);
        }

        if (sizeToContent.HasAllFlags(SizeToContent.Height))
        {
            constraint = constraint.WithHeight(maxAutoSize.Height);
        }

        Size result = base.MeasureOverride(constraint);

        if (!sizeToContent.HasAllFlags(SizeToContent.Width))
        {
            if (!double.IsInfinity(availableSize.Width))
            {
                result = result.WithWidth(availableSize.Width);
            }
            else
            {
                result = result.WithWidth(clientSize.Width);
            }
        }

        if (!sizeToContent.HasAllFlags(SizeToContent.Height))
        {
            if (!double.IsInfinity(availableSize.Height))
            {
                result = result.WithHeight(availableSize.Height);
            }
            else
            {
                result = result.WithHeight(clientSize.Height);
            }
        }

        return result;
    }
}