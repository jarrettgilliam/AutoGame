﻿namespace AutoGame.Core.Interfaces;

public interface ISoftwareManager
{
    string Key { get; }

    string Description { get; }

    string DefaultArguments { get; }

    bool IsRunning(string softwarePath);

    void Start(string softwarePath, string? softwareArguments);

    string FindSoftwarePathOrDefault();
}