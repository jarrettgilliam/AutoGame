namespace AutoGame.Core.Models;

using Serilog.Events;

public readonly record struct MessageBoxParms(string Message, string Title, LogEventLevel Icon);