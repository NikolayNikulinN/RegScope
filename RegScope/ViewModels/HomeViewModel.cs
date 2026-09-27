using RegScope.Services.Constants;
using System.Collections.ObjectModel;
using System.IO;
using System.Security;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RegScope.Logger.Constants;
using RegScope.Models;
using RegScope.Services;
using RegScope.Logger.Abstract;
using RegScope.Logger.Enums;
using RegScope.Model;
using RegScope.Presentation.Abstract;
using RegScope.Presentation.Constants;

namespace RegScope.ViewModels;

/// <summary>Coordinates the home page registry searches and selected deletions.</summary>
public sealed partial class HomeViewModel : ObservableObject
{
    private readonly ILoggerWrapper logger;
    private readonly IResourceWrapper resourceWrapper;
    private readonly IDialogService dialogService;
    private readonly IUiDispatcher uiDispatcher;

    private readonly IRegistrySearchService registrySearchService;
    private readonly IRegistryDeleteService registryDeleteService;
    private readonly Dictionary<string, RegistrySearchResultGroupModel> resultGroupsByKey = new(StringComparer.OrdinalIgnoreCase);
    private CancellationTokenSource? searchCancellationTokenSource;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    private string searchText = string.Empty;

    [ObservableProperty]
    private RegistryHiveOption? selectedHiveOption;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    [NotifyCanExecuteChangedFor(nameof(StopCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteSelectedCommand))]
    private bool isSearching;

    [ObservableProperty]
    private string searchStatus;

    [ObservableProperty]
    private string resultCountText;

    public HomeViewModel(IRegistrySearchService registrySearchService, IRegistryDeleteService registryDeleteService,
                         ILoggerWrapper logger, IResourceWrapper resourceWrapper,
                         IDialogService dialogService, IUiDispatcher uiDispatcher)
    {
        this.registrySearchService = registrySearchService;
        this.registryDeleteService = registryDeleteService;
        this.logger = logger;
        this.resourceWrapper = resourceWrapper;
        this.dialogService = dialogService;
        this.uiDispatcher = uiDispatcher;
        searchStatus = resourceWrapper.ReadyStatus;
        resultCountText = string.Format(resourceWrapper.ResultCountFormat, 0);

        HiveOptions = new ObservableCollection<RegistryHiveOption>(
            ServiceConstants.HiveOptions.Select(option => new RegistryHiveOption(option.DisplayName, option.Hive, option.IsAllRegistry)));
        SelectedHiveOption = HiveOptions.FirstOrDefault();

        Results.CollectionChanged += (_, _) => UpdateResultCount();
    }

    public ObservableCollection<RegistryHiveOption> HiveOptions { get; }

    public ObservableCollection<RegistrySearchResult> Results { get; } = [];

    public ObservableCollection<RegistrySearchResultGroupModel> ResultGroups { get; } = [];

    public bool IsIdle => !IsSearching;

    [RelayCommand(CanExecute = nameof(CanSearch))]
    private async Task SearchAsync()
    {
        if (SelectedHiveOption is null)
        {
            return;
        }

        searchCancellationTokenSource?.Dispose();
        searchCancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = searchCancellationTokenSource.Token;

        Results.Clear();
        ResultGroups.Clear();
        resultGroupsByKey.Clear();
        IsSearching = true;
        SearchStatus = resourceWrapper.SearchingStatus;
        logger.Write(LogConstants.LogTemplateRegistrySearchStarted, SearchText, SelectedHiveOption.DisplayName);

        try
        {
            var options = new RegistrySearchOptions(SearchText.Trim(), SelectedHiveOption);
            await Task.Run(() => registrySearchService.SearchAsync(options, AddBatchAsync, cancellationToken), cancellationToken);
            SearchStatus = string.Format(resourceWrapper.SearchCompletedStatusFormat, Results.Count);
            logger.Write(LogConstants.LogTemplateRegistrySearchCompleted, Results.Count);
        }
        catch (OperationCanceledException)
        {
            SearchStatus = string.Format(resourceWrapper.SearchStoppedStatusFormat, Results.Count);
            logger.Write(LogConstants.LogTemplateRegistrySearchStopped, Results.Count);
        }
        catch (Exception ex)
        {
            SearchStatus = resourceWrapper.SearchFailedStatus;
            logger.Write(ex, LogLevel.Error, LogConstants.LogTemplateRegistrySearchFailed);
            dialogService.ShowError(resourceWrapper.SearchFailedMessage, resourceWrapper.AppTitle);
        }
        finally
        {
            IsSearching = false;
            searchCancellationTokenSource?.Dispose();
            searchCancellationTokenSource = null;
        }
    }

    [RelayCommand(CanExecute = nameof(CanStop))]
    private void Stop()
    {
        searchCancellationTokenSource?.Cancel();
        SearchStatus = resourceWrapper.StoppingStatus;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteSelected))]
    private async Task DeleteSelectedAsync()
    {
        var operations = CreateSelectedDeleteOperations();
        if (operations.Count == 0)
        {
            return;
        }

        var message = string.Format(resourceWrapper.ConfirmDeleteSelectionListMessage, FormatDeleteList(operations));
        if (!dialogService.Confirm(message, resourceWrapper.DeleteSelectedButton))
        {
            return;
        }

        await ExecuteDeleteOperationsAsync(operations);
    }

    partial void OnIsSearchingChanged(bool value)
    {
        OnPropertyChanged(nameof(IsIdle));
    }

    private async Task AddBatchAsync(IReadOnlyList<RegistrySearchResult> batch, CancellationToken cancellationToken)
    {
        await uiDispatcher.InvokeAsync(() =>
        {
            foreach (var result in batch)
            {
                Results.Add(result);
                AddResultToGroup(result);
            }
        }, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
    }

    private void AddResultToGroup(RegistrySearchResult result)
    {
        var groupInfo = CreateGroupInfo(result);
        var key = CreateGroupKey(result.HiveKey, result.RegistryView, groupInfo.SubKeyPath);

        if (!resultGroupsByKey.TryGetValue(key, out var group))
        {
            group = new RegistrySearchResultGroupModel(
                result.Hive,
                groupInfo.FullPath,
                result.HiveKey,
                result.RegistryView,
                groupInfo.SubKeyPath,
                !string.IsNullOrWhiteSpace(groupInfo.SubKeyPath),
                NotifySelectionChanged);
            resultGroupsByKey.Add(key, group);
            ResultGroups.Add(group);
        }

        group.Items.Add(new RegistrySearchResultItemModel(result, group, CreateItemDisplayPath(result), NotifySelectionChanged));
        group.RefreshSelectionFromChildren();
    }

    private IReadOnlyList<DeleteOperation> CreateSelectedDeleteOperations()
    {
        var selectedGroups = ResultGroups
            .Where(group => group.IsSelected == true && group.CanDeleteKey)
            .ToArray();

        var operations = new List<DeleteOperation>();
        operations.AddRange(selectedGroups.Select(group => new DeleteOperation(
            CreateGroupDeleteRequest(group),
            string.Format(resourceWrapper.DeleteRegistryKeyListItemFormat, group.FullPath),
            group,
            null)));

        var selectedItems = ResultGroups
            .SelectMany(group => group.Items)
            .Where(item => item.IsSelected)
            .Where(item => !IsCoveredBySelectedGroup(item.Result, selectedGroups));

        operations.AddRange(selectedItems.Select(CreateItemDeleteOperation));

        return RemoveOperationsCoveredByKeyDeletes(operations);
    }

    private async Task ExecuteDeleteOperationsAsync(IReadOnlyList<DeleteOperation> operations)
    {
        var deletedOperations = new List<DeleteOperation>();

        foreach (var operation in operations)
        {
            try
            {
                await registryDeleteService.DeleteAsync(operation.Request, CancellationToken.None);
                deletedOperations.Add(operation);
            }
            catch (Exception ex) when (IsDeletionException(ex))
            {
                var errorMessage = ToUserFriendlyDeleteError(ex);
                if (operation.Group is not null)
                {
                    operation.Group.ErrorMessage = errorMessage;
                }

                if (operation.Item is not null)
                {
                    operation.Item.Result.ErrorMessage = errorMessage;
                    operation.Item.RefreshErrorMessage();
                }

                logger.Write(ex, LogLevel.Warning, LogConstants.LogTemplateUnableToDeleteRegistryItem, operation.DisplayText);
            }
        }

        RemoveDeletedOperations(deletedOperations);
        SearchStatus = string.Format(resourceWrapper.DeleteCompletedStatusFormat, deletedOperations.Count, operations.Count);

        var failed = operations.Count - deletedOperations.Count;
        if (failed > 0)
        {
            dialogService.ShowWarning(
                string.Format(resourceWrapper.DeleteFailedMessageFormat, failed),
                resourceWrapper.AppTitle);
        }

        NotifySelectionChanged();
        UpdateResultCount();
    }

    private void RemoveDeletedOperations(IReadOnlyList<DeleteOperation> deletedOperations)
    {
        foreach (var operation in deletedOperations)
        {
            if (operation.Request.DeleteKey)
            {
                RemoveDeletedKey(operation.Request);
            }
            else if (operation.Item is not null)
            {
                RemoveItem(operation.Item);
            }
        }
    }

    private void RemoveDeletedKey(RegistryDeleteRequest request)
    {
        foreach (var group in ResultGroups.ToArray())
        {
            if (group.HiveKey == request.HiveKey
                && group.RegistryView == request.RegistryView
                && IsSameOrDescendant(group.SubKeyPath, request.SubKeyPath))
            {
                RemoveGroup(group);
            }
        }

        foreach (var group in ResultGroups.ToArray())
        {
            foreach (var item in group.Items.ToArray())
            {
                if (item.Result.HiveKey == request.HiveKey
                    && item.Result.RegistryView == request.RegistryView
                    && IsSameOrDescendant(item.Result.SubKeyPath, request.SubKeyPath))
                {
                    RemoveItem(item);
                }
            }
        }
    }

    private void RemoveItem(RegistrySearchResultItemModel item)
    {
        Results.Remove(item.Result);
        item.Parent.Items.Remove(item);
        item.Parent.RefreshSelectionFromChildren();

        if (item.Parent.Items.Count == 0)
        {
            RemoveGroup(item.Parent);
        }
    }

    private void RemoveGroup(RegistrySearchResultGroupModel group)
    {
        foreach (var item in group.Items.ToArray())
        {
            Results.Remove(item.Result);
        }

        group.Items.Clear();
        ResultGroups.Remove(group);
        resultGroupsByKey.Remove(CreateGroupKey(group.HiveKey, group.RegistryView, group.SubKeyPath));
    }

    private bool CanSearch()
    {
        return !IsSearching && !string.IsNullOrWhiteSpace(SearchText) && SelectedHiveOption is not null;
    }

    private bool CanStop()
    {
        return IsSearching;
    }

    private bool CanDeleteSelected()
    {
        return IsIdle && ResultGroups.Any(group => group.IsSelected == true || group.Items.Any(item => item.IsSelected));
    }

    private void UpdateResultCount()
    {
        ResultCountText = string.Format(resourceWrapper.ResultCountFormat, Results.Count);
    }

    private void NotifySelectionChanged()
    {
        DeleteSelectedCommand.NotifyCanExecuteChanged();
    }

    private DeleteOperation CreateItemDeleteOperation(RegistrySearchResultItemModel item)
    {
        var deleteKey = !item.Result.IsValueResult;
        return new DeleteOperation(
            new RegistryDeleteRequest
            {
                HiveKey = item.Result.HiveKey,
                RegistryView = item.Result.RegistryView,
                SubKeyPath = item.Result.SubKeyPath,
                ValueName = item.Result.ValueName,
                DeleteKey = deleteKey
            },
            deleteKey
                ? string.Format(resourceWrapper.DeleteRegistryKeyListItemFormat, item.Result.FullPath)
                : string.Format(resourceWrapper.DeleteRegistryValueListItemFormat, item.Result.FullPath, item.Result.ValueName),
            null,
            item);
    }

    private static RegistryDeleteRequest CreateGroupDeleteRequest(RegistrySearchResultGroupModel group)
    {
        return new RegistryDeleteRequest
        {
            HiveKey = group.HiveKey,
            RegistryView = group.RegistryView,
            SubKeyPath = group.SubKeyPath,
            DeleteKey = true
        };
    }

    private static GroupInfo CreateGroupInfo(RegistrySearchResult result)
    {
        if (result.IsValueResult)
        {
            return new GroupInfo(result.FullPath, result.SubKeyPath);
        }

        if (string.IsNullOrWhiteSpace(result.SubKeyPath))
        {
            return new GroupInfo(result.FullPath, string.Empty);
        }

        return new GroupInfo(GetParentRegistryPath(result.FullPath), GetParentRegistryPath(result.SubKeyPath));
    }

    private static string CreateItemDisplayPath(RegistrySearchResult result)
    {
        if (result.IsValueResult)
        {
            return result.ValueName ?? ServiceConstants.DefaultValueName;
        }

        return GetLastRegistryPathPart(result.FullPath);
    }

    private static string CreateGroupKey(Microsoft.Win32.RegistryHive hive, Microsoft.Win32.RegistryView view, string subKeyPath)
    {
        return $"{hive}|{view}|{subKeyPath}";
    }

    private string FormatDeleteList(IReadOnlyList<DeleteOperation> operations)
    {
        const int maxVisibleItems = 25;
        var visibleItems = operations.Take(maxVisibleItems).Select(operation => operation.DisplayText);
        var list = string.Join(Environment.NewLine, visibleItems);
        var hiddenCount = operations.Count - maxVisibleItems;

        return hiddenCount > 0
            ? list + Environment.NewLine + string.Format(resourceWrapper.DeleteListMoreItemsFormat, hiddenCount)
            : list;
    }

    private static IReadOnlyList<DeleteOperation> RemoveOperationsCoveredByKeyDeletes(IReadOnlyList<DeleteOperation> operations)
    {
        var keyDeletes = operations
            .Where(operation => operation.Request.DeleteKey)
            .ToArray();

        return operations
            .Where(operation => !keyDeletes.Any(keyDelete =>
                !ReferenceEquals(operation, keyDelete)
                && operation.Request.HiveKey == keyDelete.Request.HiveKey
                && operation.Request.RegistryView == keyDelete.Request.RegistryView
                && IsSameOrDescendant(operation.Request.SubKeyPath, keyDelete.Request.SubKeyPath)))
            .ToArray();
    }

    private static bool IsCoveredBySelectedGroup(RegistrySearchResult result, IReadOnlyCollection<RegistrySearchResultGroupModel> selectedGroups)
    {
        return selectedGroups.Any(group =>
            result.HiveKey == group.HiveKey
            && result.RegistryView == group.RegistryView
            && IsSameOrDescendant(result.SubKeyPath, group.SubKeyPath));
    }

    private static bool IsSameOrDescendant(string candidatePath, string parentPath)
    {
        return string.Equals(candidatePath, parentPath, StringComparison.OrdinalIgnoreCase)
            || candidatePath.StartsWith(parentPath + "\\", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetParentRegistryPath(string path)
    {
        var separatorIndex = path.LastIndexOf('\\');
        return separatorIndex >= 0 ? path[..separatorIndex] : string.Empty;
    }

    private static string GetLastRegistryPathPart(string path)
    {
        var separatorIndex = path.LastIndexOf('\\');
        return separatorIndex >= 0 ? path[(separatorIndex + 1)..] : path;
    }

    private static bool IsDeletionException(Exception exception)
    {
        return exception is UnauthorizedAccessException or SecurityException or IOException or InvalidOperationException;
    }

    private string ToUserFriendlyDeleteError(Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException => resourceWrapper.AccessDeniedDeleteError,
            SecurityException => resourceWrapper.AccessDeniedDeleteError,
            IOException => resourceWrapper.RegistryIoError,
            _ => exception.Message
        };
    }

    private sealed record DeleteOperation(
        RegistryDeleteRequest Request,
        string DisplayText,
        RegistrySearchResultGroupModel? Group,
        RegistrySearchResultItemModel? Item);

    private sealed record GroupInfo(string FullPath, string SubKeyPath);
}
