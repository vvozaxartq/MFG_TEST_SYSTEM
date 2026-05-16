namespace ATS.Core.Models;

public sealed class StepResult
{
    public string StepName { get; init; } = string.Empty;

    public string Command { get; init; } = string.Empty;

    public string Prefix { get; init; } = string.Empty;

    public MeasurementSet MeasurementSet { get; init; } = new();

    public List<MeasurementItem> Measurements { get; init; } = new();

    public List<SpecEvaluationResult> SpecResults { get; init; } = new();

    public string FinalStatus { get; init; } = string.Empty;

    public bool CountsTowardFinalStatus { get; init; } = true;

    public int AttemptCount { get; init; } = 1;

    public string FailureMessage { get; init; } = string.Empty;

    public DateTimeOffset StartedAtUtc { get; init; }

    public DateTimeOffset CompletedAtUtc { get; init; }

    public double DurationSeconds => (CompletedAtUtc - StartedAtUtc).TotalSeconds;

    public int RetryCount => Math.Max(0, AttemptCount - 1);
}
