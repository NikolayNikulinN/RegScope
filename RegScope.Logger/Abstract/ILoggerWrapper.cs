using RegScope.Logger.Enums;

namespace RegScope.Logger.Abstract;

/// <summary>
/// Writes application log events.
/// </summary>
public interface ILoggerWrapper
{
    /// <summary>
    /// Writes an informational message.
    /// </summary>
    /// <param name="message">Message to write.</param>
    /// <param name="propertyValues">Values for the structured message template.</param>
    void Write(string message, params object?[] propertyValues);

    /// <summary>
    /// Writes a message at the specified log level.
    /// </summary>
    /// <param name="message">Message to write.</param>
    /// <param name="level">Log event level.</param>
    void Write(string message, LogLevel level);

    /// <summary>Writes an exception and structured properties at the specified level.</summary>
    void Write(Exception? exception, LogLevel level, string messageTemplate, params object?[] propertyValues);

    /// <summary>
    /// Flushes pending log events and closes the logger.
    /// </summary>
    void CloseAndFlush();
}
