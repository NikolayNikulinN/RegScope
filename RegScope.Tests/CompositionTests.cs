using System.Runtime.ExceptionServices;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RegScope.Configuration;
using RegScope.Logger.Abstract;
using RegScope.Presentation.Abstract;
using RegScope.Presentation.Wrappers;
using RegScope.ViewModels;
using RegScope.Views;
using Xunit;

namespace RegScope.Tests;

public sealed class CompositionTests
{
    [Fact]
    public void Container_MainWindow_ResolvesWithResourcesAndViewModel()
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                var app = new App();
                app.InitializeComponent();
                using var provider = new ServiceCollection().AddRegScope().BuildServiceProvider(
                    new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
                try
                {
                    var resources = provider.GetRequiredService<IResourceWrapper>();
                    app.Resources[nameof(ResourceWrapper)] = resources;
                    var window = provider.GetRequiredService<MainWindow>();
                    window.DataContext.Should().BeOfType<MainViewModel>();
                    window.Title.Should().Be(resources.AppTitle);
                    window.Icon.Should().NotBeNull();
                    window.Close();
                }
                finally
                {
                    provider.GetRequiredService<ILoggerWrapper>().CloseAndFlush();
                    app.Shutdown();
                }
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure is not null)
        {
            ExceptionDispatchInfo.Capture(failure).Throw();
        }
    }
}
