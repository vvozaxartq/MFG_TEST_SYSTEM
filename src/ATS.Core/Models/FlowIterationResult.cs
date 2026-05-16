namespace ATS.Core.Models;

public sealed class FlowIterationResult
{
    public int IterationNumber { get; init; }

    public string Status { get; init; } = string.Empty;

    public bool CountsTowardFinalStatus { get; init; } = true;

    public DateTimeOffset StartedAtUtc { get; init; }

    public DateTimeOffset CompletedAtUtc { get; init; }

    public double DurationSeconds => (CompletedAtUtc - StartedAtUtc).TotalSeconds;

    public List<FlowNodeResult> Children { get; init; } = new();
}
