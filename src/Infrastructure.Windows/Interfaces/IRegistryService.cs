namespace AutoGame.Infrastructure.Windows.Interfaces;

public interface IRegistryService
{
    object? GetValue(string keyName, string? valueName, object? defaultValue);
}