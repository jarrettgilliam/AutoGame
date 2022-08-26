namespace AutoGame.Core.Services;

using System.Runtime.InteropServices;

public interface IRuntimeInformation
{
    bool IsOSPlatform(OSPlatform platform);
}