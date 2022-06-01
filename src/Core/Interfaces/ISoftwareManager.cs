namespace AutoGame.Core.Interfaces;

public interface ISoftwareManager
{
    string Key { get; }

    string Description { get; }

    bool IsRunning(string softwarePath);

    void Start(string softwarePath);

    string FindSoftwarePathOrDefault();
}