namespace AutoGame.Core.Tests.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using AutoGame.Core.Interfaces;
using AutoGame.Core.Models;
using AutoGame.Core.Services;

public class ConfigServiceTests
{
    private ConfigService sut;
    private readonly Mock<IAppInfoService> appInfoServiceMock = new();
    private readonly Mock<IFileSystem> fileSystemMock = new();
    private readonly Mock<IFile> fileMock = new();
    private readonly Mock<IDirectory> directoryMock = new();
    private readonly Mock<IPath> pathMock = new();
    private readonly Mock<ISoftwareManager> softwareMock = new();
    private readonly Mock<IRuntimeInformation> runtimeInformationMock = new();

    private readonly Config configMock = new()
    {
        EnableTraceLogging = true,
        SoftwareKey = nameof(softwareMock),
        SoftwarePath = $"/saved/path/to/{nameof(softwareMock)}",
        SoftwareArguments = "--saved-arguments",
        LaunchWhenGameControllerConnected = true,
        LaunchWhenParsecConnected = true,
        IsDirty = false
    };

    private readonly MockFileSystemStream saveMemoryStream;
    private readonly MockFileSystemStream readMemoryStream;

    public ConfigServiceTests()
    {
        this.appInfoServiceMock
            .SetupGet(x => x.AppDataFolder)
            .Returns(@"C:\AutoGame");

        this.appInfoServiceMock
            .SetupGet(x => x.ConfigFilePath)
            .Returns(@"C:\AutoGame\config.json");

        this.readMemoryStream = new MockFileSystemStream(
            Encoding.UTF8.GetBytes(JsonSerializer.Serialize(this.configMock)),
            this.appInfoServiceMock.Object.ConfigFilePath);

        this.saveMemoryStream = new MockFileSystemStream(
            this.appInfoServiceMock.Object.ConfigFilePath);

        this.fileMock
            .Setup(x => x.OpenRead(this.readMemoryStream.Name))
            .Returns(() => this.readMemoryStream);

        this.fileMock
            .Setup(x => x.Create(this.saveMemoryStream.Name))
            .Returns(this.saveMemoryStream);

        this.fileMock
            .Setup(x => x.Exists(It.IsAny<string>()))
            .Returns(true);

        this.pathMock
            .Setup(x => x.GetFileName(It.IsAny<string>()))
            .Returns<string>(Path.GetFileName);

        this.fileSystemMock
            .SetupGet(x => x.File)
            .Returns(this.fileMock.Object);

        this.fileSystemMock
            .SetupGet(x => x.Directory)
            .Returns(this.directoryMock.Object);

        this.fileSystemMock
            .SetupGet(x => x.Path)
            .Returns(this.pathMock.Object);

        this.softwareMock
            .SetupGet(x => x.Key)
            .Returns(nameof(this.softwareMock));

        this.softwareMock
            .Setup(x => x.FindSoftwarePathOrDefault())
            .Returns($"/default/path/to/{nameof(this.softwareMock)}");

        this.softwareMock
            .SetupGet(x => x.DefaultArguments)
            .Returns("--default-arguments");

        this.runtimeInformationMock
            .Setup(x => x.IsOSPlatform(OSPlatform.Windows))
            .Returns(true);

        this.sut = new ConfigService(
            this.appInfoServiceMock.Object,
            this.fileSystemMock.Object,
            this.runtimeInformationMock.Object);
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
            .Setup(x => x.OpenRead(this.readMemoryStream.Name))
            .Returns(() => new MockFileSystemStream(
                "This is not json"u8.ToArray(),
                this.readMemoryStream.Name));

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

        var savedConfig = JsonSerializer.Deserialize<Config>(
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

    // These are intentionally hard coded strings
    [Theory]
    [InlineData("SoftwareKey")]
    [InlineData("SoftwarePath")]
    [InlineData("LaunchWhenGameControllerConnected")]
    [InlineData("LaunchWhenParsecConnected")]
    [InlineData("EnableTraceLogging")]
    public void Save_MaintainsPropertyNames(string serializedPropertyName)
    {
        this.sut.Save(this.configMock);

        string configJson = Encoding.UTF8.GetString(this.saveMemoryStream.ToArray());

        Assert.Contains(serializedPropertyName, configJson);
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

        this.sut = new ConfigService(this.appInfoServiceMock.Object, fsMock, this.runtimeInformationMock.Object);
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

        Assert.Equal(
            this.softwareMock.Object.DefaultArguments,
            config.SoftwareArguments);
    }

    [Fact]
    public void CreateDefault_Config_SetsVersion()
    {
        Config config = this.sut.CreateDefault(this.softwareMock.Object);

        int defaultVersion = config.Version;

        this.sut.Upgrade(config, this.softwareMock.Object);

        Assert.Equal(defaultVersion, config.Version);
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

    [Fact]
    public void Validate_SoftwarePathWrongSoftware_ConfigErrors()
    {
        this.configMock.SoftwarePath = "/path/to/wrongExecutable";
        this.sut.Validate(this.configMock, new[] { this.softwareMock.Object });
        Assert.True(this.configMock.HasErrors);
    }

    [Fact]
    public void Validate_NoDefaultSoftwarePath_NoErrors()
    {
        this.softwareMock.Setup(x => x.FindSoftwarePathOrDefault()).Returns("");
        this.configMock.SoftwarePath = "/path/to/anyExecutable";
        this.sut.Validate(this.configMock, new[] { this.softwareMock.Object });
        Assert.False(this.configMock.HasErrors);
    }

    [Fact]
    public void Upgrade_Version0To1_SetsVersion()
    {
        this.configMock.Version = 0;
        this.sut.Upgrade(this.configMock, this.softwareMock.Object);
        Assert.Equal(1, this.configMock.Version);
    }

    [Fact]
    public void Upgrade_Version0To1_AddsDefaultArguments()
    {
        this.configMock.Version = 0;
        this.configMock.SoftwareArguments = null;

        this.sut.Upgrade(this.configMock, this.softwareMock.Object);

        Assert.Equal(
            this.softwareMock.Object.DefaultArguments,
            this.configMock.SoftwareArguments);
    }

    [Fact]
    public void Upgrade_Version0To1_MovesLaunchWhenGamepadConnected()
    {
        const string oldPropertyName = "LaunchWhenGamepadConnected";
        JsonDocument extensionData = JsonDocument.Parse($"{{ \"{oldPropertyName}\": true }}");

        this.configMock.Version = 0;
        this.configMock.LaunchWhenGameControllerConnected = false;
        this.configMock.JsonExtensionData = new Dictionary<string, JsonElement>
        {
            { oldPropertyName, extensionData.RootElement.GetProperty(oldPropertyName) }
        };

        this.sut.Upgrade(this.configMock, this.softwareMock.Object);

        Assert.True(this.configMock.LaunchWhenGameControllerConnected);
        Assert.DoesNotContain(oldPropertyName, this.configMock.JsonExtensionData.Keys);
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

    private class MockFileSystemStream : FileSystemStream
    {
        public MockFileSystemStream(MemoryStream baseMemoryStream, string? path)
            : base(baseMemoryStream, path, true)
        {
            this.BaseMemoryStream = baseMemoryStream;
        }

        public MockFileSystemStream(byte[] buffer, string? path)
            : this(new MemoryStream(buffer), path)
        {
        }

        public MockFileSystemStream(string? path)
            : this(new MemoryStream(), path)
        {
        }

        private MemoryStream BaseMemoryStream { get; }

        public byte[] ToArray() => this.BaseMemoryStream.ToArray();
    }
}