namespace AutoGame.Infrastructure.Services;

using AutoGame.Infrastructure.Interfaces;
using AutoGame.Infrastructure.Models;
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
}