using RegScope.Models;

namespace RegScope.Services;

/// <summary>Searches registry keys and values in cancellable batches.</summary>
public interface IRegistrySearchService
{
    /// <summary>Reports matching entries through the awaited batch callback.</summary>
    Task SearchAsync(
        RegistrySearchOptions options,
        Func<IReadOnlyList<RegistrySearchResult>, CancellationToken, Task> onBatchFound,
        CancellationToken cancellationToken);
}
