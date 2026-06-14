using RegScope.Models;

namespace RegScope.Services;

public interface IRegistrySearchService
{
    Task SearchAsync(
        RegistrySearchOptions options,
        Func<IReadOnlyList<RegistrySearchResult>, CancellationToken, Task> onBatchFound,
        CancellationToken cancellationToken);
}
