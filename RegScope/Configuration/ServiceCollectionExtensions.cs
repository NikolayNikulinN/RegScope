using Microsoft.Extensions.DependencyInjection;
using RegScope.Logger.Abstract;
using RegScope.Logger.Wrappers;
using RegScope.Presentation.Abstract;
using RegScope.Presentation.Wrappers;
using RegScope.Services;
using RegScope.ViewModels;
using RegScope.Views;

namespace RegScope.Configuration;

/// <summary>Registers the application's service graph.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Adds shared services and the main window to the container.</summary>
    public static IServiceCollection AddRegScope(this IServiceCollection services)
    {
        services.AddSingleton<ILoggerWrapper, LoggerWrapper>();
        services.AddSingleton<IResourceWrapper, ResourceWrapper>();
        services.AddSingleton<IRegistrySearchService, RegistrySearchService>();
        services.AddSingleton<IRegistryDeleteService, RegistryDeleteService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton(_ => System.Windows.Application.Current.Dispatcher);
        services.AddSingleton<IUiDispatcher, UiDispatcher>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<MainWindow>();
        return services;
    }
}
