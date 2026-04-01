namespace ATS.Core.Models;

public sealed class SpecEvaluationResult
{
    public string RuleName { get; init; } = string.Empty;

    public string TargetKey { get; init; } = string.Empty;

    public string RuleType { get; init; } = string.Empty;

    public string ActualValue { get; init; } = string.Empty;

    public string PassFail { get; init; } = string.Empty;

    public string ErrorCode { get; init; } = string.Empty;

    public string Reason { get; init; } = string.Empty;
}
