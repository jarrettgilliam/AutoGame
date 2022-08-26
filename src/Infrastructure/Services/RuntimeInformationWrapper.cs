namespace AutoGame.Infrastructure.Services;

using System.Runtime.InteropServices;
using AutoGame.Core.Services;

internal sealed class RuntimeInformationWrapper : IRuntimeInformation
{
    public bool IsOSPlatform(OSPlatform platform) => RuntimeInformation.IsOSPlatform(platform);
}