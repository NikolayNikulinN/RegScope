using Microsoft.Win32;
using RegScope.Constants;
using RegScope.Models;
using Serilog;

namespace RegScope.Services;

public sealed class RegistryDeleteService : IRegistryDeleteService
{
    public Task DeleteAsync(RegistryDeleteRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request.DeleteKey)
        {
            DeleteKey(request);
        }
        else
        {
            DeleteValue(request);
        }

        return Task.CompletedTask;
    }

    private static void DeleteKey(RegistryDeleteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SubKeyPath))
        {
            throw new InvalidOperationException(Resources.Strings.CannotDeleteRootKeyError);
        }

        var separatorIndex = request.SubKeyPath.LastIndexOf('\\');
        var parentPath = separatorIndex >= 0 ? request.SubKeyPath[..separatorIndex] : string.Empty;
        var keyName = separatorIndex >= 0 ? request.SubKeyPath[(separatorIndex + 1)..] : request.SubKeyPath;

        using var baseKey = RegistryKey.OpenBaseKey(request.HiveKey, request.RegistryView);
        using var parentKey = string.IsNullOrEmpty(parentPath)
            ? baseKey
            : baseKey.OpenSubKey(parentPath, writable: true);

        if (parentKey is null)
        {
            throw new IOException(Resources.Strings.RegistryKeyNotFoundError);
        }

        parentKey.DeleteSubKeyTree(keyName, throwOnMissingSubKey: false);
        Log.Information(CommonConstants.LogTemplateDeletedRegistryKey, request.HiveKey, request.SubKeyPath, request.RegistryView);
    }

    private static void DeleteValue(RegistryDeleteRequest request)
    {
        if (request.ValueName is null)
        {
            throw new InvalidOperationException(Resources.Strings.RegistryValueNotSelectedError);
        }

        using var baseKey = RegistryKey.OpenBaseKey(request.HiveKey, request.RegistryView);
        using var key = baseKey.OpenSubKey(request.SubKeyPath, writable: true);

        if (key is null)
        {
            throw new IOException(Resources.Strings.RegistryKeyNotFoundError);
        }

        var valueName = request.ValueName == CommonConstants.DefaultValueName ? string.Empty : request.ValueName;
        key.DeleteValue(valueName, throwOnMissingValue: false);
        Log.Information(CommonConstants.LogTemplateDeletedRegistryValue, request.ValueName, request.HiveKey, request.SubKeyPath, request.RegistryView);
    }
}
