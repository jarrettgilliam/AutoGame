namespace AutoGame.Infrastructure.Interfaces
{
    public interface ILauncherManager
    {
        bool IsRunning { get; }

        void Start();
    }
}
