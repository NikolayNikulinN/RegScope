using System.Windows;

namespace RegScope.Services;

/// <summary>Displays native WPF message dialogs.</summary>
public sealed class DialogService : IDialogService
{
    /// <inheritdoc />
    public void ShowError(string message, string title) =>
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

    /// <inheritdoc />
    public void ShowWarning(string message, string title) =>
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);

    /// <inheritdoc />
    public bool Confirm(string message, string title) =>
        MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes;
}
