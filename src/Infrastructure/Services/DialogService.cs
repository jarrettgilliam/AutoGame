namespace AutoGame.Infrastructure.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoGame.Core.Enums;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using FluentAvalonia.Styling;
using MessageBox.Avalonia.BaseWindows.Base;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using MessageBox.Avalonia.ViewModels;
using MessageBox.Avalonia.Views;

internal sealed class DialogService : IDialogService
{
    public async Task<string?> ShowOpenFileDialog(OpenFileDialogParms parms)
    {
        var dialog = new OpenFileDialog
        {
            InitialFileName = parms.FileName,
            Directory = parms.InitialDirectory,
            Filters = new List<FileDialogFilter>
            {
                new()
                {
                    Name = parms.FilterName,
                    Extensions = parms.FilterExtensions
                }
            }
        };

        if (!(Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime))
        {
            return null;
        }

        string[]? selectedFiles = await dialog.ShowAsync(lifetime.MainWindow).ConfigureAwait(false);

        return selectedFiles?.FirstOrDefault();
    }

    public void ShowMessageBox(MessageBoxParms parms)
    {
        var stdParms = new MessageBoxStandardParams
        {
            ContentMessage = parms.Message,
            ContentTitle = parms.Title,
            ButtonDefinitions = ButtonEnum.Ok,
            Icon = this.GetIconForLogLevel(parms.Icon),
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            MaxWidth = 600
        };

        if (AvaloniaLocator.Current.GetService<IAssetLoader>() is { } assets)
        {
            stdParms.WindowIcon = new WindowIcon(
                new Bitmap(assets.Open(new Uri(@"avares://AutoGame/Assets/AutoGame.ico"))));
        }

        _ = this.GetMessageBoxStandardWindow(stdParms).Show();
    }

    private Icon GetIconForLogLevel(LogLevel level) =>
        level switch
        {
            LogLevel.Trace => Icon.Info,
            LogLevel.Error => Icon.Error,
            _ => default
        };

    // Copied from MessageBoxManager and added a call to ForceWin32WindowToTheme
    private IMsBoxWindow<ButtonResult> GetMessageBoxStandardWindow(MessageBoxStandardParams @params)
    {
        var boxStandardWindow = new MsBoxStandardWindow();

        if (AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>() is { } theme)
        {
            theme.ForceWin32WindowToTheme(boxStandardWindow);
        }

        boxStandardWindow.DataContext = new MsBoxStandardViewModel(@params, boxStandardWindow);
        return new MsBoxWindowBase<MsBoxStandardWindow, ButtonResult>(boxStandardWindow);
    }
}