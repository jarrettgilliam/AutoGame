namespace AutoGame.Core.Interfaces;

public interface IRegistryService
{
    object? GetValue(string keyName, string? valueName, object? defaultValue);
}