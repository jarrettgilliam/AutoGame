namespace AutoGame.Core.Interfaces;

using System;
using System.Collections.Generic;

public interface IAppInfoService
{
    string AppDataFolder { get; }

    string ConfigFilePath { get; }

    string LogFilePath { get; }

    IEnumerable<string> ParsecLogDirectories { get; }

    Version CurrentVersion { get; }
}