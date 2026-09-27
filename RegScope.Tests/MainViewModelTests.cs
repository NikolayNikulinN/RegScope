using FluentAssertions;
using Microsoft.Win32;
using NSubstitute;
using RegScope.Logger.Abstract;
using RegScope.Logger.Constants;
using RegScope.Logger.Enums;
using RegScope.Models;
using RegScope.Presentation.Wrappers;
using RegScope.Services;
using RegScope.ViewModels;
using Xunit;

namespace RegScope.Tests;

public sealed class HomeViewModelTests
{
    private readonly IRegistrySearchService search = Substitute.For<IRegistrySearchService>();
    private readonly IRegistryDeleteService delete = Substitute.For<IRegistryDeleteService>();
    private readonly ILoggerWrapper logger = Substitute.For<ILoggerWrapper>();
    private readonly IDialogService dialogs = Substitute.For<IDialogService>();
    private readonly ResourceWrapper resources = new();

    [Fact]
    public async Task Search_BatchReturned_UpdatesResultsAndCompletionStatus()
    {
        var result = CreateResult();
        search.SearchAsync(Arg.Any<RegistrySearchOptions>(), Arg.Any<Func<IReadOnlyList<RegistrySearchResult>, CancellationToken, Task>>(), Arg.Any<CancellationToken>())
              .Returns(call => call.Arg<Func<IReadOnlyList<RegistrySearchResult>, CancellationToken, Task>>()!([result], call.Arg<CancellationToken>()));
        var viewModel = CreateViewModel();

        await viewModel.SearchCommand.ExecuteAsync(null);

        viewModel.Results.Should().ContainSingle().Which.Should().BeSameAs(result);
        viewModel.SearchStatus.Should().Be(string.Format(resources.SearchCompletedStatusFormat, 1));
        viewModel.IsSearching.Should().BeFalse();
    }

    [Fact]
    public async Task Search_Failure_UsesInjectedDialogAndExceptionLogger()
    {
        var exception = new InvalidOperationException("test failure");
        search.SearchAsync(Arg.Any<RegistrySearchOptions>(), Arg.Any<Func<IReadOnlyList<RegistrySearchResult>, CancellationToken, Task>>(), Arg.Any<CancellationToken>())
              .Returns(Task.FromException(exception));
        var viewModel = CreateViewModel();

        await viewModel.SearchCommand.ExecuteAsync(null);

        viewModel.SearchStatus.Should().Be(resources.SearchFailedStatus);
        viewModel.IsSearching.Should().BeFalse();
        dialogs.Received(1).ShowError(resources.SearchFailedMessage, resources.AppTitle);
        logger.Received(1).Write(exception, LogLevel.Error, LogConstants.LogTemplateRegistrySearchFailed);
    }

    [Fact]
    public async Task Search_Cancelled_ReportsStoppedWithoutErrorDialog()
    {
        search.SearchAsync(Arg.Any<RegistrySearchOptions>(), Arg.Any<Func<IReadOnlyList<RegistrySearchResult>, CancellationToken, Task>>(), Arg.Any<CancellationToken>())
              .Returns(Task.FromCanceled(new CancellationToken(true)));
        var viewModel = CreateViewModel();

        await viewModel.SearchCommand.ExecuteAsync(null);

        viewModel.SearchStatus.Should().Be(string.Format(resources.SearchStoppedStatusFormat, 0));
        viewModel.IsSearching.Should().BeFalse();
        dialogs.DidNotReceiveWithAnyArgs().ShowError(default!, default!);
    }

    [Fact]
    public async Task Delete_ConfirmationDeclined_DoesNotCallRegistryService()
    {
        search.SearchAsync(Arg.Any<RegistrySearchOptions>(), Arg.Any<Func<IReadOnlyList<RegistrySearchResult>, CancellationToken, Task>>(), Arg.Any<CancellationToken>())
              .Returns(call => call.Arg<Func<IReadOnlyList<RegistrySearchResult>, CancellationToken, Task>>()!([CreateResult()], call.Arg<CancellationToken>()));
        var viewModel = CreateViewModel();
        await viewModel.SearchCommand.ExecuteAsync(null);
        viewModel.ResultGroups[0].Items[0].IsSelected = true;
        dialogs.Confirm(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        await viewModel.DeleteSelectedCommand.ExecuteAsync(null);

        await delete.DidNotReceiveWithAnyArgs().DeleteAsync(default!, TestContext.Current.CancellationToken);
        viewModel.Results.Should().ContainSingle();
    }

    private HomeViewModel CreateViewModel() => new(search, delete, logger, resources, dialogs, new ImmediateDispatcher()) { SearchText = "test" };

    private static RegistrySearchResult CreateResult() => new()
    {
        Hive = "HKEY_CURRENT_USER", FullPath = @"HKEY_CURRENT_USER\Software\Test",
        SubKeyPath = @"Software\Test", HiveKey = RegistryHive.CurrentUser,
        RegistryView = RegistryView.Default, MatchType = RegistryMatchType.ValueName,
        ValueName = "test", CanDeleteKey = true
    };
}

internal sealed class ImmediateDispatcher : IUiDispatcher
{
    public Task InvokeAsync(Action action, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        action();
        return Task.CompletedTask;
    }
}
