using System.IO;
using Microsoft.Win32;

namespace RegScope.Constants;

public static class CommonConstants
{
    public const int RetainedLogFileCount = 14;
    public const int SearchBatchSize = 75;
    public const int ValueDataDisplayLimit = 500;
    public const string DefaultValueName = "(Default)";
    public const string LogFileNamePattern = "regscope-.log";

    public static string LogDirectory { get; } =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RegScope",
            "Logs");

    public static readonly RegistryHiveOptionData[] HiveOptions =
    [
        new("HKEY_CLASSES_ROOT", RegistryHive.ClassesRoot, false),
        new("HKEY_CURRENT_USER", RegistryHive.CurrentUser, false),
        new("HKEY_LOCAL_MACHINE", RegistryHive.LocalMachine, false),
        new("HKEY_USERS", RegistryHive.Users, false),
        new("HKEY_CURRENT_CONFIG", RegistryHive.CurrentConfig, false),
        new("All Registry", null, true)
    ];
}

public sealed record RegistryHiveOptionData(string DisplayName, RegistryHive? Hive, bool IsAllRegistry);
