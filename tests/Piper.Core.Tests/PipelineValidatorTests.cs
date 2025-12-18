using Piper.Core.Builder;
using Piper.Core.Definition.Implementations;
using Piper.Core.Elements;
using Piper.Core.Validation;
using System.Runtime.CompilerServices;

namespace Piper.Core.Tests;

public class PipelineValidatorTests
{
    [Fact]
    public void Validate_ValidPipeline_DoesNotThrow()
    {
        // Arrange
        var builder = new PipelineBuilder("valid-pipeline");
        var source = SourceElement.Create("source", ct => GetNumbersAsync(ct));
        var sink = SinkElement.Create<int>("sink", x => { });

        var definition = builder
            .Add(source)
            .Add(sink)
            .Link(source.Outputs[0], sink.Inputs[0])
            .BuildDefinition();

        var validator = new PipelineValidator();

        // Act & Assert
        validator.Validate(definition); // Should not throw
    }

    [Fact]
    public void Validate_DuplicateElementNames_Throws()
    {
        // Arrange
        var source1 = SourceElement.Create("source", ct => GetNumbersAsync(ct));
        var source2 = SourceElement.Create("source", ct => GetNumbersAsync(ct)); // Duplicate name

        var definition = new PipelineDefinition(
            "duplicate-names",
            new[] { source1, source2 },
            Array.Empty<PipelineLinkDefinition>());

        var validator = new PipelineValidator();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => validator.Validate(definition));
        Assert.Contains("duplicate element names", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_RequiredInputNotConnected_Throws()
    {
        // Arrange
        var transform = TransformElement.Create("transform", (int x) => x * 2);
        
        var definition = new PipelineDefinition(
            "missing-input",
            new[] { transform },
            Array.Empty<PipelineLinkDefinition>());

        var validator = new PipelineValidator();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => validator.Validate(definition));
        Assert.Contains("not connected", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_TypeMismatch_Throws()
    {
        // Arrange
        var source = SourceElement.Create("source", ct => GetNumbersAsync(ct));
        var sink = SinkElement.Create<string>("sink", x => { }); // Wrong type

        var definition = new PipelineDefinition(
            "type-mismatch",
            new[] { source, sink },
            new[] { new PipelineLinkDefinition(source.Outputs[0], sink.Inputs[0]) });

        var validator = new PipelineValidator();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => validator.Validate(definition));
        Assert.Contains("Type mismatch", ex.Message);
    }

    [Fact]
    public void Validate_IllegalFanIn_Throws()
    {
        // Arrange
        var source1 = SourceElement.Create("source1", ct => GetNumbersAsync(ct));
        var source2 = SourceElement.Create("source2", ct => GetNumbersAsync(ct));
        var sink = SinkElement.Create<int>("sink", x => { });

        var definition = new PipelineDefinition(
            "illegal-fanin",
            new[] { source1, source2, sink },
            new[]
            {
                new PipelineLinkDefinition(source1.Outputs[0], sink.Inputs[0]),
                new PipelineLinkDefinition(source2.Outputs[0], sink.Inputs[0]) // Duplicate target
            });

        var validator = new PipelineValidator();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => validator.Validate(definition));
        Assert.Contains("fan-in", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_IllegalFanOut_Throws()
    {
        // Arrange
        var source = SourceElement.Create("source", ct => GetNumbersAsync(ct));
        var sink1 = SinkElement.Create<int>("sink1", x => { });
        var sink2 = SinkElement.Create<int>("sink2", x => { });

        var definition = new PipelineDefinition(
            "illegal-fanout",
            new[] { source, sink1, sink2 },
            new[]
            {
                new PipelineLinkDefinition(source.Outputs[0], sink1.Inputs[0]),
                new PipelineLinkDefinition(source.Outputs[0], sink2.Inputs[0]) // Duplicate source
            });

        var validator = new PipelineValidator();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => validator.Validate(definition));
        Assert.Contains("fan-out", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_DegreeOfParallelismGreaterThanOne_Throws()
    {
        // Arrange
        var builder = new PipelineBuilder("parallel-pipeline");
        var source = SourceElement.Create("source", ct => GetNumbersAsync(ct), 
            new ElementPolicy { DegreeOfParallelism = 2 }); // Not supported yet
        var sink = SinkElement.Create<int>("sink", x => { });

        var definition = builder
            .Add(source)
            .Add(sink)
            .Link(source.Outputs[0], sink.Inputs[0])
            .BuildDefinition();

        var validator = new PipelineValidator();

        // Act & Assert
        var ex = Assert.Throws<NotSupportedException>(() => validator.Validate(definition));
        Assert.Contains("DegreeOfParallelism", ex.Message);
        Assert.Contains("only DegreeOfParallelism = 1 is currently supported", ex.Message);
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
