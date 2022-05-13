namespace AutoGame.Core.Interfaces;

public interface ILaunchCondition
{
    event EventHandler? ConditionMet;

    void StartMonitoring();

    void StopMonitoring();
}