namespace AutoGame.Core.Models;

using System;

public sealed class UpdateInfo
{
    public bool IsAvailable { get; init; }

    public Version? NewVersion { get; init; }

    public string? Link { get; init; }
}