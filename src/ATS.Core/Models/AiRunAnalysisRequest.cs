namespace ATS.Core.Models;

public sealed record AiRunAnalysisRequest
{
    public string ResultJsonPath { get; init; } = string.Empty;

    public RunArtifactSummary ArtifactSummary { get; init; } = new();
}
