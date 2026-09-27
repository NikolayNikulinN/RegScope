using System.Windows;
using RegScope.Presentation.Abstract;
using RegScope.Presentation.Constants;
using RegScope.Presentation.Controls;

namespace RegScope.Services;

/// <summary>Displays native WPF message dialogs.</summary>
public sealed class DialogService : IDialogService
{
    private readonly IResourceWrapper resourceWrapper;
    private readonly ImagePaths imagePaths;

    /// <summary>Initializes the dialog service.</summary>
    public DialogService(IResourceWrapper resourceWrapper, AppConstants appConstants)
    {
        ArgumentNullException.ThrowIfNull(resourceWrapper);
        ArgumentNullException.ThrowIfNull(appConstants);
        this.resourceWrapper = resourceWrapper;
        imagePaths = appConstants.Images;
    }

    /// <inheritdoc />
    public void ShowError(string message, string title) =>
        ShowMessage(message, title, imagePaths.ErrorPath);

    /// <inheritdoc />
    public void ShowWarning(string message, string title) =>
        ShowMessage(message, title, imagePaths.WarningPath);

    /// <inheritdoc />
    public void ShowInformation(string message, string title) =>
        ShowMessage(message, title, imagePaths.InfoPath);

    /// <inheritdoc />
    public bool Confirm(string message, string title)
    {
        var popup = new UiPopupWindow(
            message,
            title,
            resourceWrapper.YesButton,
            resourceWrapper.NoButton,
            imagePaths.WarningPath,
            imagePaths.AppIconPath,
            imagePaths.ClosePath,
            resourceWrapper.CloseToolTip);

        return ShowPopup(popup);
    }

    private void ShowMessage(string message, string title, string iconPath)
    {
        var popup = new UiPopupWindow(
            message,
            title,
            resourceWrapper.OkButton,
            null,
            iconPath,
            imagePaths.AppIconPath,
            imagePaths.ClosePath,
            resourceWrapper.CloseToolTip);
        ShowPopup(popup);
    }

    private static bool ShowPopup(UiPopupWindow popup)
    {
        var owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive)
            ?? Application.Current?.MainWindow;

        if (owner is { IsVisible: true })
        {
            popup.Owner = owner;
        }
        else
        {
            popup.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        return popup.ShowDialog() == true;
    }
}
