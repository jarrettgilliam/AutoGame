namespace AutoGame.Core;

using AutoGame.Core.Interfaces;
using AutoGame.Core.Services;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddCore(this IServiceCollection services)
    {
        services.AddSingleton<IAutoGameService, AutoGameService>();
        services.AddSingleton<IConfigService, ConfigService>();
        services.AddSingleton<ISoftwareCollection, SoftwareCollection>();
    }
}