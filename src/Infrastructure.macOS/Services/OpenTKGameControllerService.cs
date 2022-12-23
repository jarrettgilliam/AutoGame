namespace AutoGame.Infrastructure.macOS.Services;

using System;
using System.Linq;
using AutoGame.Core.Interfaces;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

internal sealed class OpenTKGameControllerService : IGameControllerService, IDisposable
{
    private const int MAX_JOYSTICKS = 16;

    private readonly object addRemoveLock = new();
    private bool subscribedToLowLevelEvent;
    private event EventHandler? gameControllerAdded;

    public void Dispose() => Joysticks.JoystickCallback -= this.JoystickCallback;

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
                    Joysticks.JoystickCallback += this.JoystickCallback;
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
                    Joysticks.JoystickCallback -= this.JoystickCallback;
                    this.subscribedToLowLevelEvent = false;
                }
            }
        }
    }

    public bool HasAnyGameControllers => Enumerable.Range(0, MAX_JOYSTICKS).Any(GLFW.JoystickPresent);

    private void JoystickCallback(int joystick, ConnectedState state)
    {
        if (state is ConnectedState.Connected)
        {
            this.gameControllerAdded?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Copied from <see cref="OpenTK.Windowing.Desktop.Joysticks"/>
    /// </summary>
    private static class Joysticks
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private static readonly GLFWCallbacks.JoystickCallback? joystickCallback;

        public static event GLFWCallbacks.JoystickCallback? JoystickCallback;

        static Joysticks()
        {
            GLFWProvider.EnsureInitialized();
            joystickCallback = (id, state) => JoystickCallback?.Invoke(id, state);
            GLFW.SetJoystickCallback(joystickCallback);
        }
    }
}