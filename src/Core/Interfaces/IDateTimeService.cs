namespace AutoGame.Core.Interfaces;

using System;

public interface IDateTimeService
{
    DateTime UtcNow { get; }
    DateTimeOffset NowOffset { get; }
}