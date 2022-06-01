namespace AutoGame.Core.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;

internal sealed class AutoGameService : IAutoGameService
{
    private ISoftwareManager? appliedSoftware;
    private string? appliedSoftwarePath;
    private IList<ILaunchCondition>? appliedLaunchConditions;

    public AutoGameService(
        ILoggingService loggingService,
        IEnumerable<ISoftwareManager> availableSoftware,
        IGameControllerConnectedCondition gameControllerConnectedCondition,
        IParsecConnectedCondition parsecConnectedCondition)
    {
        this.LoggingService = loggingService;
        this.AvailableSoftware = availableSoftware;
        this.GameControllerConnectedCondition = gameControllerConnectedCondition;
        this.ParsecConnectedCondition = parsecConnectedCondition;
    }

    private ILoggingService LoggingService { get; }
    private ILaunchCondition GameControllerConnectedCondition { get; }
    private ILaunchCondition ParsecConnectedCondition { get; }

    public IEnumerable<ISoftwareManager> AvailableSoftware { get; }

    public void ApplyConfiguration(Config config)
    {
        this.appliedSoftware = this.GetSoftwareByKeyOrNull(config.SoftwareKey);
        this.appliedSoftwarePath = config.SoftwarePath;

        this.StopMonitoringAllLaunchConditions();

        this.appliedLaunchConditions = new List<ILaunchCondition>();

        if (config.LaunchWhenGameControllerConnected)
        {
            this.appliedLaunchConditions.Add(this.GameControllerConnectedCondition);
        }

        if (config.LaunchWhenParsecConnected)
        {
            this.appliedLaunchConditions.Add(this.ParsecConnectedCondition);
        }

        foreach (ILaunchCondition condition in this.appliedLaunchConditions)
        {
            condition.ConditionMet += this.OnLaunchConditionMet;
            condition.StartMonitoring();
        }
    }

    public ISoftwareManager? GetSoftwareByKeyOrNull(string? softwareKey)
    {
        return this.AvailableSoftware.FirstOrDefault(s => s.Key == softwareKey);
    }

    public void Dispose()
    {
        this.StopMonitoringAllLaunchConditions();
        this.appliedSoftware = null;
        this.appliedSoftwarePath = null;
    }

    private void StopMonitoringAllLaunchConditions()
    {
        if (this.appliedLaunchConditions is null)
        {
            return;
        }

        foreach (ILaunchCondition condition in this.appliedLaunchConditions)
        {
            condition.ConditionMet -= this.OnLaunchConditionMet;
            condition.StopMonitoring();
        }

        this.appliedLaunchConditions = null;
    }

    private void OnLaunchConditionMet(object? sender, EventArgs e)
    {
        try
        {
            if (this.appliedSoftwarePath is not null &&
                this.appliedSoftware?.IsRunning(this.appliedSoftwarePath) == false)
            {
                this.appliedSoftware.Start(this.appliedSoftwarePath);
            }
        }
        catch (Exception ex)
        {
            this.LoggingService.LogException("handling launch condition met", ex);
        }
    }
}