namespace AutoGame.Infrastructure.Windows.Services;

using System;
using System.Linq;
using AutoGame.Core.Interfaces;
using global::Windows.Gaming.Input;

internal sealed class WindowsGameControllerService : IGameControllerService
{
    private readonly object addRemoveLock = new();
    private bool subscribedToLowLevelEvent;
    private event EventHandler? gameControllerAdded;

    public event EventHandler? GameControllerAdded
    {
        add
        {
            lock (this.addRemoveLock)
            {
                this.gameControllerAdded += value;

                if (this.gameControllerAdded is not null &&
                    !this.subscribedToLowLevelEvent)
                {
                    RawGameController.RawGameControllerAdded += this.InternalOnRawGameControllerAdded;
                    this.subscribedToLowLevelEvent = true;
                }
            }
        }
        remove
        {
            lock (this.addRemoveLock)
            {
                this.gameControllerAdded -= value;

                if (this.gameControllerAdded is null &&
                    this.subscribedToLowLevelEvent)
                {
                    RawGameController.RawGameControllerAdded -= this.InternalOnRawGameControllerAdded;
                    this.subscribedToLowLevelEvent = false;
                }
            }
        }
    }

    public bool HasAnyGameControllers =>
        RawGameController.RawGameControllers.Any();

    private void InternalOnRawGameControllerAdded(object? sender, RawGameController e) =>
        this.gameControllerAdded?.Invoke(this, EventArgs.Empty);
}