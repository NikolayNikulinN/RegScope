using Microsoft.Win32;
using RegScope.Constants;
using RegScope.Models;
using Serilog;
using System.Collections;
using System.Globalization;
using System.Security;

namespace RegScope.Services;

public sealed class RegistrySearchService : IRegistrySearchService
{
    public async Task SearchAsync(
        RegistrySearchOptions options,
        Func<IReadOnlyList<RegistrySearchResult>, CancellationToken, Task> onBatchFound,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(options.SearchText);

        var batch = new List<RegistrySearchResult>(CommonConstants.SearchBatchSize);
        RegistryView[] views = Environment.Is64BitOperatingSystem
            ? [RegistryView.Registry64, RegistryView.Registry32]
            : [RegistryView.Default];

        foreach (var hive in GetHives(options.HiveOption))
        {
            foreach (var view in views)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await SearchHiveAsync(hive, view, options.SearchText, batch, onBatchFound, cancellationToken);
            }
        }

        if (batch.Count > 0)
        {
            await FlushBatchAsync(batch, onBatchFound, cancellationToken);
        }
    }

    private static IEnumerable<RegistryHiveOption> GetHives(RegistryHiveOption selectedOption)
    {
        if (!selectedOption.IsAllRegistry)
        {
            yield return selectedOption;
            yield break;
        }

        foreach (var hive in CommonConstants.HiveOptions.Where(option => !option.IsAllRegistry))
        {
            yield return new RegistryHiveOption(hive.DisplayName, hive.Hive, hive.IsAllRegistry);
        }
    }

    private static async Task SearchHiveAsync(
        RegistryHiveOption hiveOption,
        RegistryView view,
        string searchText,
        List<RegistrySearchResult> batch,
        Func<IReadOnlyList<RegistrySearchResult>, CancellationToken, Task> onBatchFound,
        CancellationToken cancellationToken)
    {
        if (hiveOption.Hive is not { } hive)
        {
            return;
        }

        try
        {
            using var root = RegistryKey.OpenBaseKey(hive, view);
            await SearchKeyAsync(root, hiveOption.DisplayName, hiveOption.DisplayName, string.Empty, hive, view, searchText, batch, onBatchFound, cancellationToken);
        }
        catch (Exception ex) when (IsExpectedRegistryException(ex))
        {
            Log.Warning(ex, CommonConstants.LogTemplateUnableToOpenRegistryHive, hiveOption.DisplayName, view);
            batch.Add(CreateErrorResult(hiveOption.DisplayName, hiveOption.DisplayName, string.Empty, hive, view, ex));
            await FlushIfNeededAsync(batch, onBatchFound, cancellationToken);
        }
    }

    private static async Task SearchKeyAsync(
        RegistryKey key,
        string hiveName,
        string fullPath,
        string subKeyPath,
        RegistryHive hive,
        RegistryView view,
        string searchText,
        List<RegistrySearchResult> batch,
        Func<IReadOnlyList<RegistrySearchResult>, CancellationToken, Task> onBatchFound,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (Contains(GetLastPathPart(fullPath), searchText))
        {
            batch.Add(new RegistrySearchResult
            {
                Hive = hiveName,
                FullPath = fullPath,
                MatchType = RegistryMatchType.KeyName,
                CanDeleteKey = !string.IsNullOrWhiteSpace(subKeyPath),
                HiveKey = hive,
                RegistryView = view,
                SubKeyPath = subKeyPath
            });
            await FlushIfNeededAsync(batch, onBatchFound, cancellationToken);
        }

        await SearchValuesAsync(key, hiveName, fullPath, subKeyPath, hive, view, searchText, batch, onBatchFound, cancellationToken);

        string[] subKeyNames;
        try
        {
            subKeyNames = key.GetSubKeyNames();
        }
        catch (Exception ex) when (IsExpectedRegistryException(ex))
        {
            Log.Warning(ex, CommonConstants.LogTemplateUnableToEnumerateRegistrySubKeys, fullPath);
            batch.Add(CreateErrorResult(hiveName, fullPath, subKeyPath, hive, view, ex));
            await FlushIfNeededAsync(batch, onBatchFound, cancellationToken);
            return;
        }

        foreach (var childName in subKeyNames)
        {
            cancellationToken.ThrowIfCancellationRequested();

            RegistryKey? child = null;
            var childSubKeyPath = CombineRegistryPath(subKeyPath, childName);
            var childFullPath = CombineRegistryPath(fullPath, childName);

            try
            {
                child = key.OpenSubKey(childName, writable: false);
            }
            catch (Exception ex) when (IsExpectedRegistryException(ex))
            {
                Log.Warning(ex, CommonConstants.LogTemplateUnableToOpenRegistryKey, childFullPath);
                batch.Add(CreateErrorResult(hiveName, childFullPath, childSubKeyPath, hive, view, ex));
                await FlushIfNeededAsync(batch, onBatchFound, cancellationToken);
            }

            if (child is null)
            {
                continue;
            }

            using (child)
            {
                await SearchKeyAsync(child, hiveName, childFullPath, childSubKeyPath, hive, view, searchText, batch, onBatchFound, cancellationToken);
            }
        }
    }

    private static async Task SearchValuesAsync(
        RegistryKey key,
        string hiveName,
        string fullPath,
        string subKeyPath,
        RegistryHive hive,
        RegistryView view,
        string searchText,
        List<RegistrySearchResult> batch,
        Func<IReadOnlyList<RegistrySearchResult>, CancellationToken, Task> onBatchFound,
        CancellationToken cancellationToken)
    {
        string[] valueNames;
        try
        {
            valueNames = key.GetValueNames();
        }
        catch (Exception ex) when (IsExpectedRegistryException(ex))
        {
            Log.Warning(ex, CommonConstants.LogTemplateUnableToEnumerateRegistryValues, fullPath);
            batch.Add(CreateErrorResult(hiveName, fullPath, subKeyPath, hive, view, ex));
            await FlushIfNeededAsync(batch, onBatchFound, cancellationToken);
            return;
        }

        foreach (var valueName in valueNames)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var rawValue = key.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
                var valueKind = key.GetValueKind(valueName).ToString();
                var formattedValue = FormatValue(rawValue);
                var displayName = string.IsNullOrEmpty(valueName) ? CommonConstants.DefaultValueName : valueName;

                if (Contains(displayName, searchText))
                {
                    batch.Add(CreateValueResult(hiveName, fullPath, subKeyPath, hive, view, valueName, valueKind, rawValue, formattedValue, RegistryMatchType.ValueName));
                    await FlushIfNeededAsync(batch, onBatchFound, cancellationToken);
                }

                if (Contains(formattedValue, searchText))
                {
                    batch.Add(CreateValueResult(hiveName, fullPath, subKeyPath, hive, view, valueName, valueKind, rawValue, formattedValue, RegistryMatchType.ValueData));
                    await FlushIfNeededAsync(batch, onBatchFound, cancellationToken);
                }
            }
            catch (Exception ex) when (IsExpectedRegistryException(ex))
            {
                Log.Warning(ex, CommonConstants.LogTemplateUnableToReadRegistryValue, valueName, fullPath);
                batch.Add(CreateErrorResult(hiveName, fullPath, subKeyPath, hive, view, ex));
                await FlushIfNeededAsync(batch, onBatchFound, cancellationToken);
            }
        }
    }

    private static RegistrySearchResult CreateValueResult(
        string hiveName,
        string fullPath,
        string subKeyPath,
        RegistryHive hive,
        RegistryView view,
        string valueName,
        string valueKind,
        object? rawValue,
        string formattedValue,
        RegistryMatchType matchType)
    {
        return new RegistrySearchResult
        {
            Hive = hiveName,
            FullPath = fullPath,
            MatchType = matchType,
            ValueName = string.IsNullOrEmpty(valueName) ? CommonConstants.DefaultValueName : valueName,
            ValueKind = valueKind,
            ValueData = Truncate(formattedValue),
            FullValueData = rawValue,
            CanDeleteKey = !string.IsNullOrWhiteSpace(subKeyPath),
            HiveKey = hive,
            RegistryView = view,
            SubKeyPath = subKeyPath
        };
    }

    private static RegistrySearchResult CreateErrorResult(
        string hiveName,
        string fullPath,
        string subKeyPath,
        RegistryHive hive,
        RegistryView view,
        Exception exception)
    {
        return new RegistrySearchResult
        {
            Hive = hiveName,
            FullPath = fullPath,
            MatchType = RegistryMatchType.KeyName,
            CanDeleteKey = !string.IsNullOrWhiteSpace(subKeyPath),
            HiveKey = hive,
            RegistryView = view,
            SubKeyPath = subKeyPath,
            ErrorMessage = ToUserFriendlyError(exception)
        };
    }

    private static bool Contains(string? source, string searchText)
    {
        return source?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static string FormatValue(object? value)
    {
        return value switch
        {
            null => string.Empty,
            string text => text,
            string[] values => string.Join("; ", values),
            byte[] bytes => BitConverter.ToString(bytes),
            IEnumerable values and not string => string.Join("; ", values.Cast<object>().Select(item => Convert.ToString(item, CultureInfo.InvariantCulture))),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty
        };
    }

    private static string Truncate(string value)
    {
        return value.Length <= CommonConstants.ValueDataDisplayLimit
            ? value
            : value[..CommonConstants.ValueDataDisplayLimit] + "...";
    }

    private static string CombineRegistryPath(string left, string right)
    {
        return string.IsNullOrEmpty(left) ? right : left + "\\" + right;
    }

    private static string GetLastPathPart(string path)
    {
        var index = path.LastIndexOf('\\');
        return index >= 0 ? path[(index + 1)..] : path;
    }

    private static async Task FlushIfNeededAsync(
        List<RegistrySearchResult> batch,
        Func<IReadOnlyList<RegistrySearchResult>, CancellationToken, Task> onBatchFound,
        CancellationToken cancellationToken)
    {
        if (batch.Count >= CommonConstants.SearchBatchSize)
        {
            await FlushBatchAsync(batch, onBatchFound, cancellationToken);
        }
    }

    private static async Task FlushBatchAsync(
        List<RegistrySearchResult> batch,
        Func<IReadOnlyList<RegistrySearchResult>, CancellationToken, Task> onBatchFound,
        CancellationToken cancellationToken)
    {
        var snapshot = batch.ToArray();
        batch.Clear();
        await onBatchFound(snapshot, cancellationToken);
    }

    private static bool IsExpectedRegistryException(Exception exception)
    {
        return exception is UnauthorizedAccessException or SecurityException or IOException;
    }

    private static string ToUserFriendlyError(Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException => Resources.Strings.AccessDeniedError,
            SecurityException => Resources.Strings.AccessDeniedError,
            IOException => Resources.Strings.RegistryIoError,
            _ => exception.Message
        };
    }
}
