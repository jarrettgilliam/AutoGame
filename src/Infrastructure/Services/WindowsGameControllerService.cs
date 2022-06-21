namespace AutoGame.Infrastructure.Services;

using System;
using System.Linq;
using Windows.Gaming.Input;
using AutoGame.Infrastructure.Interfaces;

internal sealed class WindowsGameControllerService : IGameControllerService, IDisposable
{
    public WindowsGameControllerService() =>
        RawGameController.RawGameControllerAdded += this.OnRawGameControllerAdded;

    public void Dispose() =>
        RawGameController.RawGameControllerAdded -= this.OnRawGameControllerAdded;

    public event EventHandler? GameControllerAdded;

    public bool HasAnyGameControllers =>
        RawGameController.RawGameControllers.Any();

    private void OnRawGameControllerAdded(object? sender, RawGameController e) =>
        this.GameControllerAdded?.Invoke(this, EventArgs.Empty);
}