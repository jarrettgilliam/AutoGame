namespace AutoGame.Core.Interfaces;

public interface IAppInfoService
{
    string AppDataFolder { get; }

    string ConfigFilePath { get; }

    string LogFilePath { get; }
}