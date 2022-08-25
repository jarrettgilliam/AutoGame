namespace AutoGame.Infrastructure.Windows.Interfaces;

using System;

public interface IWindowService
{
    bool RepeatTryForceForegroundWindowByTitle(
        string windowTitle,
        TimeSpan timeout);
}