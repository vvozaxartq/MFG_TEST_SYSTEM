namespace ATS.Core.Models;

public sealed class ScriptExecutionResult
{
    public string ScriptName { get; init; } = string.Empty;

    public string Command { get; init; } = string.Empty;

    public string MeasurementKey { get; init; } = string.Empty;

    public string Unit { get; init; } = string.Empty;

    public string SpecKey { get; init; } = string.Empty;

    public string RawValue { get; init; } = string.Empty;

    public decimal? NumericValue { get; init; }
}
