namespace ATS.Core.Models;

public sealed class ScriptExecutionResult
{
    public string ScriptName { get; init; } = string.Empty;

    public string Command { get; init; } = string.Empty;

    public string Prefix { get; init; } = string.Empty;

    public string SpecKey { get; init; } = string.Empty;

    public DateTimeOffset StartedAtUtc { get; init; }

    public DateTimeOffset CompletedAtUtc { get; init; }

    public int AttemptCount { get; init; } = 1;

    public MeasurementSet MeasurementSet { get; init; } = new();
}
