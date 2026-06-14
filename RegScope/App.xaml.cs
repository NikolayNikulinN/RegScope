using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using RegScope.Constants;
using RegScope.Services;
using RegScope.ViewModels;
using RegScope.Views;
using Serilog;

namespace RegScope;

public partial class App : Application
{
    private ServiceProvider? serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        ConfigureLogging();
        serviceProvider = ConfigureServices();

        DispatcherUnhandledException += (_, args) =>
        {
            Log.Fatal(args.Exception, "Unhandled UI exception.");
            MessageBox.Show(
                global::RegScope.Resources.Strings.UnhandledErrorMessage,
                global::RegScope.Resources.Strings.AppTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            args.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            Log.Fatal(args.ExceptionObject as Exception, "Unhandled application exception.");

        Log.Information("RegScope started.");
        MainWindow = serviceProvider.GetRequiredService<MainWindow>();
        MainWindow.Show();

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("RegScope exited with code {ExitCode}.", e.ApplicationExitCode);
        serviceProvider?.Dispose();
        Log.CloseAndFlush();
        base.OnExit(e);
    }

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IRegistrySearchService, RegistrySearchService>();
        services.AddSingleton<IRegistryDeleteService, RegistryDeleteService>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<MainWindow>();

        return services.BuildServiceProvider();
    }

    private static void ConfigureLogging()
    {
        Directory.CreateDirectory(CommonConstants.LogDirectory);
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                Path.Combine(CommonConstants.LogDirectory, CommonConstants.LogFileNamePattern),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: CommonConstants.RetainedLogFileCount)
            .CreateLogger();
    }
}
