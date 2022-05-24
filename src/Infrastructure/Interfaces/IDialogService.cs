namespace AutoGame.Infrastructure.Interfaces;

using AutoGame.Infrastructure.Models;

public interface IDialogService
{
    bool ShowOpenFileDialog(OpenFileDialogParms parms, out string? selectedFileName);
}