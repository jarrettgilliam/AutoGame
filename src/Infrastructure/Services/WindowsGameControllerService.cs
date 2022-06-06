namespace AutoGame.Infrastructure.Services;

using System;
using System.Linq;
using AutoGame.Infrastructure.Interfaces;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

internal sealed class WindowsGameControllerService : IGameControllerService, IDisposable
{
    public WindowsGameControllerService()
    {
        this.NativeWindow = new NativeWindow(new NativeWindowSettings
        {
            StartVisible = false,
        });
        
        this.NativeWindow.JoystickConnected += this.NativeWindow_OnJoystickConnected;
    }

    public event EventHandler? GameControllerAdded;

    public bool HasAnyGameControllers => this.NativeWindow.JoystickStates.Any(x => x is not null);

    private void NativeWindow_OnJoystickConnected(JoystickEventArgs obj)
    {
        this.GameControllerAdded?.Invoke(this, EventArgs.Empty);
    }

    private NativeWindow NativeWindow { get; }

    public void Dispose()
    {
        this.NativeWindow.Dispose();
    }
}