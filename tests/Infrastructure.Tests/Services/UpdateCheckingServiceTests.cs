namespace AutoGame.Infrastructure.Tests.Services;

using System;
using System.Threading.Tasks;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;
using AutoGame.Infrastructure.Services;
using Octokit;
using Serilog;

public class UpdateCheckingServiceTests
{
    private readonly UpdateCheckingService sut;

    private readonly Mock<IReleasesClient> releasesClientMock = new();
    private readonly Mock<IAppInfoService> appInfoServiceMock = new();
    private readonly Mock<ILogger> loggerMock = new();

    private string mockTagName = "v2.0.1";
    private string mockHtmlUrl = string.Empty;

    public UpdateCheckingServiceTests()
    {
        this.releasesClientMock
            .Setup(x => x.GetLatest(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(() =>
            {
                this.mockHtmlUrl = $"https://github.com/jarrettgilliam/AutoGame/releases/tag/{this.mockTagName}";

                return new Release(
                    url: "",
                    htmlUrl: this.mockHtmlUrl,
                    assetsUrl: "",
                    uploadUrl: "",
                    id: 0,
                    nodeId: "",
                    tagName: this.mockTagName,
                    targetCommitish: "",
                    name: "",
                    body: "",
                    draft: false,
                    prerelease: false,
                    createdAt: DateTimeOffset.MinValue,
                    publishedAt: DateTimeOffset.MinValue,
                    author: new Author(),
                    tarballUrl: "",
                    zipballUrl: "",
                    assets: Array.Empty<ReleaseAsset>());
            });

        this.appInfoServiceMock
            .SetupGet(x => x.CurrentVersion)
            .Returns(new Version(2, 0, 0));

        this.sut = new UpdateCheckingService(
            this.releasesClientMock.Object,
            this.appInfoServiceMock.Object,
            this.loggerMock.Object);
    }

    [Fact]
    public async Task GetUpdateInfo_Returns_Correct_Data()
    {
        UpdateInfo result = await this.sut.GetUpdateInfo();

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetUpdateInfo_Calls_ReleaseClient_GetLatestRelease()
    {
        await this.sut.GetUpdateInfo();

        this.releasesClientMock.Verify(x => x.GetLatest(
            "jarrettgilliam", "AutoGame"), Times.Once);
    }

    [Theory]
    [InlineData("v2.0.1")]
    [InlineData("v2.1.0")]
    [InlineData("v3.0.0")]
    public async Task GetUpdateInfo_IsAvailable_Is_True(string tag)
    {
        this.mockTagName = tag;

        UpdateInfo result = await this.sut.GetUpdateInfo();

        Assert.True(result.IsAvailable);
        Assert.Equal(this.mockTagName, $"v{result.NewVersion}");
        Assert.Equal(this.mockHtmlUrl, result.Link);
    }

    [Theory]
    [InlineData("v1.0.0")]
    [InlineData("v1.9.9")]
    [InlineData("v2.0.0")]
    [InlineData("2.0.0")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("test")]
    public async Task GetUpdateInfo_IsAvailable_Is_False(string tag)
    {
        this.mockTagName = tag;

        UpdateInfo result = await this.sut.GetUpdateInfo();

        Assert.False(result.IsAvailable);
        Assert.Null(result.NewVersion);
        Assert.Null(result.Link);
    }

    [Fact]
    public async Task GetUpdateInfo_Catches_Exceptions()
    {
        this.appInfoServiceMock
            .SetupGet(x => x.CurrentVersion)
            .Throws(new Exception("test"));

        Assert.NotNull(await this.sut.GetUpdateInfo());
    }

    [Fact]
    public async Task GetUpdateInfo_Logs_Exceptions()
    {
        this.appInfoServiceMock
            .SetupGet(x => x.CurrentVersion)
            .Throws(new Exception("test"));

        await this.sut.GetUpdateInfo();

        this.loggerMock.Verify(
                x => x.Warning(It.IsAny<Exception?>(), It.IsAny<string>()),
                Times.Once);
    }
}