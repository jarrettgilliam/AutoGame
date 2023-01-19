namespace AutoGame.Infrastructure.Services;

using System;
using System.Threading.Tasks;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;
using Octokit;
using Serilog;

internal sealed class UpdateCheckingService : IUpdateCheckingService
{
    public UpdateCheckingService(
        IReleasesClient releasesClient,
        IAppInfoService appInfoService,
        ILogger logger)
    {
        this.ReleasesClient = releasesClient;
        this.AppInfoService = appInfoService;
        this.Logger = logger;
    }

    private IReleasesClient ReleasesClient { get; }
    private IAppInfoService AppInfoService { get; }
    private ILogger Logger { get; }

    public async Task<UpdateInfo> GetUpdateInfo()
    {
        try
        {
            Release? latestRelease = await this.ReleasesClient.GetLatest(
                "jarrettgilliam", "AutoGame");

            if (latestRelease is not null &&
                Version.TryParse(latestRelease.TagName?.TrimStart('v'), out var releaseVersion) &&
                releaseVersion > this.AppInfoService.CurrentVersion)
            {
                return new UpdateInfo
                {
                    IsAvailable = true,
                    NewVersion = releaseVersion,
                    Link = latestRelease.HtmlUrl
                };
            }
        }
        catch (Exception ex)
        {
            this.Logger.Warning(ex, "Unable to get update information");
        }

        return new UpdateInfo();
    }
}