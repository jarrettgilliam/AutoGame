namespace AutoGame.Infrastructure.Services;

using System;
using System.Linq;
using AutoGame.Infrastructure.Interfaces;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

internal sealed class GameControllerService : IGameControllerService, IDisposable
{
    private const int MAX_JOYSTICKS = 16;

    public GameControllerService() =>
        Joysticks.JoystickCallback += this.JoystickCallback;

    public void Dispose() =>
        Joysticks.JoystickCallback -= this.JoystickCallback;

    public event EventHandler? GameControllerAdded;

    public bool HasAnyGameControllers => Enumerable.Range(0, MAX_JOYSTICKS).Any(GLFW.JoystickPresent);

    private void JoystickCallback(int joystick, ConnectedState state)
    {
        if (state is ConnectedState.Connected)
        {
            this.GameControllerAdded?.Invoke(this, EventArgs.Empty);
        }
    }
    
    /// <summary>
    /// Copied from <see cref="OpenTK.Windowing.Desktop.Joysticks"/> 
    /// </summary>
    private static class Joysticks
    {
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