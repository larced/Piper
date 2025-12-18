# Piper

A modern, channel-based streaming pipeline framework for .NET that enables building complex, observable, and resilient data processing workflows.

## Features

- **In-process, push-based streaming** with `IAsyncEnumerable<T>` support
- **Bounded memory + backpressure by default** using bounded channels
- **Per-element parallelism** with configurable degree of parallelism
- **Static graph, dynamic flow** - fixed topology with runtime routing decisions
- **Observable + resilient** - errors surface as events, not uncontrolled exceptions
- **Type-safe** - compile-time type checking for all connections
- **Composable** - build complex pipelines from simple, reusable elements

## Core Concepts

### Elements

Elements are the building blocks of pipelines. Each element has:
- **Input pads**: Typed connection points for receiving data
- **Output pads**: Typed connection points for sending data  
- **Policy**: Configuration for parallelism and buffering
- **Processor**: The logic that runs when the pipeline executes

### Pads and Links

- **Pads** are typed ports on elements (like function parameters/returns)
- **Links** connect one output pad to one input pad
- At runtime, each link becomes a bounded `Channel<T>`

### Special Elements

- **Source**: Generates items (0 inputs, 1+ outputs)
- **Transform**: Processes items one-by-one (1 input, 1 output)
- **Sink**: Consumes items (1 input, 0 outputs)
- **Tee**: Duplicates items to multiple outputs (1 input, N outputs)
- **Router**: Routes items to one of N outputs (1 input, N outputs)
- **Merge**: Combines multiple inputs into one output (N inputs, 1 output)

## Quick Start

```csharp
using Piper.Core.Builder;
using Piper.Core.Elements;

// Create a simple linear pipeline
var builder = new PipelineBuilder("my-pipeline");

// Define elements
var source = SourceElement.Create("source", async ct =>
{
    for (int i = 1; i <= 10; i++)
    {
        yield return i;
        await Task.Delay(100, ct);
    }
});

var transform = TransformElement.Create("double", (int x) => x * 2);

var sink = SinkElement.Create<int>("print", x => Console.WriteLine($"Result: {x}"));

// Build the pipeline
var definition = builder
    .Add(source)
    .Add(transform)
    .Add(sink)
    .Link(source.Outputs[0], transform.Inputs[0])
    .Link(transform.Outputs[0], sink.Inputs[0])
    .BuildDefinition();

// Create and run the runtime
var factory = new PipelineRuntimeFactory();
var runtime = factory.Build(definition);

await runtime.StartAsync(CancellationToken.None);
await Task.Delay(2000); // Let it process
await runtime.StopAsync(CancellationToken.None);
```

## Examples

### Tee Pipeline (Fan-Out)

```csharp
var source = SourceElement.Create("source", ct => GenerateData(ct));
var tee = TeeElement.Create<int>("tee", 2);
var sink1 = SinkElement.Create<int>("sink1", x => Console.WriteLine($"Sink1: {x}"));
var sink2 = SinkElement.Create<int>("sink2", x => Console.WriteLine($"Sink2: {x}"));

var definition = builder
    .Add(source)
    .Add(tee)
    .Add(sink1)
    .Add(sink2)
    .Link(source.Outputs[0], tee.Inputs[0])
    .Link(tee.Outputs[0], sink1.Inputs[0])
    .Link(tee.Outputs[1], sink2.Inputs[0])
    .BuildDefinition();
```

### Router Pipeline (Conditional Routing)

```csharp
var source = SourceElement.Create("source", ct => GenerateNumbers(ct));
var router = RouterElement.Create<int>("router", 2, x => x % 2); // Even/Odd
var evenSink = SinkElement.Create<int>("evens", x => Console.WriteLine($"Even: {x}"));
var oddSink = SinkElement.Create<int>("odds", x => Console.WriteLine($"Odd: {x}"));

var definition = builder
    .Add(source)
    .Add(router)
    .Add(evenSink)
    .Add(oddSink)
    .Link(source.Outputs[0], router.Inputs[0])
    .Link(router.Outputs[0], evenSink.Inputs[0])
    .Link(router.Outputs[1], oddSink.Inputs[0])
    .BuildDefinition();
```

### Merge Pipeline (Fan-In)

```csharp
var source1 = SourceElement.Create("source1", ct => GenerateSequence(1, 5, ct));
var source2 = SourceElement.Create("source2", ct => GenerateSequence(10, 15, ct));
var merge = MergeElement.Create<int>("merge", 2, 
    new MergePolicy { Mode = MergeMode.Interleave });
var sink = SinkElement.Create<int>("sink", x => Console.WriteLine($"Merged: {x}"));

var definition = builder
    .Add(source1)
    .Add(source2)
    .Add(merge)
    .Add(sink)
    .Link(source1.Outputs[0], merge.Inputs[0])
    .Link(source2.Outputs[0], merge.Inputs[1])
    .Link(merge.Outputs[0], sink.Inputs[0])
    .BuildDefinition();
```

## Configuration

### Element Policies

```csharp
var definition = builder
    .Add(source)
    .Add(transform)
    .ConfigureElement("transform", policy => 
        new ElementPolicy { DegreeOfParallelism = 4 })
    .BuildDefinition();
```

### Link Policies

```csharp
var definition = builder
    .Add(source)
    .Add(sink)
    .Link(source.Outputs[0], sink.Inputs[0])
    .ConfigureLink(source.Outputs[0], sink.Inputs[0], policy =>
        new LinkPolicy 
        { 
            BufferSize = 100,
            FullMode = BoundedChannelFullMode.Wait 
        })
    .BuildDefinition();
```

## Architecture

Piper follows a clean separation between **Definition** and **Runtime**:

### Definition Layer (Static)
- Describes *what* the pipeline looks like
- Elements, pads, links, policies
- Validation and introspection
- Completely serializable/inspectable

### Runtime Layer (Executing)  
- Materializes channels from links
- Runs element processors
- Manages lifecycle and state transitions
- Publishes events to the bus

## Validation

The validator ensures:
- ✅ Unique element names
- ✅ Required pads are connected
- ✅ Type compatibility across links
- ✅ No illegal fan-in (multiple sources to one input without merge)

```csharp
var validator = new PipelineValidator();
validator.Validate(definition); // Throws if invalid
```

## Observability

Subscribe to pipeline events:

```csharp
var runtime = factory.Build(definition);

// Subscribe to events
var eventTask = Task.Run(async () =>
{
    await foreach (var evt in runtime.Bus.Events(CancellationToken.None))
    {
        Console.WriteLine($"[{evt.Timestamp}] {evt.GetType().Name}: {evt.ElementName}");
        
        if (evt is PipelineErrorEvent error)
        {
            Console.WriteLine($"  Error: {error.Exception.Message}");
        }
    }
});

await runtime.StartAsync(CancellationToken.None);
```

## Pipeline States

```
Created → Prepared → Running → Draining → Stopped
                        ↓
                     Faulted
```

## Building from Source

```bash
dotnet build
dotnet test
```

## License

See [LICENSE](LICENSE) file for details.