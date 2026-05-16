namespace ATS.Core.Models;

public sealed record AiRunAnalysisResult
{
    public string AnalyzerName { get; init; } = string.Empty;

    public string PrimaryCategory { get; init; } = string.Empty;

    public string PrimaryCause { get; init; } = string.Empty;

    public double? Confidence { get; init; }

    public string Summary { get; init; } = string.Empty;

    public List<AiObservation> Observations { get; init; } = new();

    public List<string> RecommendedActions { get; init; } = new();

    public List<AiEvidenceItem> Evidence { get; init; } = new();

    public List<string> MatchedRules { get; init; } = new();
}
