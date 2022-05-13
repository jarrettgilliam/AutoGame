namespace AutoGame.Controls;

using AutoGame.ViewModels;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;

public class BaseWindow : Window
{
    public static readonly DependencyProperty ShowWindowProperty =
        DependencyProperty.Register(
            nameof(ShowWindow),
            typeof(bool),
            typeof(BaseWindow),
            new FrameworkPropertyMetadata(true, OnShowWindowChanged));

    public static readonly DependencyProperty NotifyIconVisibleProperty =
        DependencyProperty.Register(
            nameof(NotifyIconVisible),
            typeof(bool),
            typeof(BaseWindow),
            new FrameworkPropertyMetadata(false, OnNotifyIconVisibleChanged));

    private NotifyIcon? notifyIcon;

    public BaseWindow()
    {
        this.Loaded += this.MainWindow_Loaded;
    }

    public bool ShowWindow
    {
        get => (bool)this.GetValue(ShowWindowProperty);
        set => this.SetValue(ShowWindowProperty, value);
    }

    public bool NotifyIconVisible
    {
        get => (bool)this.GetValue(NotifyIconVisibleProperty);
        set => this.SetValue(NotifyIconVisibleProperty, value);
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        this.notifyIcon = new NotifyIcon();
        this.notifyIcon.Click += this.NotifyIcon_Click;
        this.notifyIcon.Icon = new Icon(@"Icons\AutoGame.ico");
    }

    private MainWindowViewModel? ViewModel => this.DataContext as MainWindowViewModel;

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        this.Loaded -= this.MainWindow_Loaded;

        if (this.notifyIcon is not null)
        {
            this.notifyIcon.Click -= this.NotifyIcon_Click;
            this.notifyIcon.Dispose();
            this.notifyIcon = null;
        }

        this.ViewModel?.Dispose();
    }

    private static void OnShowWindowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BaseWindow baseWindow && e.NewValue is bool showWindow)
        {
            if (showWindow)
            {
                baseWindow.Show();
            }
            else
            {
                baseWindow.Hide();
            }
        }
    }

    private static void OnNotifyIconVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BaseWindow baseWindow && 
            baseWindow.notifyIcon is not null &&
            e.NewValue is bool notifyIconVisible)
        {
            baseWindow.notifyIcon.Visible = notifyIconVisible;
        }
    }

    private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        this.Loaded -= this.MainWindow_Loaded;
        this.ViewModel?.LoadedCommand.Execute(null);
    }

    private void NotifyIcon_Click(object? sender, EventArgs e)
    {
        this.ViewModel?.NotifyIconClickCommand.Execute(null);
    }
}