namespace AutoGame.Infrastructure;

using AutoGame.Core.Interfaces;
using AutoGame.Infrastructure.Interfaces;
using AutoGame.Infrastructure.LaunchConditions;
using AutoGame.Infrastructure.Services;
using AutoGame.Infrastructure.SoftwareManagers;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services)
    {
        services.AddServices();
        services.AddLaunchConditions();
        services.AddSoftwareManagers();
    }

    private static void AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeService, DateTimeService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IMMDeviceEnumerator, MMDeviceEnumeratorWrapper>();
        services.AddSingleton<INetStatPortsService, NetStatPortsService>();
        services.AddSingleton<IProcessService, ProcessService>();
        services.AddSingleton<ISleepService, SleepService>();
        services.AddSingleton<ISystemEventsService, SystemEventsService>();
        services.AddSingleton<IWindowService, WindowService>();
        services.AddSingleton<IRawGameControllerService, WindowsRawGameControllerService>();
        services.AddSingleton<IRegistryService, WindowsRegistryService>();
        services.AddSingleton<IUser32Service, WindowsUser32Service>();
    }

    private static void AddLaunchConditions(this IServiceCollection services)
    {
        services.AddSingleton<IGamepadConnectedCondition, GamepadConnectedCondition>();
        services.AddSingleton<IParsecConnectedCondition, ParsecConnectedCondition>();
    }

    private static void AddSoftwareManagers(this IServiceCollection services)
    {
        services.AddSingleton<ISoftwareManager, SteamBigPictureManager>();
        services.AddSingleton<ISoftwareManager, PlayniteFullscreenManager>();
    }
}