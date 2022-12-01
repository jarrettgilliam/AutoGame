namespace AutoGame.Infrastructure.Windows.Services;

using AutoGame.Infrastructure.Windows.Interfaces;
using Microsoft.Win32;

internal sealed class WindowsRegistryService : IRegistryService
{
    public object? GetValue(string keyName, string? valueName, object? defaultValue) =>
        Registry.GetValue(keyName, valueName, defaultValue);
}