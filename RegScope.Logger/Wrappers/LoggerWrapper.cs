using RegScope.Logger.Abstract;
using RegScope.Logger.Configuration;
using RegScope.Logger.Enums;
using Serilog;

namespace RegScope.Logger.Wrappers;

/// <summary>
/// Adapts Serilog to the application logging abstraction.
/// </summary>
public sealed class LoggerWrapper : ILoggerWrapper
{
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggerWrapper"/> class.
    /// </summary>
    public LoggerWrapper()
        : this(FileLoggerConfigurator.GetLogger())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggerWrapper"/> class.
    /// </summary>
    /// <param name="logger">Serilog logger.</param>
    public LoggerWrapper(ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        this.logger = logger.ForContext<LoggerWrapper>();
    }

    /// <inheritdoc />
    public void Write(string message, params object?[] propertyValues) =>
        logger.Information(message, propertyValues);

    /// <inheritdoc />
    public void Write(Exception? exception, LogLevel level, string messageTemplate, params object?[] propertyValues) =>
        logger.Write(level switch
        {
            LogLevel.Info => Serilog.Events.LogEventLevel.Information,
            LogLevel.Warning => Serilog.Events.LogEventLevel.Warning,
            LogLevel.Error => Serilog.Events.LogEventLevel.Error,
            LogLevel.Fatal => Serilog.Events.LogEventLevel.Fatal,
            _ => throw new ArgumentOutOfRangeException(nameof(level))
        }, exception, messageTemplate, propertyValues);

    /// <inheritdoc />
    public void Write(string message, LogLevel level) =>
        Write(null, level, message);

    /// <inheritdoc />
    public void CloseAndFlush() =>
        FileLoggerConfigurator.CloseAndFlush();
}
