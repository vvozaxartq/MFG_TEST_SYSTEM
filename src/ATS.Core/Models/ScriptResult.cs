namespace ATS.Core.Models;

public sealed class ScriptResult
{
    public string ScriptName { get; init; } = string.Empty;

    public string Command { get; init; } = string.Empty;

    public string Prefix { get; init; } = string.Empty;

    public string MeasurementKey { get; init; } = string.Empty;

    public string FullKey { get; init; } = string.Empty;

    public string SpecKey { get; init; } = string.Empty;

    public string RuleName { get; init; } = string.Empty;

    public string ActualValue { get; init; } = string.Empty;

    public decimal? NumericValue { get; init; }

    public string Unit { get; init; } = string.Empty;

    public string Operator { get; init; } = string.Empty;

    public string Expected { get; init; } = string.Empty;

    public decimal? Minimum { get; init; }

    public decimal? Maximum { get; init; }

    public string ErrorCode { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;
}
