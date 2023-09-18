namespace AutoGame.Infrastructure.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using Serilog.Events;

internal sealed class DialogService : IDialogService
{
    public async Task<string?> ShowOpenFileDialog(OpenFileDialogParms parms)
    {
        if (Application.Current?.ApplicationLifetime is not
            IClassicDesktopStyleApplicationLifetime { MainWindow.StorageProvider.CanOpen: true } lifetime)
        {
            return null;
        }

        IStorageProvider storageProvider = lifetime.MainWindow.StorageProvider;

        FilePickerOpenOptions options = await this.GetFilePickerOpenOptions(parms, storageProvider);

        IReadOnlyList<IStorageFile> result = await storageProvider.OpenFilePickerAsync(options).ConfigureAwait(false);

        return result.FirstOrDefault()?.Path.LocalPath;
    }

    private async Task<FilePickerOpenOptions> GetFilePickerOpenOptions(OpenFileDialogParms parms, IStorageProvider storageProvider)
    {
        var options = new FilePickerOpenOptions();

        if (parms.InitialDirectory != null)
        {
            options.SuggestedStartLocation =
                await storageProvider.TryGetFolderFromPathAsync(parms.InitialDirectory).ConfigureAwait(false);
        }

        options.FileTypeFilter = new List<FilePickerFileType>
        {
            new(parms.FilterName)
            {
                Patterns = parms.FilterPatterns
            }
        };

        return options;
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
            MaxWidth = 600,
            WindowIcon = new WindowIcon(new Bitmap(AssetLoader.Open(new Uri("avares://AutoGame/Assets/AutoGame.ico"))))
        };

        _ = MessageBoxManager.GetMessageBoxStandard(stdParms).ShowWindowAsync();
    }

    private Icon GetIconForLogLevel(LogEventLevel level) =>
        level switch
        {
            < LogEventLevel.Warning => Icon.Info,
            LogEventLevel.Warning => Icon.Warning,
            > LogEventLevel.Warning => Icon.Error
        };
}