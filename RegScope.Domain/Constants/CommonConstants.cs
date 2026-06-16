using System.IO;
using Microsoft.Win32;

namespace RegScope.Constants;

public static class CommonConstants
{
    public const int RetainedLogFileCount = 14;
    public const int SearchBatchSize = 75;
    public const int ValueDataDisplayLimit = 500;
    public const string DefaultValueName = "(Default)";
    public const string AppDataDirectoryName = "RegScope";
    public const string LogDirectoryName = "Logs";
    public const string LogFileNamePattern = "regscope-.log";

    public const string LogTemplateUnhandledUiException = "Unhandled UI exception.";
    public const string LogTemplateUnhandledApplicationException = "Unhandled application exception.";
    public const string LogTemplateApplicationStarted = "RegScope started.";
    public const string LogTemplateApplicationExited = "RegScope exited with code {ExitCode}.";
    public const string LogTemplateRegistrySearchStarted = "Registry search started for text {SearchText} in scope {Scope}.";
    public const string LogTemplateRegistrySearchCompleted = "Registry search completed with {ResultCount} results.";
    public const string LogTemplateRegistrySearchStopped = "Registry search stopped with {ResultCount} results.";
    public const string LogTemplateRegistrySearchFailed = "Registry search failed.";
    public const string LogTemplateUnableToDeleteRegistryItem = "Unable to delete registry item {DisplayText}.";
    public const string LogTemplateDeletedRegistryKey = "Deleted registry key {Hive}\\{Path} in view {View}.";
    public const string LogTemplateDeletedRegistryValue = "Deleted registry value {ValueName} from {Hive}\\{Path} in view {View}.";
    public const string LogTemplateUnableToOpenRegistryHive = "Unable to open registry hive {Hive} in view {View}.";
    public const string LogTemplateUnableToEnumerateRegistrySubKeys = "Unable to enumerate subkeys for {FullPath}.";
    public const string LogTemplateUnableToOpenRegistryKey = "Unable to open registry key {FullPath}.";
    public const string LogTemplateUnableToEnumerateRegistryValues = "Unable to enumerate values for {FullPath}.";
    public const string LogTemplateUnableToReadRegistryValue = "Unable to read registry value {ValueName} in {FullPath}.";

    public static string LogDirectory { get; } =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            AppDataDirectoryName,
            LogDirectoryName);

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
