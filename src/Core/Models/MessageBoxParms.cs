namespace AutoGame.Core.Models;

using AutoGame.Core.Enums;

public readonly record struct MessageBoxParms(string Message, string Title, LogLevel Icon);