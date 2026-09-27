using Microsoft.Win32;
using RegScope.Logger.Constants;
using RegScope.Models;
using RegScope.Services.Constants;
using RegScope.Logger.Abstract;
using RegScope.Logger.Enums;
using RegScope.Presentation.Abstract;

namespace RegScope.Services;

/// <summary>Provides registry operations through injected logging and resources.</summary>
public sealed class RegistryDeleteService : IRegistryDeleteService
{
    private readonly ILoggerWrapper logger;
    private readonly IResourceWrapper resourceWrapper;

    public RegistryDeleteService(ILoggerWrapper logger, IResourceWrapper resourceWrapper)
    {
        this.logger = logger;
        this.resourceWrapper = resourceWrapper;
    }

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

    private void DeleteKey(RegistryDeleteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SubKeyPath))
        {
            throw new InvalidOperationException(resourceWrapper.CannotDeleteRootKeyError);
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
            throw new IOException(resourceWrapper.RegistryKeyNotFoundError);
        }

        parentKey.DeleteSubKeyTree(keyName, throwOnMissingSubKey: false);
        logger.Write(LogConstants.LogTemplateDeletedRegistryKey, request.HiveKey, request.SubKeyPath, request.RegistryView);
    }

    private void DeleteValue(RegistryDeleteRequest request)
    {
        if (request.ValueName is null)
        {
            throw new InvalidOperationException(resourceWrapper.RegistryValueNotSelectedError);
        }

        using var baseKey = RegistryKey.OpenBaseKey(request.HiveKey, request.RegistryView);
        using var key = baseKey.OpenSubKey(request.SubKeyPath, writable: true);

        if (key is null)
        {
            throw new IOException(resourceWrapper.RegistryKeyNotFoundError);
        }

        var valueName = request.ValueName == ServiceConstants.DefaultValueName ? string.Empty : request.ValueName;
        key.DeleteValue(valueName, throwOnMissingValue: false);
        logger.Write(LogConstants.LogTemplateDeletedRegistryValue, request.ValueName, request.HiveKey, request.SubKeyPath, request.RegistryView);
    }
}
