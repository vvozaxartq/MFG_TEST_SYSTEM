namespace ATS.Core.Models;

public sealed class FlowNodeResult
{
    public string NodeKind { get; init; } = string.Empty;

    public string NodeName { get; init; } = string.Empty;

    public string NodeReference { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public bool CountsTowardFinalStatus { get; init; } = true;

    public string OutcomePolicy { get; init; } = string.Empty;

    public string TriggeredByNodeName { get; init; } = string.Empty;

    public DateTimeOffset StartedAtUtc { get; init; }

    public DateTimeOffset CompletedAtUtc { get; init; }

    public double DurationSeconds => (CompletedAtUtc - StartedAtUtc).TotalSeconds;

    public List<FlowNodeResult> Children { get; init; } = new();

    public string ConditionType { get; init; } = string.Empty;

    public string SelectedBranch { get; init; } = string.Empty;

    public int MaxIterations { get; init; }

    public int CompletedIterations { get; init; }

    public string StopReason { get; init; } = string.Empty;

    public List<FlowIterationResult> Iterations { get; init; } = new();
}
