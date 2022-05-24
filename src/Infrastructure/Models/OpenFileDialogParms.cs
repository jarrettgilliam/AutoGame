namespace AutoGame.Infrastructure.Models;

public class OpenFileDialogParms
{
    public string? FileName { get; set; }
    public string? InitialDirectory { get; set; }
    public string? Filter { get; set; }
}