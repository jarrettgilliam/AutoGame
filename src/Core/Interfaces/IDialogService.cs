namespace AutoGame.Core.Interfaces;

using AutoGame.Core.Models;

public interface IDialogService
{
    Task<string?> ShowOpenFileDialog(OpenFileDialogParms parms);

    void ShowMessageBox(MessageBoxParms parms);
}