﻿namespace AutoGame.Infrastructure.Windows.SoftwareManagers;

using System;
using System.IO;
using AutoGame.Core.Interfaces;
using AutoGame.Infrastructure.Windows.Interfaces;

internal sealed class HeroicManager : ISoftwareManager
{
    public HeroicManager(
        IUser32Service user32Service,
        IProcessService processService)
    {
        this.User32Service = user32Service;
        this.ProcessService = processService;
    }

    private IUser32Service User32Service { get; }
    private IProcessService ProcessService { get; }

    public string Key => "Heroic";

    public string Description => "Heroic Games Launcher";

    public string DefaultArguments => "--fullscreen";

    public bool IsRunning(string softwarePath) =>
        this.User32Service.FindWindow("Chrome_WidgetWin_1", "Heroic Games Launcher") != IntPtr.Zero;

    public void Start(string softwarePath, string? softwareArguments) =>
        this.ProcessService.Start(softwarePath, softwareArguments).Dispose();

    public string FindSoftwarePathOrDefault() =>
        Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Programs",
            "heroic",
            "Heroic.exe");
}