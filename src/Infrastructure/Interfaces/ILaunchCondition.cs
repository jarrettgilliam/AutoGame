using System;

namespace AutoGame.Infrastructure.Interfaces
{
    public interface ILaunchCondition : IDisposable
    {
        event EventHandler ConditionMet;

        void StartCheckingConditions();
    }
}
