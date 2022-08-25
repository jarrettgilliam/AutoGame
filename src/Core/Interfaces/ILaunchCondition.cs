namespace AutoGame.Core.Interfaces;

using System;

public interface ILaunchCondition
{
    event EventHandler? ConditionMet;

    void StartMonitoring();

    void StopMonitoring();
}