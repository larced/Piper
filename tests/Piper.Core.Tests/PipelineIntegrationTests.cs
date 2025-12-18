using Piper.Core.Builder;
using Piper.Core.Definition.Implementations;
using Piper.Core.Definition.Interfaces;
using Piper.Core.Elements;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Piper.Core.Tests;

public class PipelineIntegrationTests
{
    [Fact]
    public async Task SimpleLinearPipeline_ProcessesAllItems()
    {
        // Arrange
        var results = new ConcurrentBag<int>();
        var builder = new PipelineBuilder("linear-pipeline");
        
        var source = SourceElement.Create("source", ct => GetNumbersAsync(1, 5, ct));
        var transform = TransformElement.Create("transform", (int x) => x * 2);
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
        await Task.Delay(500); // Give it time to process
        await runtime.StopAsync(CancellationToken.None);

        // Assert
        Assert.Equal(5, results.Count);
        Assert.Contains(2, results);  // 1 * 2
        Assert.Contains(4, results);  // 2 * 2
        Assert.Contains(6, results);  // 3 * 2
        Assert.Contains(8, results);  // 4 * 2
        Assert.Contains(10, results); // 5 * 2
    }

    [Fact]
    public async Task TeeElement_DuplicatesItemsToMultipleOutputs()
    {
        // Arrange
        var results1 = new ConcurrentBag<int>();
        var results2 = new ConcurrentBag<int>();
        var builder = new PipelineBuilder("tee-pipeline");
        
        var source = SourceElement.Create("source", ct => GetNumbersAsync(1, 3, ct));
        var tee = TeeElement.Create<int>("tee", 2);
        var sink1 = SinkElement.Create<int>("sink1", x => results1.Add(x));
        var sink2 = SinkElement.Create<int>("sink2", x => results2.Add(x));

        var definition = builder
            .Add(source)
            .Add(tee)
            .Add(sink1)
            .Add(sink2)
            .Link(source.Outputs[0], tee.Inputs[0])
            .Link(tee.Outputs[0], sink1.Inputs[0])
            .Link(tee.Outputs[1], sink2.Inputs[0])
            .BuildDefinition();

        var factory = new PipelineRuntimeFactory();
        var runtime = factory.Build(definition);

        // Act
        await runtime.StartAsync(CancellationToken.None);
        await Task.Delay(500);
        await runtime.StopAsync(CancellationToken.None);

        // Assert
        Assert.Equal(3, results1.Count);
        Assert.Equal(3, results2.Count);
        Assert.Equal(results1.OrderBy(x => x), results2.OrderBy(x => x));
    }

    [Fact]
    public async Task RouterElement_RoutesItemsToCorrectOutputs()
    {
        // Arrange
        var evens = new ConcurrentBag<int>();
        var odds = new ConcurrentBag<int>();
        var builder = new PipelineBuilder("router-pipeline");
        
        var source = SourceElement.Create("source", ct => GetNumbersAsync(1, 6, ct));
        var router = RouterElement.Create<int>("router", 2, x => x % 2); // Even to 0, Odd to 1
        var evenSink = SinkElement.Create<int>("even-sink", x => evens.Add(x));
        var oddSink = SinkElement.Create<int>("odd-sink", x => odds.Add(x));

        var definition = builder
            .Add(source)
            .Add(router)
            .Add(evenSink)
            .Add(oddSink)
            .Link(source.Outputs[0], router.Inputs[0])
            .Link(router.Outputs[0], evenSink.Inputs[0])
            .Link(router.Outputs[1], oddSink.Inputs[0])
            .BuildDefinition();

        var factory = new PipelineRuntimeFactory();
        var runtime = factory.Build(definition);

        // Act
        await runtime.StartAsync(CancellationToken.None);
        await Task.Delay(500);
        await runtime.StopAsync(CancellationToken.None);

        // Assert
        Assert.Equal(3, evens.Count); // 2, 4, 6
        Assert.Equal(3, odds.Count);  // 1, 3, 5
        Assert.All(evens, x => Assert.Equal(0, x % 2));
        Assert.All(odds, x => Assert.Equal(1, x % 2));
    }

    [Fact]
    public async Task MergeElement_CombinesMultipleInputs()
    {
        // Arrange
        var results = new ConcurrentBag<int>();
        var builder = new PipelineBuilder("merge-pipeline");
        
        var source1 = SourceElement.Create("source1", ct => GetNumbersAsync(1, 3, ct));
        var source2 = SourceElement.Create("source2", ct => GetNumbersAsync(10, 12, ct));
        var merge = MergeElement.Create<int>("merge", 2, new MergePolicy { Mode = MergeMode.Interleave });
        var sink = SinkElement.Create<int>("sink", x => results.Add(x));

        var definition = builder
            .Add(source1)
            .Add(source2)
            .Add(merge)
            .Add(sink)
            .Link(source1.Outputs[0], merge.Inputs[0])
            .Link(source2.Outputs[0], merge.Inputs[1])
            .Link(merge.Outputs[0], sink.Inputs[0])
            .BuildDefinition();

        var factory = new PipelineRuntimeFactory();
        var runtime = factory.Build(definition);

        // Act
        await runtime.StartAsync(CancellationToken.None);
        await Task.Delay(500);
        await runtime.StopAsync(CancellationToken.None);

        // Assert
        Assert.Equal(6, results.Count); // 3 from each source
        Assert.Contains(1, results);
        Assert.Contains(2, results);
        Assert.Contains(3, results);
        Assert.Contains(10, results);
        Assert.Contains(11, results);
        Assert.Contains(12, results);
    }

    [Fact]
    public async Task ComplexPipeline_WithMultipleStages_ProcessesCorrectly()
    {
        // Arrange
        var results = new ConcurrentBag<string>();
        var builder = new PipelineBuilder("complex-pipeline");
        
        var source = SourceElement.Create("source", ct => GetNumbersAsync(1, 5, ct));
        var multiply = TransformElement.Create("multiply", (int x) => x * 2);
        var toString = TransformElement.Create("toString", (int x) => x.ToString());
        var sink = SinkElement.Create<string>("sink", x => results.Add(x));

        var definition = builder
            .Add(source)
            .Add(multiply)
            .Add(toString)
            .Add(sink)
            .Link(source.Outputs[0], multiply.Inputs[0])
            .Link(multiply.Outputs[0], toString.Inputs[0])
            .Link(toString.Outputs[0], sink.Inputs[0])
            .BuildDefinition();

        var factory = new PipelineRuntimeFactory();
        var runtime = factory.Build(definition);

        // Act
        await runtime.StartAsync(CancellationToken.None);
        await Task.Delay(1500); // Give enough time for all items to process through multiple stages
        await runtime.StopAsync(CancellationToken.None);

        // Assert
        Assert.Equal(5, results.Count);
        Assert.Contains("2", results);
        Assert.Contains("4", results);
        Assert.Contains("6", results);
        Assert.Contains("8", results);
        Assert.Contains("10", results);
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
