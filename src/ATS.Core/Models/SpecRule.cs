namespace ATS.Core.Models;

public sealed class SpecRule
{
    public string Name { get; init; } = string.Empty;

    public string TargetKey { get; init; } = string.Empty;

    public string RuleType { get; init; } = string.Empty;

    public string Expected { get; init; } = string.Empty;

    public decimal? Min { get; init; }

    public decimal? Max { get; init; }

    public string Pattern { get; init; } = string.Empty;

    public string ErrorCode { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public bool IgnoreCase { get; init; }
}
