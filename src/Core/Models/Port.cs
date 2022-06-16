namespace AutoGame.Core.Models;

using System.Text.RegularExpressions;

public record struct Port
{
    private static readonly Regex SpacesRegex = new(@"\s+");

    public string? Protocol;

    public string? LocalAddress;

    public string? ForeignAddress;

    public string? State;

    public int ProcessId;

    public static bool TryParse(string? s, out Port port)
    {
        port = new Port();

        if (string.IsNullOrEmpty(s))
        {
            return false;
        }

        var tokens = new Stack<string>(SpacesRegex.Split(s).Reverse());

        if (string.IsNullOrEmpty(tokens.Peek()))
        {
            tokens.Pop();
        }

        if (tokens.Count < 1)
        {
            return false;
        }

        port.Protocol = tokens.Pop();

        if (port.Protocol == "TCP")
        {
            if (tokens.Count < 4)
            {
                return false;
            }
        }
        else if (port.Protocol == "UDP")
        {
            if (tokens.Count < 3)
            {
                return false;
            }
        }
        else
        {
            return false;
        }

        port.LocalAddress = tokens.Pop();
        port.ForeignAddress = tokens.Pop();

        if (port.Protocol != "UDP")
        {
            port.State = tokens.Pop();
        }

        if (!int.TryParse(tokens.Pop(), out port.ProcessId))
        {
            return false;
        }

        return true;
    }
}