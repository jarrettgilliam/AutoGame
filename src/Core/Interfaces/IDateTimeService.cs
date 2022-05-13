namespace AutoGame.Core.Interfaces;

public interface IDateTimeService
{
    DateTime UtcNow { get; }
    DateTimeOffset NowOffset { get; }
}