namespace AutoGame.Core.Models;

public struct OpenFileDialogParms
{
    public string? FileName { get; set; }
    public string? InitialDirectory { get; set; }
    public string? FilterName { get; set; }
    public List<string> FilterExtensions { get; set; }
}