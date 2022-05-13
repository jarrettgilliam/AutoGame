namespace AutoGame.Core.Services;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;

public sealed class AutoGameService : IAutoGameService
{
    private ISoftwareManager? appliedSoftware;
    private string? appliedSoftwarePath;
    private IList<ILaunchCondition>? appliedLaunchConditions;

    public AutoGameService(
        ILoggingService loggingService,
        IFileSystem fileSystem,
        IList<ISoftwareManager> availableSoftware,
        ILaunchCondition gamepadConnectedCondition,
        ILaunchCondition parsecConnectedCondition)
    {
        this.LoggingService = loggingService;
        this.FileSystem = fileSystem;
        this.AvailableSoftware = availableSoftware;
        this.GamepadConnectedCondition = gamepadConnectedCondition;
        this.ParsecConnectedCondition = parsecConnectedCondition;
    }

    private ILoggingService LoggingService { get; }
    private IFileSystem FileSystem { get; }
    public IList<ISoftwareManager> AvailableSoftware { get; }
    private ILaunchCondition GamepadConnectedCondition { get; }
    private ILaunchCondition ParsecConnectedCondition { get; }

    public bool TryApplyConfiguration(Config config)
    {
        this.ValidateConfig(config);

        if (config.HasErrors)
        {
            return false;
        }

        this.appliedSoftware = this.GetSoftwareByKeyOrNull(config.SoftwareKey);
        this.appliedSoftwarePath = config.SoftwarePath;

        this.StopMonitoringAllLaunchConditions();

        this.appliedLaunchConditions = new List<ILaunchCondition>();

        if (config.LaunchWhenGamepadConnected)
        {
            this.appliedLaunchConditions.Add(this.GamepadConnectedCondition);
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

        return true;
    }

    public Config CreateDefaultConfiguration()
    {
        ISoftwareManager s = this.AvailableSoftware.First();

        return new Config()
        {
            SoftwareKey = s.Key,
            SoftwarePath = s.FindSoftwarePathOrDefault(),
            LaunchWhenGamepadConnected = true,
            LaunchWhenParsecConnected = true
        };
    }

    public ISoftwareManager? GetSoftwareByKeyOrNull(string? softwareKey)
    {
        return this.AvailableSoftware.FirstOrDefault(s => s.Key == softwareKey) ??
               this.AvailableSoftware.FirstOrDefault();
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
            if (this.appliedSoftware?.IsRunning == false && 
                this.appliedSoftwarePath is not null)
            {
                this.appliedSoftware.Start(this.appliedSoftwarePath);
            }
        }
        catch (Exception ex)
        {
            this.LoggingService.LogException("handling launch condition met", ex);
        }
    }

    private void ValidateConfig(Config config)
    {
        config.ClearAllErrors();

        if (string.IsNullOrEmpty(config.SoftwarePath))
        {
            config.AddError(nameof(config.SoftwarePath), "Required");
        }
        else if (!this.FileSystem.File.Exists(config.SoftwarePath))
        {
            config.AddError(nameof(config.SoftwarePath), "File not found");
        }
    }
}