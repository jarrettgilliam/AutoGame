namespace AutoGame.Infrastructure.Tests.LaunchConditions;

using System;
using AutoGame.Core.Interfaces;
using IDisposable = ABI.System.IDisposable;

internal class LaunchConditionTestHelper : IDisposable
{
    private readonly ILaunchCondition launchCondition;

    public LaunchConditionTestHelper(ILaunchCondition launchCondition)
    {
        this.launchCondition = launchCondition;
        this.launchCondition.ConditionMet += this.OnConditionMet;
        this.launchCondition.StartMonitoring();
    }

    public int FiredCount { get; private set; }

    public void Dispose()
    {
        this.launchCondition.StopMonitoring();
        this.launchCondition.ConditionMet -= this.OnConditionMet;
    }

    private void OnConditionMet(object? sender, EventArgs e) => this.FiredCount++;
}