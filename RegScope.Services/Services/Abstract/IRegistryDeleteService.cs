using RegScope.Models;

namespace RegScope.Services;

/// <summary>Deletes a selected registry key or value.</summary>
public interface IRegistryDeleteService
{
    /// <summary>Deletes the requested entry unless cancellation is requested.</summary>
    Task DeleteAsync(RegistryDeleteRequest request, CancellationToken cancellationToken);
}
