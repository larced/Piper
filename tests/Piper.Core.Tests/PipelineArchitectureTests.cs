using Piper.Core.Builder;
using Piper.Core.Definition.Implementations;
using Piper.Core.Definition.Interfaces;
using Piper.Core.Elements;
using Piper.Core.Validation;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Piper.Core.Tests;

/// <summary>
/// Tests for architectural improvements to the pipeline runtime.
/// </summary>
public class PipelineArchitectureTests
{
    [Fact]
    public void Validator_DetectsIllegalFanOut_ThrowsException()
    {
        // Arrange - Try to connect one output pad to multiple input pads without using a Tee
        var builder = new PipelineBuilder("invalid-fanout");
        
        var source = SourceElement.Create("source", ct => GetNumbersAsync(1, 5, ct));
        var sink1 = SinkElement.Create<int>("sink1", x => { });
        var sink2 = SinkElement.Create<int>("sink2", x => { });

        var definition = builder
            .Add(source)
            .Add(sink1)
            .Add(sink2)
            .Link(source.Outputs[0], sink1.Inputs[0])
            .Link(source.Outputs[0], sink2.Inputs[0]) // Illegal: same output pad linked twice
            .BuildDefinition();

        var factory = new PipelineRuntimeFactory();

        // Act & Assert - Validation happens when building the runtime
        var exception = Assert.Throws<InvalidOperationException>(() => factory.Build(definition));
        Assert.Contains("Illegal fan-out", exception.Message);
        Assert.Contains("Tee element", exception.Message);
    }

    [Fact]
    public async Task Pipeline_WithDegreeOfParallelism_ProcessesCorrectly()
    {
        // Arrange
        var results = new ConcurrentBag<int>();
        var builder = new PipelineBuilder("parallel-pipeline");
        
        var source = SourceElement.Create("source", ct => GetNumbersAsync(1, 10, ct));
        // Transform with DegreeOfParallelism=3 should spawn 3 processors
        var transform = TransformElement.Create("transform", (int x) => x * 2, new ElementPolicy { DegreeOfParallelism = 3 });
        var sink = SinkElement.Create<int>("sink", x => results.Add(x));

        var definition = builder
            .Add(source)
            .Add(transform)
            .Add(sink)
            .Link(source.Outputs[0], transform.Inputs[0])
            .Link(transform.Outputs[0], sink.Inputs[0])
            .BuildDefinition();

        var factory = new PipelineRuntimeFactory();
        var runtime = factory.Build(definition);

        // Act
        await runtime.StartAsync(CancellationToken.None);
        // Integration test: wait for async processing to complete (10 items * 10ms delay each)
        await Task.Delay(1000);
        await runtime.StopAsync(CancellationToken.None);

        // Assert - All items should be processed
        Assert.Equal(10, results.Count);
        for (int i = 1; i <= 10; i++)
        {
            Assert.Contains(i * 2, results);
        }
    }

    [Fact]
    public async Task Pipeline_OnFault_CancelsGracefully()
    {
        // Arrange
        var faultEvents = new ConcurrentBag<string>();
        var builder = new PipelineBuilder("fault-pipeline");
        
        var source = SourceElement.Create("source", ct => GetNumbersAsync(1, 100, ct));
        var faultyTransform = TransformElement.Create("transform", (int x) =>
        {
            if (x == 5)
                throw new InvalidOperationException("Simulated fault");
            return x * 2;
        });
        var sink = SinkElement.Create<int>("sink", x => { });

        var definition = builder
            .Add(source)
            .Add(faultyTransform)
            .Add(sink)
            .Link(source.Outputs[0], faultyTransform.Inputs[0])
            .Link(faultyTransform.Outputs[0], sink.Inputs[0])
            .BuildDefinition();

        var factory = new PipelineRuntimeFactory();
        var runtime = factory.Build(definition);

        // Start listening to events
        var cts = new CancellationTokenSource();
        var eventTask = Task.Run(async () =>
        {
            await foreach (var evt in runtime.Bus.Events(cts.Token))
            {
                if (evt is Piper.Core.Events.ElementFaultedEvent faultedEvent)
                {
                    // ElementFaultedEvent requires non-null elementName in constructor
                    faultEvents.Add(faultedEvent.ElementName!);
                }
            }
        });

        // Act
        await runtime.StartAsync(CancellationToken.None);
        // Integration test: wait for async fault to propagate (5 items * 10ms delay each)
        await Task.Delay(1000);
        
        // The pipeline should be in Faulted state
        Assert.Equal(Piper.Core.Runtime.Interfaces.PipelineState.Faulted, runtime.State);
        
        // Assert - Fault event should be published, not thrown
        Assert.Contains("transform", faultEvents);
        
        // Cleanup
        cts.Cancel();
    }

    [Fact]
    public async Task Pipeline_DrainAsync_CompletesNaturally()
    {
        // Arrange
        var results = new ConcurrentBag<int>();
        var completed = false;
        var builder = new PipelineBuilder("drain-pipeline");
        
        var source = SourceElement.Create("source", ct => GetNumbersAsync(1, 5, ct));
        var sink = SinkElement.Create<int>("sink", x =>
        {
            results.Add(x);
            // Small delay to simulate processing
            Thread.Sleep(10);
        });

        var definition = builder
            .Add(source)
            .Add(sink)
            .Link(source.Outputs[0], sink.Inputs[0])
            .BuildDefinition();

        var factory = new PipelineRuntimeFactory();
        var runtime = factory.Build(definition);

        // Start listening to events
        var cts = new CancellationTokenSource();
        var eventTask = Task.Run(async () =>
        {
            await foreach (var evt in runtime.Bus.Events(cts.Token))
            {
                if (evt is Piper.Core.Events.PipelineStateChangedEvent stateEvent &&
                    stateEvent.NewState == Piper.Core.Runtime.Interfaces.PipelineState.Draining)
                {
                    completed = true;
                }
            }
        });

        // Act
        await runtime.StartAsync(CancellationToken.None);
        await runtime.DrainAsync(CancellationToken.None);

        // Assert
        Assert.True(completed, "Pipeline should have entered Draining state");
        Assert.Equal(5, results.Count); // All items should be processed
        
        // Cleanup
        cts.Cancel();
    }

    [Fact]
    public void Validator_AllowsTeeForFanOut()
    {
        // Arrange - Using a Tee element for fan-out should be valid
        var builder = new PipelineBuilder("valid-fanout-with-tee");
        
        var source = SourceElement.Create("source", ct => GetNumbersAsync(1, 5, ct));
        var tee = TeeElement.Create<int>("tee", 2);
        var sink1 = SinkElement.Create<int>("sink1", x => { });
        var sink2 = SinkElement.Create<int>("sink2", x => { });

        var definition = builder
            .Add(source)
            .Add(tee)
            .Add(sink1)
            .Add(sink2)
            .Link(source.Outputs[0], tee.Inputs[0])
            .Link(tee.Outputs[0], sink1.Inputs[0])
            .Link(tee.Outputs[1], sink2.Inputs[0])
            .BuildDefinition();

        // Act & Assert - Should not throw
        var validator = new PipelineValidator();
        validator.Validate(definition);
    }

    private static async IAsyncEnumerable<int> GetNumbersAsync(int start, int end, [EnumeratorCancellation] CancellationToken ct)
    {
        for (int i = start; i <= end; i++)
        {
            if (ct.IsCancellationRequested)
                yield break;
            yield return i;
            await Task.Delay(10, ct);
        }
    }
}
