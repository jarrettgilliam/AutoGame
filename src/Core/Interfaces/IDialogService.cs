namespace AutoGame.Core.Interfaces;

using AutoGame.Core.Models;

public interface IDialogService
{
    bool ShowOpenFileDialog(OpenFileDialogParms parms, out string? selectedFileName);

    void ShowMessageBox(MessageBoxParms parms);
}