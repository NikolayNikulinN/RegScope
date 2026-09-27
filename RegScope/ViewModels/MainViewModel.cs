using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using RegScope.Presentation.Abstract;
using RegScope.Presentation.Constants;
using RegScope.Services.Constants;

namespace RegScope.ViewModels;

/// <summary>Coordinates the main window chrome and active page.</summary>
public sealed partial class MainViewModel : ObservableObject
{
    private readonly AppConstants appConstants;
    private readonly IResourceWrapper resourceWrapper;
    private readonly HomeViewModel homeViewModel;

    [ObservableProperty]
    private string maximizeRestoreIconPath;

    [ObservableProperty]
    private string maximizeRestoreToolTip;

    public MainViewModel(AppConstants appConstants, IResourceWrapper resourceWrapper, HomeViewModel homeViewModel)
    {
        ArgumentNullException.ThrowIfNull(appConstants);
        ArgumentNullException.ThrowIfNull(resourceWrapper);
        ArgumentNullException.ThrowIfNull(homeViewModel);

        this.appConstants = appConstants;
        this.resourceWrapper = resourceWrapper;
        this.homeViewModel = homeViewModel;
        ActivePage = homeViewModel;
        maximizeRestoreIconPath = appConstants.Images.MaximizePath;
        maximizeRestoreToolTip = resourceWrapper.MaximizeToolTip;
        homeViewModel.PropertyChanged += OnHomeViewModelPropertyChanged;
    }

    public object ActivePage { get; }

    public AppConstants AppConstants => appConstants;

    public IResourceWrapper ResourceWrapper => resourceWrapper;

    public bool CanCloseApplication => homeViewModel.IsIdle;

    public string CloseButtonToolTip => CanCloseApplication
        ? resourceWrapper.CloseToolTip
        : resourceWrapper.CloseBlockedToolTip;

    /// <summary>Updates the maximize or restore icon for the current window state.</summary>
    public void UpdateMaximizeRestoreState(bool isMaximized)
    {
        MaximizeRestoreIconPath = isMaximized ? appConstants.Images.RestorePath : appConstants.Images.MaximizePath;
        MaximizeRestoreToolTip = isMaximized ? resourceWrapper.RestoreToolTip : resourceWrapper.MaximizeToolTip;
    }

    private void OnHomeViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(HomeViewModel.IsIdle))
        {
            OnPropertyChanged(nameof(CanCloseApplication));
            OnPropertyChanged(nameof(CloseButtonToolTip));
        }
    }
}
