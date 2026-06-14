using System.Globalization;
using System.Resources;

#nullable enable

namespace RegScope.Resources;

public static class Strings
{
    private static readonly ResourceManager ResourceManager = new("RegScope.Resources.Strings", typeof(Strings).Assembly);

    public static CultureInfo? Culture { get; set; }

    public static string AccessDeniedDeleteError => GetString(nameof(AccessDeniedDeleteError));
    public static string AccessDeniedError => GetString(nameof(AccessDeniedError));
    public static string AppTitle => GetString(nameof(AppTitle));
    public static string CanDeleteKeyColumn => GetString(nameof(CanDeleteKeyColumn));
    public static string CannotDeleteRootKeyError => GetString(nameof(CannotDeleteRootKeyError));
    public static string ConfirmDeleteKeyMessageFormat => GetString(nameof(ConfirmDeleteKeyMessageFormat));
    public static string ConfirmDeleteSelectedMessageFormat => GetString(nameof(ConfirmDeleteSelectedMessageFormat));
    public static string ConfirmDeleteSelectionListMessage => GetString(nameof(ConfirmDeleteSelectionListMessage));
    public static string ConfirmDeleteValueMessageFormat => GetString(nameof(ConfirmDeleteValueMessageFormat));
    public static string CopyPathMenuItem => GetString(nameof(CopyPathMenuItem));
    public static string DeleteCompletedStatusFormat => GetString(nameof(DeleteCompletedStatusFormat));
    public static string DeleteFailedMessageFormat => GetString(nameof(DeleteFailedMessageFormat));
    public static string DeleteKeyMenuItem => GetString(nameof(DeleteKeyMenuItem));
    public static string DeleteListMoreItemsFormat => GetString(nameof(DeleteListMoreItemsFormat));
    public static string DeleteRegistryKeyListItemFormat => GetString(nameof(DeleteRegistryKeyListItemFormat));
    public static string DeleteRegistryValueListItemFormat => GetString(nameof(DeleteRegistryValueListItemFormat));
    public static string DeleteSelectedButton => GetString(nameof(DeleteSelectedButton));
    public static string DeleteValueMenuItem => GetString(nameof(DeleteValueMenuItem));
    public static string ErrorMessageColumn => GetString(nameof(ErrorMessageColumn));
    public static string FullPathColumn => GetString(nameof(FullPathColumn));
    public static string HiveColumn => GetString(nameof(HiveColumn));
    public static string MatchTypeColumn => GetString(nameof(MatchTypeColumn));
    public static string PathCopiedStatus => GetString(nameof(PathCopiedStatus));
    public static string ReadyStatus => GetString(nameof(ReadyStatus));
    public static string RegistryIoError => GetString(nameof(RegistryIoError));
    public static string RegistryKeyNotFoundError => GetString(nameof(RegistryKeyNotFoundError));
    public static string RegistryValueNotSelectedError => GetString(nameof(RegistryValueNotSelectedError));
    public static string ResultCountFormat => GetString(nameof(ResultCountFormat));
    public static string SearchButton => GetString(nameof(SearchButton));
    public static string SearchCompletedStatusFormat => GetString(nameof(SearchCompletedStatusFormat));
    public static string SearchFailedMessage => GetString(nameof(SearchFailedMessage));
    public static string SearchFailedStatus => GetString(nameof(SearchFailedStatus));
    public static string SearchingStatus => GetString(nameof(SearchingStatus));
    public static string SearchStoppedStatusFormat => GetString(nameof(SearchStoppedStatusFormat));
    public static string StopButton => GetString(nameof(StopButton));
    public static string StoppingStatus => GetString(nameof(StoppingStatus));
    public static string UnhandledErrorMessage => GetString(nameof(UnhandledErrorMessage));
    public static string ValueDataColumn => GetString(nameof(ValueDataColumn));
    public static string ValueKindColumn => GetString(nameof(ValueKindColumn));
    public static string ValueNameColumn => GetString(nameof(ValueNameColumn));

    private static string GetString(string name)
    {
        return ResourceManager.GetString(name, Culture) ?? name;
    }
}
