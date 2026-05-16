using ATS.Core.Models;

namespace ATS.Application.Ai;

public sealed record AiAnalysisBundleComparison
{
    public AiAnalysisBundle LeftBundle { get; init; } = new();

    public AiAnalysisBundle RightBundle { get; init; } = new();

    public AiComparisonValueChange PrimaryCategory { get; init; } = new();

    public AiComparisonValueChange PrimaryCause { get; init; } = new();

    public AiComparisonValueChange Confidence { get; init; } = new();

    public List<AiComparisonCountChange> SummaryCountChanges { get; init; } = new();

    public List<string> AddedMatchedRules { get; init; } = new();

    public List<string> RemovedMatchedRules { get; init; } = new();

    public List<string> AddedFailedStepNames { get; init; } = new();

    public List<string> RemovedFailedStepNames { get; init; } = new();

    public List<string> AddedRecommendedActions { get; init; } = new();

    public List<string> RemovedRecommendedActions { get; init; } = new();

    public List<AiEvidenceItem> AddedEvidence { get; init; } = new();

    public List<AiEvidenceItem> RemovedEvidence { get; init; } = new();

    public bool HasDifferences { get; init; }
}

public sealed record AiComparisonValueChange
{
    public string Label { get; init; } = string.Empty;

    public string LeftValue { get; init; } = string.Empty;

    public string RightValue { get; init; } = string.Empty;

    public bool Changed { get; init; }
}

public sealed record AiComparisonCountChange
{
    public string Label { get; init; } = string.Empty;

    public int LeftValue { get; init; }

    public int RightValue { get; init; }

    public int Delta { get; init; }
}
