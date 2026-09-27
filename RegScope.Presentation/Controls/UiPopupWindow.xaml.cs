using System.Windows;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace RegScope.Presentation.Controls;

/// <summary>Displays a branded modal confirmation dialog.</summary>
public partial class UiPopupWindow : FluentWindow
{
    /// <summary>Creates a confirmation popup.</summary>
    public UiPopupWindow(
        string message,
        string title,
        string confirmButtonText,
        string? cancelButtonText,
        string iconPath,
        string appIconPath,
        string closeIconPath,
        string closeToolTip)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(confirmButtonText);
        ArgumentNullException.ThrowIfNull(iconPath);
        ArgumentNullException.ThrowIfNull(appIconPath);
        ArgumentNullException.ThrowIfNull(closeIconPath);
        ArgumentNullException.ThrowIfNull(closeToolTip);

        Message = message;
        PopupTitle = title;
        ConfirmButtonText = confirmButtonText;
        CancelButtonText = cancelButtonText ?? string.Empty;
        CancelButtonVisibility = string.IsNullOrWhiteSpace(cancelButtonText) ? Visibility.Collapsed : Visibility.Visible;
        IconPath = iconPath;
        AppIconPath = appIconPath;
        CloseIconPath = closeIconPath;
        CloseToolTip = closeToolTip;

        InitializeComponent();
        DataContext = this;
    }

    /// <summary>Gets the popup message.</summary>
    public string Message { get; }

    /// <summary>Gets the popup title.</summary>
    public string PopupTitle { get; }

    /// <summary>Gets the confirmation button label.</summary>
    public string ConfirmButtonText { get; }

    /// <summary>Gets the cancellation button label.</summary>
    public string CancelButtonText { get; }

    /// <summary>Gets whether the cancel button is shown.</summary>
    public Visibility CancelButtonVisibility { get; }

    /// <summary>Gets the message icon path.</summary>
    public string IconPath { get; }

    /// <summary>Gets the application icon path.</summary>
    public string AppIconPath { get; }

    /// <summary>Gets the close icon path.</summary>
    public string CloseIconPath { get; }

    /// <summary>Gets the close button tooltip.</summary>
    public string CloseToolTip { get; }

    private void TitleBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void CloseButtonClick(object sender, RoutedEventArgs e) => DialogResult = false;

    private void CancelButtonClick(object sender, RoutedEventArgs e) => DialogResult = false;

    private void ConfirmButtonClick(object sender, RoutedEventArgs e) => DialogResult = true;

    private void WindowPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            DialogResult = false;
        }
    }
}
