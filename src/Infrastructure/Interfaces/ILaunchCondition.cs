using System;

namespace AutoGame.Infrastructure.Interfaces
{
    public interface ILaunchCondition
    {
        event EventHandler ConditionMet;

        void StartMonitoring();

        void StopMonitoring();
    }
}
