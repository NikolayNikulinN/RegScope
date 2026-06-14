using RegScope.Models;

namespace RegScope.Services;

public interface IRegistryDeleteService
{
    Task DeleteAsync(RegistryDeleteRequest request, CancellationToken cancellationToken);
}
