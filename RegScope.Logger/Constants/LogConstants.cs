namespace RegScope.Logger.Constants;

/// <summary>
/// Provides logging configuration and message templates.
/// </summary>
public static class LogConstants
{
    public const int RetainedFileCountLimit = 10;

    public const string ApplicationFolderName = "RegScope";

    public const string LogDirectoryName = "Logs";

    public const string LogFileName = "RegScope-.log";

    public const string WriteProbeFileNameFormat = ".write-test-{0:N}.tmp";

    public const string OutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";

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

    public static string LogDirectory { get; } = Path.Combine(
                                                               Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                                               ApplicationFolderName,
                                                               LogDirectoryName);
}
