using FluentAssertions;
using RegScope.Logger.Enums;
using RegScope.Logger.Wrappers;
using RegScope.Presentation.Abstract;
using RegScope.Presentation.Wrappers;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Xunit;

namespace RegScope.Tests;

public sealed class WrapperTests
{
    [Fact]
    public void Resources_AllDeclaredProperties_ResolveNonemptyStrings()
    {
        var resources = new ResourceWrapper();

        foreach (var property in typeof(IResourceWrapper).GetProperties())
        {
            var value = (string?)property.GetValue(resources);
            value.Should().NotBeNullOrWhiteSpace().And.NotBe(property.Name);
        }
    }

    [Fact]
    public void Write_ExceptionAndProperties_PreservesStructuredEvent()
    {
        var sink = new CapturingSink();
        using var serilog = new LoggerConfiguration().WriteTo.Sink(sink).CreateLogger();
        var logger = new LoggerWrapper(serilog);
        var exception = new InvalidOperationException("test failure");

        logger.Write(exception, LogLevel.Error, "Operation {OperationId} failed", 42);

        var entry = sink.Events.Should().ContainSingle().Subject;
        entry.Exception.Should().BeSameAs(exception);
        entry.Level.Should().Be(LogEventLevel.Error);
        entry.Properties["OperationId"].Should().BeEquivalentTo(new ScalarValue(42));
    }

    [Fact]
    public void Write_FatalLevel_EmitsFatalEvent()
    {
        var sink = new CapturingSink();
        using var serilog = new LoggerConfiguration().WriteTo.Sink(sink).CreateLogger();

        new LoggerWrapper(serilog).Write("fatal test", LogLevel.Fatal);

        sink.Events.Should().ContainSingle().Which.Level.Should().Be(LogEventLevel.Fatal);
    }
}

internal sealed class CapturingSink : ILogEventSink
{
    public List<LogEvent> Events { get; } = [];
    public void Emit(LogEvent logEvent) => Events.Add(logEvent);
}
