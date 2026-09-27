namespace RegScope.Services;

/// <summary>Schedules updates on the application's UI thread.</summary>
public interface IUiDispatcher
{
    /// <summary>Runs an action on the UI thread unless cancellation is requested.</summary>
    Task InvokeAsync(Action action, CancellationToken cancellationToken);
}
