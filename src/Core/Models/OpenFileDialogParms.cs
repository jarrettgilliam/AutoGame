namespace AutoGame.Core.Models;

using System.Collections.Generic;

public struct OpenFileDialogParms
{
    public string? FileName { get; set; }
    public string? InitialDirectory { get; set; }
    public string? FilterName { get; set; }
    public List<string> FilterPatterns { get; set; }
}