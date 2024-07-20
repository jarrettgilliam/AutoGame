namespace AutoGame.Core.Models;

using System.Collections.Generic;

public readonly record struct OpenFileDialogParms
{
    public string? InitialDirectory { get; init; }
    public string? FilterName { get; init; }
    public List<string> FilterPatterns { get; init; }
}