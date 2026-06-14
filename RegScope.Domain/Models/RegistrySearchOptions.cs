namespace RegScope.Models;

public sealed class RegistrySearchOptions
{
    public RegistrySearchOptions(string searchText, RegistryHiveOption hiveOption)
    {
        SearchText = searchText;
        HiveOption = hiveOption;
    }

    public string SearchText { get; }

    public RegistryHiveOption HiveOption { get; }
}
