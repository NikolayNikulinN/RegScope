namespace RegScope.Presentation.Abstract;

/// <summary>Provides localized application strings.</summary>
public interface IResourceWrapper
{
    /// <summary>Gets the localized AccessDeniedDeleteError resource.</summary>
    string AccessDeniedDeleteError { get; }

    /// <summary>Gets the localized AccessDeniedError resource.</summary>
    string AccessDeniedError { get; }

    /// <summary>Gets the localized AppTitle resource.</summary>
    string AppTitle { get; }

    /// <summary>Gets the localized MinimizeToolTip resource.</summary>
    string MinimizeToolTip { get; }

    /// <summary>Gets the localized MaximizeToolTip resource.</summary>
    string MaximizeToolTip { get; }

    /// <summary>Gets the localized RestoreToolTip resource.</summary>
    string RestoreToolTip { get; }

    /// <summary>Gets the localized CloseToolTip resource.</summary>
    string CloseToolTip { get; }

    /// <summary>Gets the localized CloseBlockedToolTip resource.</summary>
    string CloseBlockedToolTip { get; }

    /// <summary>Gets the localized CanDeleteKeyColumn resource.</summary>
    string CanDeleteKeyColumn { get; }

    /// <summary>Gets the localized CannotDeleteRootKeyError resource.</summary>
    string CannotDeleteRootKeyError { get; }

    /// <summary>Gets the localized ConfirmDeleteKeyMessageFormat resource.</summary>
    string ConfirmDeleteKeyMessageFormat { get; }

    /// <summary>Gets the localized ConfirmDeleteSelectedMessageFormat resource.</summary>
    string ConfirmDeleteSelectedMessageFormat { get; }

    /// <summary>Gets the localized ConfirmDeleteSelectionListMessage resource.</summary>
    string ConfirmDeleteSelectionListMessage { get; }

    /// <summary>Gets the localized ConfirmDeleteValueMessageFormat resource.</summary>
    string ConfirmDeleteValueMessageFormat { get; }

    /// <summary>Gets the localized ConfirmStopSearchMessage resource.</summary>
    string ConfirmStopSearchMessage { get; }

    /// <summary>Gets the localized CopyPathMenuItem resource.</summary>
    string CopyPathMenuItem { get; }

    /// <summary>Gets the localized DeleteCompletedStatusFormat resource.</summary>
    string DeleteCompletedStatusFormat { get; }

    /// <summary>Gets the localized DeleteFailedMessageFormat resource.</summary>
    string DeleteFailedMessageFormat { get; }

    /// <summary>Gets the localized DeleteKeyMenuItem resource.</summary>
    string DeleteKeyMenuItem { get; }

    /// <summary>Gets the localized DeleteListMoreItemsFormat resource.</summary>
    string DeleteListMoreItemsFormat { get; }

    /// <summary>Gets the localized DeleteRegistryKeyListItemFormat resource.</summary>
    string DeleteRegistryKeyListItemFormat { get; }

    /// <summary>Gets the localized DeleteRegistryValueListItemFormat resource.</summary>
    string DeleteRegistryValueListItemFormat { get; }

    /// <summary>Gets the localized DeleteSelectedButton resource.</summary>
    string DeleteSelectedButton { get; }

    /// <summary>Gets the localized DeleteValueMenuItem resource.</summary>
    string DeleteValueMenuItem { get; }

    /// <summary>Gets the localized ErrorMessageColumn resource.</summary>
    string ErrorMessageColumn { get; }

    /// <summary>Gets the localized FullPathColumn resource.</summary>
    string FullPathColumn { get; }

    /// <summary>Gets the localized HiveColumn resource.</summary>
    string HiveColumn { get; }

    /// <summary>Gets the localized MatchTypeColumn resource.</summary>
    string MatchTypeColumn { get; }

    /// <summary>Gets the localized PathCopiedStatus resource.</summary>
    string PathCopiedStatus { get; }

    /// <summary>Gets the localized ReadyStatus resource.</summary>
    string ReadyStatus { get; }

    /// <summary>Gets the localized RegistryIoError resource.</summary>
    string RegistryIoError { get; }

    /// <summary>Gets the localized RegistryKeyNotFoundError resource.</summary>
    string RegistryKeyNotFoundError { get; }

    /// <summary>Gets the localized RegistryValueNotSelectedError resource.</summary>
    string RegistryValueNotSelectedError { get; }

    /// <summary>Gets the localized ResultCountFormat resource.</summary>
    string ResultCountFormat { get; }

    /// <summary>Gets the localized SearchButton resource.</summary>
    string SearchButton { get; }

    /// <summary>Gets the localized SearchCompletedStatusFormat resource.</summary>
    string SearchCompletedStatusFormat { get; }

    /// <summary>Gets the localized SearchFailedMessage resource.</summary>
    string SearchFailedMessage { get; }

    /// <summary>Gets the localized SearchFailedStatus resource.</summary>
    string SearchFailedStatus { get; }

    /// <summary>Gets the localized SearchingStatus resource.</summary>
    string SearchingStatus { get; }

    /// <summary>Gets the localized SearchStoppedStatusFormat resource.</summary>
    string SearchStoppedStatusFormat { get; }

    /// <summary>Gets the localized SearchWatermark resource.</summary>
    string SearchWatermark { get; }

    /// <summary>Gets the localized StopButton resource.</summary>
    string StopButton { get; }

    /// <summary>Gets the localized StoppingStatus resource.</summary>
    string StoppingStatus { get; }

    /// <summary>Gets the localized UnhandledErrorMessage resource.</summary>
    string UnhandledErrorMessage { get; }

    /// <summary>Gets the localized ValueDataColumn resource.</summary>
    string ValueDataColumn { get; }

    /// <summary>Gets the localized ValueKindColumn resource.</summary>
    string ValueKindColumn { get; }

    /// <summary>Gets the localized ValueNameColumn resource.</summary>
    string ValueNameColumn { get; }

    /// <summary>Gets the localized YesButton resource.</summary>
    string YesButton { get; }

    /// <summary>Gets the localized NoButton resource.</summary>
    string NoButton { get; }

    /// <summary>Gets the localized OkButton resource.</summary>
    string OkButton { get; }
}
