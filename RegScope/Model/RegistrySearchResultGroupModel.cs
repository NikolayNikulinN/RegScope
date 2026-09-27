using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32;
namespace RegScope.Model;

public sealed partial class RegistrySearchResultGroupModel : ObservableObject
{
    private readonly Action selectionChanged;
    private bool suppressChildSelection;

    [ObservableProperty]
    private bool isExpanded = true;

    [ObservableProperty]
    private bool? isSelected = false;

    [ObservableProperty]
    private string? errorMessage;

    public RegistrySearchResultGroupModel(
        string hive,
        string fullPath,
        RegistryHive hiveKey,
        RegistryView registryView,
        string subKeyPath,
        bool canDeleteKey,
        Action selectionChanged)
    {
        Hive = hive;
        FullPath = fullPath;
        HiveKey = hiveKey;
        RegistryView = registryView;
        SubKeyPath = subKeyPath;
        CanDeleteKey = canDeleteKey;
        this.selectionChanged = selectionChanged;
    }

    public string Hive { get; }

    public string FullPath { get; }

    public RegistryHive HiveKey { get; }

    public RegistryView RegistryView { get; }

    public string SubKeyPath { get; }

    public bool CanDeleteKey { get; }

    public string MatchType => "RegistryKey";

    public ObservableCollection<RegistrySearchResultItemModel> Items { get; } = [];

    public void RefreshSelectionFromChildren()
    {
        if (suppressChildSelection)
        {
            return;
        }

        IsSelected = Items.Count == 0
            ? false
            : Items.All(item => item.IsSelected)
                ? true
                : Items.Any(item => item.IsSelected)
                    ? null
                    : false;
    }

    partial void OnIsSelectedChanged(bool? value)
    {
        if (value.HasValue)
        {
            suppressChildSelection = true;
            foreach (var item in Items)
            {
                item.SetSelectedFromParent(value.Value);
            }

            suppressChildSelection = false;
        }

        selectionChanged();
    }
}
