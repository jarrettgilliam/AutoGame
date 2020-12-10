namespace AutoGame.Infrastructure.Interfaces
{
    public interface ISoftwareManager
    {
        string Key { get; }

        string Description { get; }

        bool IsRunning { get; }

        void Start(string softwarePath);

        string FindSoftwarePathOrDefault();
    }
}
