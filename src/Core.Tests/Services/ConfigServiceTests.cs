namespace AutoGame.Core.Tests.Services;

using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Reflection;
using System.Text;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;
using AutoGame.Core.Services;
using Newtonsoft.Json;

public class ConfigServiceTests
{
    private ConfigService sut;
    private readonly Mock<IAppInfoService> appInfoServiceMock = new();
    private readonly Mock<IFileSystem> fileSystemMock = new();
    private readonly Mock<IFile> fileMock = new();
    private readonly Mock<IDirectory> directoryMock = new();
    private readonly Mock<ISoftwareManager> softwareMock = new();

    private readonly Config configMock = new()
    {
        EnableTraceLogging = true,
        SoftwareKey = nameof(softwareMock),
        SoftwarePath = "c:\\steam.exe",
        LaunchWhenGamepadConnected = true,
        LaunchWhenParsecConnected = true,
        IsDirty = false
    };

    private readonly MemoryStream saveMemoryStream = new();

    public ConfigServiceTests()
    {
        this.appInfoServiceMock
            .SetupGet(x => x.AppDataFolder)
            .Returns(@"C:\AutoGame");

        this.appInfoServiceMock
            .SetupGet(x => x.ConfigFilePath)
            .Returns(@"C:\AutoGame\config.json");

        this.fileMock
            .Setup(x => x.OpenRead(this.appInfoServiceMock.Object.ConfigFilePath))
            .Returns(() =>
                new MemoryStream(
                    Encoding.UTF8.GetBytes(
                        JsonConvert.SerializeObject(this.configMock))));

        this.fileMock
            .Setup(x => x.Create(this.appInfoServiceMock.Object.ConfigFilePath))
            .Returns(this.saveMemoryStream);

        this.fileMock
            .Setup(x => x.Exists(It.IsAny<string>()))
            .Returns(true);

        this.fileSystemMock
            .SetupGet(x => x.File)
            .Returns(this.fileMock.Object);

        this.fileSystemMock
            .SetupGet(x => x.Directory)
            .Returns(this.directoryMock.Object);

        this.softwareMock
            .SetupGet(x => x.Key)
            .Returns(nameof(this.softwareMock));

        this.softwareMock
            .Setup(x => x.FindSoftwarePathOrDefault())
            .Returns($"/path/to/{nameof(this.softwareMock)}");

        this.sut = new ConfigService(
            this.appInfoServiceMock.Object,
            this.fileSystemMock.Object);
    }

    [Fact]
    public void GetConfigOrNull_ReturnsConfig()
    {
        Config? retrievedConfig = this.sut.GetConfigOrNull();
        Assert.True(this.ConfigsAreEqual(this.configMock, retrievedConfig));
    }

    [Theory]
    [InlineData(typeof(FileNotFoundException))]
    [InlineData(typeof(DirectoryNotFoundException))]
    public void GetConfigOrNull_HandlesException_ReturnsNull(Type exceptionType)
    {
        this.fileMock
            .Setup(x => x.OpenRead(this.appInfoServiceMock.Object.ConfigFilePath))
            .Returns(() => throw ((Exception)Activator.CreateInstance(exceptionType)!));

        Assert.Null(this.sut.GetConfigOrNull());
    }

    [Theory]
    [InlineData(typeof(Exception))]
    [InlineData(typeof(IOException))]
    [InlineData(typeof(PathTooLongException))]
    [InlineData(typeof(UnauthorizedAccessException))]
    [InlineData(typeof(NotSupportedException))]
    public void GetConfigOrNull_Exception_Throws(Type exceptionType)
    {
        this.fileMock
            .Setup(x => x.OpenRead(this.appInfoServiceMock.Object.ConfigFilePath))
            .Returns(() => throw ((Exception)Activator.CreateInstance(exceptionType)!));

        Assert.Throws(exceptionType, () => this.sut.GetConfigOrNull());
    }

    [Fact]
    public void GetConfigOrNull_CantDeserialize_Throws()
    {
        this.fileMock
            .Setup(x => x.OpenRead(this.appInfoServiceMock.Object.ConfigFilePath))
            .Returns(() =>
                new MemoryStream(
                    Encoding.UTF8.GetBytes(
                        "This is not json")));

        Assert.ThrowsAny<Exception>(() => this.sut.GetConfigOrNull());
    }

    [Fact]
    public void GetConfigOrNull_SetsIsDirty_False()
    {
        Config config = this.sut.GetConfigOrNull()!;
        Assert.False(config.IsDirty);
    }

    [Fact]
    public void Save_CreatesDirectory()
    {
        this.sut.Save(this.configMock);

        this.directoryMock.Verify(
            x => x.CreateDirectory(this.appInfoServiceMock.Object.AppDataFolder),
            Times.Once);
    }

    [Fact]
    public void Save_WritesJson()
    {
        this.sut.Save(this.configMock);

        Config savedConfig = JsonConvert.DeserializeObject<Config>(
            Encoding.UTF8.GetString(this.saveMemoryStream.ToArray()))!;

        savedConfig.IsDirty = false;

        Assert.True(this.ConfigsAreEqual(this.configMock, savedConfig));
    }

    [Theory]
    [InlineData(nameof(Config.IsDirty))]
    [InlineData(nameof(Config.HasErrors))]
    public void Save_ExcludesIgnoredProperties(string ignoredPropertyName)
    {
        this.sut.Save(this.configMock);

        string configJson = Encoding.UTF8.GetString(this.saveMemoryStream.ToArray());

        Assert.DoesNotContain(ignoredPropertyName, configJson);
    }

    [Fact]
    public void Save_SetsIsDirty_False()
    {
        this.configMock.IsDirty = true;

        this.sut.Save(this.configMock);

        Assert.False(this.configMock.IsDirty);
    }

    [Fact]
    public void Save_LargeExistingFile_Truncates()
    {
        var fsMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            {
                this.appInfoServiceMock.Object.ConfigFilePath,
                new MockFileData(new string('a', 1000))
            }
        });

        this.sut = new ConfigService(this.appInfoServiceMock.Object, fsMock);
        this.sut.Save(this.configMock);

        string configFileText = fsMock.File.ReadAllText(
            this.appInfoServiceMock.Object.ConfigFilePath);

        Assert.Equal('}', configFileText.Last());
    }

    [Fact]
    public void CreateDefault_Config_NotNull()
    {
        Assert.NotNull(this.sut.CreateDefault(this.softwareMock.Object));
    }

    [Fact]
    public void CreateDefault_Config_MatchesFirstSoftware()
    {
        Config config = this.sut.CreateDefault(this.softwareMock.Object);

        Assert.Equal(
            this.softwareMock.Object.Key,
            config.SoftwareKey);

        Assert.Equal(
            this.softwareMock.Object.FindSoftwarePathOrDefault(),
            config.SoftwarePath);
    }

    [Fact]
    public void Validate_ValidConfig_NoErrors()
    {
        this.sut.Validate(this.configMock, new[] { this.softwareMock.Object });
        Assert.False(this.configMock.HasErrors);
    }

    [Fact]
    public void Validate_EmptySoftwarePath_ConfigErrors()
    {
        this.configMock.SoftwarePath = string.Empty;
        this.sut.Validate(this.configMock, new[] { this.softwareMock.Object });
        Assert.True(this.configMock.HasErrors);
    }

    [Fact]
    public void Validate_SoftwarePathDoesntExist_ConfigErrors()
    {
        this.fileMock.Setup(x => x.Exists(It.IsAny<string>())).Returns(false);
        this.sut.Validate(this.configMock, new[] { this.softwareMock.Object });
        Assert.True(this.configMock.HasErrors);
    }

    [Fact]
    public void Validate_BadSoftwareKey_ConfigErrors()
    {
        this.configMock.SoftwareKey = "BadKey";
        this.sut.Validate(this.configMock, new[] { this.softwareMock.Object });
        Assert.True(this.configMock.HasErrors);
    }

    private bool ConfigsAreEqual(Config? left, Config? right)
    {
        if (left == null || right == null)
        {
            return left == right;
        }

        IEnumerable<PropertyInfo> serializedProperties = typeof(Config)
            .GetProperties()
            .Where(p => p.CanRead && p.CanWrite);

        return serializedProperties.All(property =>
            Equals(property.GetValue(left), property.GetValue(right)));
    }
}