using Microsoft.Win32;

namespace RegScope.Models;

public sealed class RegistryDeleteRequest
{
    public required RegistryHive HiveKey { get; init; }

    public required RegistryView RegistryView { get; init; }

    public required string SubKeyPath { get; init; }

    public string? ValueName { get; init; }

    public required bool DeleteKey { get; init; }
}
