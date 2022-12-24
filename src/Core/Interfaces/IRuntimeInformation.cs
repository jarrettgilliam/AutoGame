namespace AutoGame.Core.Interfaces;

using System.Runtime.InteropServices;

public interface IRuntimeInformation
{
    bool IsOSPlatform(OSPlatform platform);
}