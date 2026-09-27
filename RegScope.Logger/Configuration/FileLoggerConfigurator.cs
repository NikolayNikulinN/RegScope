using RegScope.Logger.Constants;
using Serilog;
using Serilog.Events;
using System.Globalization;

namespace RegScope.Logger.Configuration;

internal static class FileLoggerConfigurator
{
    private static readonly object SyncRoot = new();
    private static bool isConfigured;
    private static string? logDirectoryPath;

    public static ILogger GetLogger()
    {
        EnsureConfigured();
        return Log.Logger;
    }

    public static void CloseAndFlush()
    {
        lock (SyncRoot)
        {
            Log.CloseAndFlush();
            isConfigured = false;
            logDirectoryPath = null;
        }
    }

    private static void EnsureConfigured()
    {
        if (isConfigured)
        {
            return;
        }

        lock (SyncRoot)
        {
            if (isConfigured)
            {
                return;
            }

            logDirectoryPath = ResolveLogDirectoryPath();
            var logFilePathPattern = Path.Combine(logDirectoryPath, LogConstants.LogFileName);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(
                    logFilePathPattern,
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    outputTemplate: LogConstants.OutputTemplate,
                    formatProvider: CultureInfo.InvariantCulture,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: LogConstants.RetainedFileCountLimit)
                .CreateLogger();

            isConfigured = true;
        }
    }

    private static string ResolveLogDirectoryPath()
    {
        var applicationLogDirectory = Path.Combine(AppContext.BaseDirectory, LogConstants.LogDirectoryName);

        if (CanWriteToDirectory(applicationLogDirectory))
        {
            return applicationLogDirectory;
        }

        var localApplicationDataLogDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            LogConstants.ApplicationFolderName,
            LogConstants.LogDirectoryName);

        Directory.CreateDirectory(localApplicationDataLogDirectory);
        return localApplicationDataLogDirectory;
    }

    private static bool CanWriteToDirectory(string directoryPath)
    {
        try
        {
            Directory.CreateDirectory(directoryPath);
            var probeFileName = string.Format(
                CultureInfo.InvariantCulture,
                LogConstants.WriteProbeFileNameFormat,
                Guid.NewGuid());
            var probeFilePath = Path.Combine(directoryPath, probeFileName);

            using (File.Create(probeFilePath))
            {
            }

            File.Delete(probeFilePath);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }
}
