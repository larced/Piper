using Piper.Core.Builder;
using Piper.Core.Definition.Implementations;
using Piper.Core.Definition.Interfaces;
using Piper.Core.Elements;
using Piper.Core.Events;
using System.Runtime.CompilerServices;

Console.WriteLine("=== Piper Pipeline Examples ===\n");

// Example 1: Simple Linear Pipeline
Console.WriteLine("Example 1: Simple Linear Pipeline");
Console.WriteLine("----------------------------------");
await RunSimpleLinearPipeline();
Console.WriteLine();

// Example 2: Tee Pipeline (Fan-Out)
Console.WriteLine("Example 2: Tee Pipeline (Fan-Out)");
Console.WriteLine("----------------------------------");
await RunTeePipeline();
Console.WriteLine();

// Example 3: Router Pipeline (Conditional Routing)
Console.WriteLine("Example 3: Router Pipeline (Even/Odd Router)");
Console.WriteLine("---------------------------------------------");
await RunRouterPipeline();
Console.WriteLine();

// Example 4: Merge Pipeline (Fan-In)
Console.WriteLine("Example 4: Merge Pipeline (Fan-In)");
Console.WriteLine("-----------------------------------");
await RunMergePipeline();
Console.WriteLine();

// Example 5: Complex Multi-Stage Pipeline
Console.WriteLine("Example 5: Complex Multi-Stage Pipeline");
Console.WriteLine("---------------------------------------");
await RunComplexPipeline();
Console.WriteLine();

Console.WriteLine("All examples completed!");

static async Task RunSimpleLinearPipeline()
{
    var builder = new PipelineBuilder("simple-linear");
    
    var source = SourceElement.Create("source", ct => GenerateNumbers(1, 5, ct));
    var transform = TransformElement.Create("double", (int x) => x * 2);
    var sink = SinkElement.Create<int>("print", x => Console.WriteLine($"  Result: {x}"));

    var definition = builder
        .Add(source)
        .Add(transform)
        .Add(sink)
        .Link(source.Outputs[0], transform.Inputs[0])
        .Link(transform.Outputs[0], sink.Inputs[0])
        .BuildDefinition();

    var factory = new PipelineRuntimeFactory();
    var runtime = factory.Build(definition);

    await runtime.StartAsync(CancellationToken.None);
    await Task.Delay(600);
    await runtime.StopAsync(CancellationToken.None);
    await runtime.DisposeAsync();
}

static async Task RunTeePipeline()
{
    var builder = new PipelineBuilder("tee-fanout");
    
    var source = SourceElement.Create("source", ct => GenerateNumbers(1, 3, ct));
    var tee = TeeElement.Create<int>("tee", 2);
    var sink1 = SinkElement.Create<int>("sink1", x => Console.WriteLine($"  Branch A: {x}"));
    var sink2 = SinkElement.Create<int>("sink2", x => Console.WriteLine($"  Branch B: {x}"));

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

    await runtime.StartAsync(CancellationToken.None);
    await Task.Delay(400);
    await runtime.StopAsync(CancellationToken.None);
    await runtime.DisposeAsync();
}

static async Task RunRouterPipeline()
{
    var builder = new PipelineBuilder("router-conditional");
    
    var source = SourceElement.Create("source", ct => GenerateNumbers(1, 6, ct));
    var router = RouterElement.Create<int>("even-odd-router", 2, x => x % 2);
    var evenSink = SinkElement.Create<int>("evens", x => Console.WriteLine($"  Even: {x}"));
    var oddSink = SinkElement.Create<int>("odds", x => Console.WriteLine($"  Odd: {x}"));

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

    await runtime.StartAsync(CancellationToken.None);
    await Task.Delay(700);
    await runtime.StopAsync(CancellationToken.None);
    await runtime.DisposeAsync();
}

static async Task RunMergePipeline()
{
    var builder = new PipelineBuilder("merge-fanin");
    
    var source1 = SourceElement.Create("source1", ct => GenerateNumbers(1, 3, ct));
    var source2 = SourceElement.Create("source2", ct => GenerateNumbers(100, 102, ct));
    var merge = MergeElement.Create<int>("merge", 2, 
        new MergePolicy { Mode = MergeMode.Interleave });
    var sink = SinkElement.Create<int>("sink", x => Console.WriteLine($"  Merged: {x}"));

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

    await runtime.StartAsync(CancellationToken.None);
    await Task.Delay(400);
    await runtime.StopAsync(CancellationToken.None);
    await runtime.DisposeAsync();
}

static async Task RunComplexPipeline()
{
    var builder = new PipelineBuilder("complex-multi-stage");
    
    var source = SourceElement.Create("source", ct => GenerateNumbers(1, 5, ct));
    var multiply = TransformElement.Create("multiply", (int x) => x * 3);
    var filter = TransformElement.Create<int, int?>("filter", x => x > 6 ? x : null);
    var toString = TransformElement.Create<int?, string>("format", x => 
        x.HasValue ? $"Value: {x.Value}" : "Filtered");
    var sink = SinkElement.Create<string>("print", x => Console.WriteLine($"  {x}"));

    var definition = builder
        .Add(source)
        .Add(multiply)
        .Add(filter)
        .Add(toString)
        .Add(sink)
        .Link(source.Outputs[0], multiply.Inputs[0])
        .Link(multiply.Outputs[0], filter.Inputs[0])
        .Link(filter.Outputs[0], toString.Inputs[0])
        .Link(toString.Outputs[0], sink.Inputs[0])
        .BuildDefinition();

    var factory = new PipelineRuntimeFactory();
    var runtime = factory.Build(definition);

    await runtime.StartAsync(CancellationToken.None);
    await Task.Delay(600);
    await runtime.StopAsync(CancellationToken.None);
    await runtime.DisposeAsync();
}

static async IAsyncEnumerable<int> GenerateNumbers(
    int start, 
    int end, 
    [EnumeratorCancellation] CancellationToken ct)
{
    for (int i = start; i <= end; i++)
    {
        if (ct.IsCancellationRequested)
            yield break;
        yield return i;
        await Task.Delay(50, ct);
    }
}
