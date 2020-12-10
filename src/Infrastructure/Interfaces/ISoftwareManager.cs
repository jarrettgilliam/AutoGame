namespace AutoGame.Infrastructure.Interfaces
{
    public interface ISoftwareManager
    {
        bool IsRunning { get; }

        void Start();
    }
}
