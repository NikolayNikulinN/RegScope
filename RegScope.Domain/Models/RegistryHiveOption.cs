using Microsoft.Win32;

namespace RegScope.Models;

public sealed class RegistryHiveOption
{
    public RegistryHiveOption(string displayName, RegistryHive? hive, bool isAllRegistry)
    {
        DisplayName = displayName;
        Hive = hive;
        IsAllRegistry = isAllRegistry;
    }

    public string DisplayName { get; }

    public RegistryHive? Hive { get; }

    public bool IsAllRegistry { get; }
}
