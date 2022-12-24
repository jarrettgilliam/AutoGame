﻿namespace AutoGame.Core.Services;

using System;
using System.Collections.Generic;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;

internal sealed class AutoGameService : IAutoGameService
{
    private ISoftwareManager? appliedSoftware;
    private string? appliedSoftwarePath;
    private string? appliedSoftwareArguments;
    private IList<ILaunchCondition>? appliedLaunchConditions;

    public AutoGameService(
        ILoggingService loggingService,
        ISoftwareCollection availableSoftware,
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
    private ISoftwareCollection AvailableSoftware { get; }

    public void ApplyConfiguration(Config config)
    {
        this.appliedSoftware = this.AvailableSoftware.GetSoftwareByKeyOrNull(config.SoftwareKey);
        this.appliedSoftwarePath = config.SoftwarePath;
        this.appliedSoftwareArguments = config.SoftwareArguments;

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

    public void Dispose()
    {
        this.StopMonitoringAllLaunchConditions();
        this.appliedSoftware = null;
        this.appliedSoftwarePath = null;
        this.appliedSoftwareArguments = null;
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
                this.appliedSoftware.Start(this.appliedSoftwarePath, this.appliedSoftwareArguments);
            }
        }
        catch (Exception ex)
        {
            this.LoggingService.LogException("handling launch condition met", ex);
        }
    }
}