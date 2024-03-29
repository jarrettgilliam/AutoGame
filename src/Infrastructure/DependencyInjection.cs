﻿namespace AutoGame.Infrastructure;

using AutoGame.Core.Interfaces;
using AutoGame.Infrastructure.LaunchConditions;
using AutoGame.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services)
    {
        services.AddServices();
        services.AddLaunchConditions();
    }

    private static void AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeService, DateTimeService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IMessageBoxSink, MessageBoxSink>();
        services.AddSingleton<IProcessService, ProcessService>();
        services.AddSingleton<IRuntimeInformation, RuntimeInformationWrapper>();
        services.AddSingleton<ISleepService, SleepService>();
        services.AddSingleton<IUpdateCheckingService, UpdateCheckingService>();
    }

    private static void AddLaunchConditions(this IServiceCollection services)
    {
        services.AddSingleton<IGameControllerConnectedCondition, GameControllerConnectedCondition>();
        services.AddSingleton<IParsecConnectedCondition, ParsecConnectedCondition>();
    }
}