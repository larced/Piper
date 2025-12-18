using Piper.Core.Builder;
using Piper.Core.Definition.Implementations;
using Piper.Core.Elements;
using System.Runtime.CompilerServices;

namespace Piper.Core.Tests;

public class PipelineBuilderTests
{
    [Fact]
    public void BuildDefinition_WithSimpleLinearPipeline_Succeeds()
    {
        // Arrange
        var builder = new PipelineBuilder("test-pipeline");
        var source = SourceElement.Create("source", ct => GetNumbersAsync(ct));
        var transform = TransformElement.Create("transform", (int x) => x * 2);
        var sink = SinkElement.Create<int>("sink", x => Console.WriteLine(x));

        // Act
        var definition = builder
            .Add(source)
            .Add(transform)
            .Add(sink)
            .Link(source.Outputs[0], transform.Inputs[0])
            .Link(transform.Outputs[0], sink.Inputs[0])
            .BuildDefinition();

        // Assert
        Assert.NotNull(definition);
        Assert.Equal("test-pipeline", definition.Name);
        Assert.Equal(3, definition.Elements.Count);
        Assert.Equal(2, definition.Links.Count);
    }

    [Fact]
    public void BuildDefinition_WithTeeElement_Succeeds()
    {
        // Arrange
        var builder = new PipelineBuilder("tee-pipeline");
        var source = SourceElement.Create("source", ct => GetNumbersAsync(ct));
        var tee = TeeElement.Create<int>("tee", 2);
        var sink1 = SinkElement.Create<int>("sink1", x => { });
        var sink2 = SinkElement.Create<int>("sink2", x => { });

        // Act
        var definition = builder
            .Add(source)
            .Add(tee)
            .Add(sink1)
            .Add(sink2)
            .Link(source.Outputs[0], tee.Inputs[0])
            .Link(tee.Outputs[0], sink1.Inputs[0])
            .Link(tee.Outputs[1], sink2.Inputs[0])
            .BuildDefinition();

        // Assert
        Assert.NotNull(definition);
        Assert.Equal(4, definition.Elements.Count);
        Assert.Equal(3, definition.Links.Count);
    }

    [Fact]
    public void ConfigureElement_UpdatesPolicy()
    {
        // Arrange
        var builder = new PipelineBuilder("test-pipeline");
        var source = SourceElement.Create("source", ct => GetNumbersAsync(ct));
        
        builder.Add(source);

        // Act
        var definition = builder
            .ConfigureElement("source", policy => new ElementPolicy { DegreeOfParallelism = 4 })
            .BuildDefinition();

        // Assert
        var element = definition.Elements.First(e => e.Name == "source");
        Assert.Equal(4, element.Policy.DegreeOfParallelism);
    }

    [Fact]
    public void ConfigureLink_UpdatesPolicy()
    {
        // Arrange
        var builder = new PipelineBuilder("test-pipeline");
        var source = SourceElement.Create("source", ct => GetNumbersAsync(ct));
        var sink = SinkElement.Create<int>("sink", x => { });

        builder
            .Add(source)
            .Add(sink)
            .Link(source.Outputs[0], sink.Inputs[0]);

        // Act
        var definition = builder
            .ConfigureLink(source.Outputs[0], sink.Inputs[0], 
                policy => new LinkPolicy { BufferSize = 100 })
            .BuildDefinition();

        // Assert
        var link = definition.Links.First();
        Assert.Equal(100, link.Policy.BufferSize);
    }

    private static async IAsyncEnumerable<int> GetNumbersAsync([EnumeratorCancellation] CancellationToken ct)
    {
        for (int i = 0; i < 5; i++)
        {
            if (ct.IsCancellationRequested)
                yield break;
            yield return i;
            await Task.Delay(10, ct);
        }
    }
}
