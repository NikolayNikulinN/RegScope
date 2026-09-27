using CommunityToolkit.Mvvm.ComponentModel;
using RegScope.Models;

namespace RegScope.Model;

public sealed partial class RegistrySearchResultItemModel : ObservableObject
{
    private readonly Action selectionChanged;
    private bool suppressParentNotification;

    [ObservableProperty]
    private bool isSelected;

    public RegistrySearchResultItemModel(
        RegistrySearchResult result,
        RegistrySearchResultGroupModel parent,
        string displayPath,
        Action selectionChanged)
    {
        Result = result;
        Parent = parent;
        DisplayPath = displayPath;
        this.selectionChanged = selectionChanged;
    }

    public RegistrySearchResult Result { get; }

    public RegistrySearchResultGroupModel Parent { get; }

    public string DisplayPath { get; }

    public string Hive => Result.Hive;

    public string FullPath => Result.FullPath;

    public RegistryMatchType MatchType => Result.MatchType;

    public string? ValueName => Result.ValueName;

    public string? ValueKind => Result.ValueKind;

    public string? ValueData => Result.ValueData;

    public bool CanDeleteKey => Result.CanDeleteKey;

    public string? ErrorMessage => Result.ErrorMessage;

    public void SetSelectedFromParent(bool value)
    {
        suppressParentNotification = true;
        IsSelected = value;
        suppressParentNotification = false;
    }

    public void RefreshErrorMessage()
    {
        OnPropertyChanged(nameof(ErrorMessage));
    }

    partial void OnIsSelectedChanged(bool value)
    {
        if (!suppressParentNotification)
        {
            Parent.RefreshSelectionFromChildren();
        }

        selectionChanged();
    }
}
