namespace AutoGame.Core.Interfaces;

public interface IWindowService
{
    bool RepeatTryForceForegroundWindowByTitle(string windowTitle, TimeSpan timeout);
}