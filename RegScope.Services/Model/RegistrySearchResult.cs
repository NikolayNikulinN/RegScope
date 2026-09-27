using Microsoft.Win32;

namespace RegScope.Models;

public sealed class RegistrySearchResult
{
    public required string Hive { get; init; }

    public required string FullPath { get; init; }

    public required RegistryMatchType MatchType { get; init; }

    public string? ValueName { get; init; }

    public string? ValueKind { get; init; }

    public string? ValueData { get; init; }

    public object? FullValueData { get; init; }

    public required bool CanDeleteKey { get; init; }

    public required RegistryHive HiveKey { get; init; }

    public required RegistryView RegistryView { get; init; }

    public required string SubKeyPath { get; init; }

    public string? ErrorMessage { get; set; }

    public bool IsValueResult => MatchType is RegistryMatchType.ValueName or RegistryMatchType.ValueData;
}
