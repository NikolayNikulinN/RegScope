using RegScope.ViewModels;
using Wpf.Ui.Controls;

namespace RegScope.Views;

public partial class MainWindow : FluentWindow
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
