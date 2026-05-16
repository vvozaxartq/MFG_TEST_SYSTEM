namespace ATS.Core.Models;

public sealed record AiAnalysisBundle
{
    public string SchemaVersion { get; init; } = string.Empty;

    public DateTimeOffset GeneratedAtUtc { get; init; }

    public string AnalyzerName { get; init; } = string.Empty;

    public string ResultJsonPath { get; init; } = string.Empty;

    public string EventsJsonlPath { get; init; } = string.Empty;

    public string AnalysisJsonPath { get; init; } = string.Empty;

    public RunArtifactSummary Summary { get; init; } = new();

    public AiRunAnalysisResult Analysis { get; init; } = new();
}
