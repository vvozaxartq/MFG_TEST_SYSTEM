namespace ATS.Core.Models;

public sealed record AiRegressionFinding
{
    public string Code { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public string Source { get; init; } = string.Empty;

    public string BaselineValue { get; init; } = string.Empty;

    public string CandidateValue { get; init; } = string.Empty;
}
