namespace AutoGame.Infrastructure.Services;

using System;
using AutoGame.Core.Interfaces;

public class SystemEventsService : ISystemEventsService
{
    public event EventHandler? DisplaySettingsChanged
    {
        add => Microsoft.Win32.SystemEvents.DisplaySettingsChanged += value;
        remove => Microsoft.Win32.SystemEvents.DisplaySettingsChanged -= value;
    }
}