using System.Globalization;
using RegScope.Presentation.Abstract;
using RegScope.Presentation.Resources;

namespace RegScope.Presentation.Wrappers;

/// <summary>Provides localized application resources to bindings and services.</summary>
public sealed class ResourceWrapper : IResourceWrapper
{
    /// <inheritdoc />
    public string AccessDeniedDeleteError => GetString(nameof(AccessDeniedDeleteError));

    /// <inheritdoc />
    public string AccessDeniedError => GetString(nameof(AccessDeniedError));

    /// <inheritdoc />
    public string AppTitle => GetString(nameof(AppTitle));

    /// <inheritdoc />
    public string MinimizeToolTip => GetString(nameof(MinimizeToolTip));

    /// <inheritdoc />
    public string MaximizeToolTip => GetString(nameof(MaximizeToolTip));

    /// <inheritdoc />
    public string RestoreToolTip => GetString(nameof(RestoreToolTip));

    /// <inheritdoc />
    public string CloseToolTip => GetString(nameof(CloseToolTip));

    /// <inheritdoc />
    public string CloseBlockedToolTip => GetString(nameof(CloseBlockedToolTip));

    /// <inheritdoc />
    public string CanDeleteKeyColumn => GetString(nameof(CanDeleteKeyColumn));

    /// <inheritdoc />
    public string CannotDeleteRootKeyError => GetString(nameof(CannotDeleteRootKeyError));

    /// <inheritdoc />
    public string ConfirmDeleteKeyMessageFormat => GetString(nameof(ConfirmDeleteKeyMessageFormat));

    /// <inheritdoc />
    public string ConfirmDeleteSelectedMessageFormat => GetString(nameof(ConfirmDeleteSelectedMessageFormat));

    /// <inheritdoc />
    public string ConfirmDeleteSelectionListMessage => GetString(nameof(ConfirmDeleteSelectionListMessage));

    /// <inheritdoc />
    public string ConfirmDeleteValueMessageFormat => GetString(nameof(ConfirmDeleteValueMessageFormat));

    /// <inheritdoc />
    public string ConfirmStopSearchMessage => GetString(nameof(ConfirmStopSearchMessage));

    /// <inheritdoc />
    public string CopyPathMenuItem => GetString(nameof(CopyPathMenuItem));

    /// <inheritdoc />
    public string DeleteCompletedStatusFormat => GetString(nameof(DeleteCompletedStatusFormat));

    /// <inheritdoc />
    public string DeleteFailedMessageFormat => GetString(nameof(DeleteFailedMessageFormat));

    /// <inheritdoc />
    public string DeleteKeyMenuItem => GetString(nameof(DeleteKeyMenuItem));

    /// <inheritdoc />
    public string DeleteListMoreItemsFormat => GetString(nameof(DeleteListMoreItemsFormat));

    /// <inheritdoc />
    public string DeleteRegistryKeyListItemFormat => GetString(nameof(DeleteRegistryKeyListItemFormat));

    /// <inheritdoc />
    public string DeleteRegistryValueListItemFormat => GetString(nameof(DeleteRegistryValueListItemFormat));

    /// <inheritdoc />
    public string DeleteSelectedButton => GetString(nameof(DeleteSelectedButton));

    /// <inheritdoc />
    public string DeleteValueMenuItem => GetString(nameof(DeleteValueMenuItem));

    /// <inheritdoc />
    public string ErrorMessageColumn => GetString(nameof(ErrorMessageColumn));

    /// <inheritdoc />
    public string FullPathColumn => GetString(nameof(FullPathColumn));

    /// <inheritdoc />
    public string HiveColumn => GetString(nameof(HiveColumn));

    /// <inheritdoc />
    public string MatchTypeColumn => GetString(nameof(MatchTypeColumn));

    /// <inheritdoc />
    public string PathCopiedStatus => GetString(nameof(PathCopiedStatus));

    /// <inheritdoc />
    public string ReadyStatus => GetString(nameof(ReadyStatus));

    /// <inheritdoc />
    public string RegistryIoError => GetString(nameof(RegistryIoError));

    /// <inheritdoc />
    public string RegistryKeyNotFoundError => GetString(nameof(RegistryKeyNotFoundError));

    /// <inheritdoc />
    public string RegistryValueNotSelectedError => GetString(nameof(RegistryValueNotSelectedError));

    /// <inheritdoc />
    public string ResultCountFormat => GetString(nameof(ResultCountFormat));

    /// <inheritdoc />
    public string SearchButton => GetString(nameof(SearchButton));

    /// <inheritdoc />
    public string SearchCompletedStatusFormat => GetString(nameof(SearchCompletedStatusFormat));

    /// <inheritdoc />
    public string SearchFailedMessage => GetString(nameof(SearchFailedMessage));

    /// <inheritdoc />
    public string SearchFailedStatus => GetString(nameof(SearchFailedStatus));

    /// <inheritdoc />
    public string SearchingStatus => GetString(nameof(SearchingStatus));

    /// <inheritdoc />
    public string SearchStoppedStatusFormat => GetString(nameof(SearchStoppedStatusFormat));

    /// <inheritdoc />
    public string SearchWatermark => GetString(nameof(SearchWatermark));

    /// <inheritdoc />
    public string StopButton => GetString(nameof(StopButton));

    /// <inheritdoc />
    public string StoppingStatus => GetString(nameof(StoppingStatus));

    /// <inheritdoc />
    public string UnhandledErrorMessage => GetString(nameof(UnhandledErrorMessage));

    /// <inheritdoc />
    public string ValueDataColumn => GetString(nameof(ValueDataColumn));

    /// <inheritdoc />
    public string ValueKindColumn => GetString(nameof(ValueKindColumn));

    /// <inheritdoc />
    public string ValueNameColumn => GetString(nameof(ValueNameColumn));

    /// <inheritdoc />
    public string YesButton => GetString(nameof(YesButton));

    /// <inheritdoc />
    public string NoButton => GetString(nameof(NoButton));

    /// <inheritdoc />
    public string OkButton => GetString(nameof(OkButton));

    private static string GetString(string key) => AppResource.ResourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? key;
}
