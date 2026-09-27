using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using RegScope.ViewModels;
using Wpf.Ui.Controls;

namespace RegScope.Views;

/// <summary>Hosts the active application page and custom title bar.</summary>
public partial class MainWindow : FluentWindow
{
    private readonly MainViewModel viewModel;

    /// <summary>Initializes the main application window.</summary>
    public MainWindow(MainViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        this.viewModel = viewModel;
        InitializeComponent();
        DataContext = viewModel;
        Title = viewModel.ResourceWrapper.AppTitle;
    }

    private void TitleBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            ToggleWindowState();
            return;
        }

        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void MinimizeButtonClick(object sender, RoutedEventArgs e) =>
        WindowState = WindowState.Minimized;

    private void MaximizeRestoreButtonClick(object sender, RoutedEventArgs e) =>
        ToggleWindowState();

    private void CloseButtonClick(object sender, RoutedEventArgs e)
    {
        if (viewModel.CanCloseApplication)
        {
            Close();
        }
    }

    private void WindowStateChanged(object sender, EventArgs e) =>
        viewModel.UpdateMaximizeRestoreState(WindowState == WindowState.Maximized);

    private void WindowClosing(object? sender, CancelEventArgs e) =>
        e.Cancel = !viewModel.CanCloseApplication;

    private void ToggleWindowState() =>
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
}
