using System.Windows.Threading;

namespace RegScope.Services;

/// <summary>Dispatches view model updates to the WPF dispatcher.</summary>
public sealed class UiDispatcher(Dispatcher dispatcher) : IUiDispatcher
{
    /// <inheritdoc />
    public Task InvokeAsync(Action action, CancellationToken cancellationToken) =>
        dispatcher.InvokeAsync(action, DispatcherPriority.Normal, cancellationToken).Task;
}
