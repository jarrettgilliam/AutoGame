namespace AutoGame.Core.Interfaces;

using System;

public interface ISleepService
{
    void Sleep(TimeSpan timeout);
}