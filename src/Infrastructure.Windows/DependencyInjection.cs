// ReSharper disable once CheckNamespace

namespace AutoGame.Infrastructure;

using AutoGame.Core.Interfaces;
using AutoGame.Infrastructure.Windows.Interfaces;
using AutoGame.Infrastructure.Windows.Services;
using AutoGame.Infrastructure.Windows.SoftwareManagers;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.EventLog;

public static class DependencyInjection
{
    public static void AddPlatformInfrastructure(this IServiceCollection services)
    {
        services.AddServices();
        services.AddSoftwareManagers();
    }

    private static void AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IAppInfoService, AppInfoService>();
        services.AddSingleton<IGameControllerService, WindowsGameControllerService>();
        services.AddSingleton<IIpHelperApiService, IpHelperApiService>();
        services.AddSingleton<INetStatPortsService, WindowsNetStatPortsService>();
        services.AddSingleton<IRegistryService, WindowsRegistryService>();
        services.AddSingleton<IUser32Service, WindowsUser32Service>();
        services.AddSingleton<IWindowService, WindowService>();
    }

    private static void AddSoftwareManagers(this IServiceCollection services)
    {
        services.AddSingleton<ISoftwareManager, SteamBigPictureManager>();
        services.AddSingleton<ISoftwareManager, PlayniteFullscreenManager>();
        services.AddSingleton<ISoftwareManager, OtherSoftwareManager>();
    }

    public static LoggerConfiguration WriteToPlatformSystemLog(
        this LoggerConfiguration loggerConfiguration,
        string outputTemplate)
    {
        return loggerConfiguration.WriteTo.EventLog(
            source: nameof(AutoGame),
            eventIdProvider: new EventIdOneProvider(),
            outputTemplate: outputTemplate);
    }

    private class EventIdOneProvider : IEventIdProvider
    {
        public ushort ComputeEventId(LogEvent logEvent) => 1;
    }
}