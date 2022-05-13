namespace AutoGame.Infrastructure.Services;

using AutoGame.Core.Interfaces;
using Microsoft.Win32;

public class WindowsRegistryService : IRegistryService
{
    public object? GetValue(string keyName, string? valueName, object? defaultValue) =>
        Registry.GetValue(keyName, valueName, defaultValue);
}