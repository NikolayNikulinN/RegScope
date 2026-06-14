using System.Collections.ObjectModel;
using System.IO;
using System.Security;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RegScope.Constants;
using RegScope.Models;
using RegScope.Services;
using Serilog;

namespace RegScope.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly IRegistrySearchService registrySearchService;
    private readonly IRegistryDeleteService registryDeleteService;
    private readonly Dictionary<string, RegistrySearchResultGroupViewModel> resultGroupsByKey = new(StringComparer.OrdinalIgnoreCase);
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
    private string searchStatus = Resources.Strings.ReadyStatus;

    [ObservableProperty]
    private string resultCountText = string.Format(Resources.Strings.ResultCountFormat, 0);

    public MainViewModel(IRegistrySearchService registrySearchService, IRegistryDeleteService registryDeleteService)
    {
        this.registrySearchService = registrySearchService;
        this.registryDeleteService = registryDeleteService;

        HiveOptions = new ObservableCollection<RegistryHiveOption>(
            CommonConstants.HiveOptions.Select(option => new RegistryHiveOption(option.DisplayName, option.Hive, option.IsAllRegistry)));
        SelectedHiveOption = HiveOptions.FirstOrDefault();

        Results.CollectionChanged += (_, _) => UpdateResultCount();
    }

    public ObservableCollection<RegistryHiveOption> HiveOptions { get; }

    public ObservableCollection<RegistrySearchResult> Results { get; } = [];

    public ObservableCollection<RegistrySearchResultGroupViewModel> ResultGroups { get; } = [];

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
        SearchStatus = Resources.Strings.SearchingStatus;
        Log.Information("Registry search started for text {SearchText} in scope {Scope}.", SearchText, SelectedHiveOption.DisplayName);

        try
        {
            var options = new RegistrySearchOptions(SearchText.Trim(), SelectedHiveOption);
            await Task.Run(() => registrySearchService.SearchAsync(options, AddBatchAsync, cancellationToken), cancellationToken);
            SearchStatus = string.Format(Resources.Strings.SearchCompletedStatusFormat, Results.Count);
            Log.Information("Registry search completed with {ResultCount} results.", Results.Count);
        }
        catch (OperationCanceledException)
        {
            SearchStatus = string.Format(Resources.Strings.SearchStoppedStatusFormat, Results.Count);
            Log.Information("Registry search stopped with {ResultCount} results.", Results.Count);
        }
        catch (Exception ex)
        {
            SearchStatus = Resources.Strings.SearchFailedStatus;
            Log.Error(ex, "Registry search failed.");
            MessageBox.Show(Resources.Strings.SearchFailedMessage, Resources.Strings.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
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
        SearchStatus = Resources.Strings.StoppingStatus;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteSelected))]
    private async Task DeleteSelectedAsync()
    {
        var operations = CreateSelectedDeleteOperations();
        if (operations.Count == 0)
        {
            return;
        }

        var message = string.Format(Resources.Strings.ConfirmDeleteSelectionListMessage, FormatDeleteList(operations));
        if (!Confirm(message, Resources.Strings.DeleteSelectedButton, MessageBoxImage.Warning))
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
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            foreach (var result in batch)
            {
                Results.Add(result);
                AddResultToGroup(result);
            }
        });

        cancellationToken.ThrowIfCancellationRequested();
    }

    private void AddResultToGroup(RegistrySearchResult result)
    {
        var groupInfo = CreateGroupInfo(result);
        var key = CreateGroupKey(result.HiveKey, result.RegistryView, groupInfo.SubKeyPath);

        if (!resultGroupsByKey.TryGetValue(key, out var group))
        {
            group = new RegistrySearchResultGroupViewModel(
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

        group.Items.Add(new RegistrySearchResultItemViewModel(result, group, CreateItemDisplayPath(result), NotifySelectionChanged));
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
            string.Format(Resources.Strings.DeleteRegistryKeyListItemFormat, group.FullPath),
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

                Log.Warning(ex, "Unable to delete registry item {DisplayText}.", operation.DisplayText);
            }
        }

        RemoveDeletedOperations(deletedOperations);
        SearchStatus = string.Format(Resources.Strings.DeleteCompletedStatusFormat, deletedOperations.Count, operations.Count);

        var failed = operations.Count - deletedOperations.Count;
        if (failed > 0)
        {
            MessageBox.Show(
                string.Format(Resources.Strings.DeleteFailedMessageFormat, failed),
                Resources.Strings.AppTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
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

    private void RemoveItem(RegistrySearchResultItemViewModel item)
    {
        Results.Remove(item.Result);
        item.Parent.Items.Remove(item);
        item.Parent.RefreshSelectionFromChildren();

        if (item.Parent.Items.Count == 0)
        {
            RemoveGroup(item.Parent);
        }
    }

    private void RemoveGroup(RegistrySearchResultGroupViewModel group)
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
        ResultCountText = string.Format(Resources.Strings.ResultCountFormat, Results.Count);
    }

    private void NotifySelectionChanged()
    {
        DeleteSelectedCommand.NotifyCanExecuteChanged();
    }

    private static DeleteOperation CreateItemDeleteOperation(RegistrySearchResultItemViewModel item)
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
                ? string.Format(Resources.Strings.DeleteRegistryKeyListItemFormat, item.Result.FullPath)
                : string.Format(Resources.Strings.DeleteRegistryValueListItemFormat, item.Result.FullPath, item.Result.ValueName),
            null,
            item);
    }

    private static RegistryDeleteRequest CreateGroupDeleteRequest(RegistrySearchResultGroupViewModel group)
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
            return result.ValueName ?? CommonConstants.DefaultValueName;
        }

        return GetLastRegistryPathPart(result.FullPath);
    }

    private static string CreateGroupKey(Microsoft.Win32.RegistryHive hive, Microsoft.Win32.RegistryView view, string subKeyPath)
    {
        return $"{hive}|{view}|{subKeyPath}";
    }

    private static string FormatDeleteList(IReadOnlyList<DeleteOperation> operations)
    {
        const int maxVisibleItems = 25;
        var visibleItems = operations.Take(maxVisibleItems).Select(operation => operation.DisplayText);
        var list = string.Join(Environment.NewLine, visibleItems);
        var hiddenCount = operations.Count - maxVisibleItems;

        return hiddenCount > 0
            ? list + Environment.NewLine + string.Format(Resources.Strings.DeleteListMoreItemsFormat, hiddenCount)
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

    private static bool IsCoveredBySelectedGroup(RegistrySearchResult result, IReadOnlyCollection<RegistrySearchResultGroupViewModel> selectedGroups)
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

    private static bool Confirm(string message, string caption, MessageBoxImage icon)
    {
        return MessageBox.Show(message, caption, MessageBoxButton.YesNo, icon, MessageBoxResult.No) == MessageBoxResult.Yes;
    }

    private static bool IsDeletionException(Exception exception)
    {
        return exception is UnauthorizedAccessException or SecurityException or IOException or InvalidOperationException;
    }

    private static string ToUserFriendlyDeleteError(Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException => Resources.Strings.AccessDeniedDeleteError,
            SecurityException => Resources.Strings.AccessDeniedDeleteError,
            IOException => Resources.Strings.RegistryIoError,
            _ => exception.Message
        };
    }

    private sealed record DeleteOperation(
        RegistryDeleteRequest Request,
        string DisplayText,
        RegistrySearchResultGroupViewModel? Group,
        RegistrySearchResultItemViewModel? Item);

    private sealed record GroupInfo(string FullPath, string SubKeyPath);
}
