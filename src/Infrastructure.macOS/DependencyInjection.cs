// ReSharper disable once CheckNamespace

namespace AutoGame.Infrastructure;

using AutoGame.Core.Interfaces;
using AutoGame.Infrastructure.macOS.Services;
using AutoGame.Infrastructure.macOS.SoftwareManagers;
using Microsoft.Extensions.DependencyInjection;

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
        services.AddSingleton<INetStatPortsService, NetStatPortsService>();
    }

    private static void AddSoftwareManagers(this IServiceCollection services)
    {
        services.AddSingleton<ISoftwareManager, SteamBigPictureManager>();
    }
}