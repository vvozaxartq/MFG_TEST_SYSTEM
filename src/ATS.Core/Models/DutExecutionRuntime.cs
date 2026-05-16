namespace ATS.Core.Models;

public sealed record DutExecutionRuntime
{
    public DutContext Metadata { get; init; } = new();

    public DutExecutionState State { get; init; } = new();
}
