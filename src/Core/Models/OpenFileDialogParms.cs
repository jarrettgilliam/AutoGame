namespace AutoGame.Core.Models;

public struct OpenFileDialogParms
{
    public string? FileName { get; set; }
    public string? InitialDirectory { get; set; }
    public string? Filter { get; set; }
}