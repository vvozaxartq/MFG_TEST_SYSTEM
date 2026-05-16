namespace ATS.Core.Models;

public sealed record AiRegressionCheckResult
{
    public string SchemaVersion { get; init; } = string.Empty;

    public DateTimeOffset GeneratedAtUtc { get; init; }

    public AiRegressionStatus Status { get; init; } = AiRegressionStatus.NoRegression;

    public string Summary { get; init; } = string.Empty;

    public string BaselineBundlePath { get; init; } = string.Empty;

    public string CandidateBundlePath { get; init; } = string.Empty;

    public string BaselinePrimaryCategory { get; init; } = string.Empty;

    public string CandidatePrimaryCategory { get; init; } = string.Empty;

    public string BaselinePrimaryCause { get; init; } = string.Empty;

    public string CandidatePrimaryCause { get; init; } = string.Empty;

    public List<AiRegressionFinding> Findings { get; init; } = new();
}
