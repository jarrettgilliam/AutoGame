// ReSharper disable once CheckNamespace

namespace AutoGame.Infrastructure;

using AutoGame.Core.Interfaces;
using AutoGame.Infrastructure.macOS.Services;
using AutoGame.Infrastructure.macOS.SoftwareManagers;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

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
        services.AddSingleton<IGameControllerService, OpenTKGameControllerService>();
        services.AddSingleton<INetStatPortsService, NetStatPortsService>();
        services.AddSingleton<IParsecConnectedCondition, MacOSParsecConnectedCondition>();
    }

    private static void AddSoftwareManagers(this IServiceCollection services)
    {
        services.AddSingleton<ISoftwareManager, SteamBigPictureManager>();
    }

    public static LoggerConfiguration WriteToPlatformSystemLog(
        this LoggerConfiguration loggerConfiguration,
        string outputTemplate)
    {
        return loggerConfiguration;
    }
}