using Microsoft.Win32;

namespace RegScope.Presentation.Constants;

/// <summary>
/// Provides reusable non-localized application constants.
/// </summary>
public sealed class CommonConstants
{
    public const int RetainedLogFileCount = 14;


   
}

public sealed record RegistryHiveOptionData(string DisplayName, RegistryHive? Hive, bool IsAllRegistry);