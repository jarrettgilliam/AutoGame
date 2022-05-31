namespace AutoGame.Infrastructure.Services;

using System.Windows;
using AutoGame.Core.Enums;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;
using Microsoft.Win32;

internal sealed class DialogService : IDialogService
{
    public bool ShowOpenFileDialog(OpenFileDialogParms parms, out string? selectedFileName)
    {
        var dialog = new OpenFileDialog
        {
            FileName = parms.FileName,
            InitialDirectory = parms.InitialDirectory,
            Filter = parms.Filter
        };
        
        bool result = dialog.ShowDialog() ?? false;

        selectedFileName = result ? dialog.FileName : null;
        
        return result;
    }

    public void ShowMessageBox(MessageBoxParms parms) =>
        MessageBox.Show(
            parms.Message,
            parms.Title,
            MessageBoxButton.OK,
            this.GetMessageBoxImageForLogLevel(parms.Icon));

    private MessageBoxImage GetMessageBoxImageForLogLevel(LogLevel level) =>
        level switch
        {
            LogLevel.Trace => MessageBoxImage.Information,
            LogLevel.Error => MessageBoxImage.Error,
            _ => default
        };
}