using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using RegScope.Configuration;
using RegScope.Logger.Abstract;
using RegScope.Logger.Constants;
using RegScope.Logger.Enums;
using RegScope.Presentation.Abstract;
using RegScope.Presentation.Wrappers;
using RegScope.Views;

namespace RegScope;

/// <summary>Composes application services and manages their lifetime.</summary>
public partial class App : Application
{
    private ServiceProvider? serviceProvider;
    private ILoggerWrapper? logger;
    private IResourceWrapper? resourceWrapper;

    protected override void OnStartup(StartupEventArgs e)
    {
        serviceProvider = new ServiceCollection().AddRegScope().BuildServiceProvider(
            new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
        logger = serviceProvider.GetRequiredService<ILoggerWrapper>();
        resourceWrapper = serviceProvider.GetRequiredService<IResourceWrapper>();
        Resources[nameof(ResourceWrapper)] = resourceWrapper;
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        logger.Write(LogConstants.LogTemplateApplicationStarted);
        MainWindow = serviceProvider.GetRequiredService<MainWindow>();
        MainWindow.Show();
        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        DispatcherUnhandledException -= OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
        try
        {
            logger?.Write(LogConstants.LogTemplateApplicationExited, e.ApplicationExitCode);
            serviceProvider?.Dispose();
        }
        finally
        {
            logger?.CloseAndFlush();
            base.OnExit(e);
        }
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
    {
        logger?.Write(args.Exception, LogLevel.Fatal, LogConstants.LogTemplateUnhandledUiException);
        MessageBox.Show(resourceWrapper!.UnhandledErrorMessage, resourceWrapper.AppTitle,
                        MessageBoxButton.OK, MessageBoxImage.Error);
        args.Handled = true;
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
    {
        logger?.Write(args.ExceptionObject as Exception, LogLevel.Fatal, LogConstants.LogTemplateUnhandledApplicationException);
        logger?.CloseAndFlush();
    }
}
