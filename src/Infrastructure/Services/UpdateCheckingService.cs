namespace AutoGame.Infrastructure.Services;

using System;
using System.Threading.Tasks;
using AutoGame.Core.Enums;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;
using Octokit;

internal sealed class UpdateCheckingService : IUpdateCheckingService
{
    public UpdateCheckingService(
        IReleasesClient releasesClient,
        IAppInfoService appInfoService,
        ILoggingService loggingService)
    {
        this.ReleasesClient = releasesClient;
        this.AppInfoService = appInfoService;
        this.LoggingService = loggingService;
    }

    private IReleasesClient ReleasesClient { get; }
    private IAppInfoService AppInfoService { get; }
    private ILoggingService LoggingService { get; }

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
            this.LoggingService.LogException("Unable to get update information", ex, LogLevel.Warning);
        }

        return new UpdateInfo();
    }
}