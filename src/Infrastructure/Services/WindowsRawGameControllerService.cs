namespace AutoGame.Infrastructure.Services;

using System;
using System.Linq;
using Windows.Gaming.Input;
using AutoGame.Infrastructure.Interfaces;

public sealed class WindowsRawGameControllerService : IRawGameControllerService
{
    private readonly object addRemoveLock = new();
    private bool subscribedToLowLevelEvent;
    private event EventHandler? rawGameControllerAdded;

    public event EventHandler? RawGameControllerAdded
    {
        add
        {
            lock (this.addRemoveLock)
            {
                this.rawGameControllerAdded += value;

                if (this.rawGameControllerAdded is not null && 
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
                this.rawGameControllerAdded -= value;

                if (this.rawGameControllerAdded is null &&
                    this.subscribedToLowLevelEvent)
                {
                    RawGameController.RawGameControllerAdded -= this.InternalOnRawGameControllerAdded;
                    this.subscribedToLowLevelEvent = false;
                }
            }
        }
    }

    public bool HasAnyRawGameControllers =>
        RawGameController.RawGameControllers.Any();

    private void InternalOnRawGameControllerAdded(object? sender, RawGameController e) =>
        this.rawGameControllerAdded?.Invoke(this, EventArgs.Empty);
}