using ATS.Core.Models;

namespace ATS.Application.Ai;

internal interface IRunAnalysisRule
{
    bool IsMatch(RunArtifactSummary summary);

    int Priority { get; }

    string Category { get; }

    string Cause { get; }

    double Confidence { get; }

    string BuildObservationDetail(RunArtifactSummary summary);

    IReadOnlyList<string> BuildRecommendedActions(RunArtifactSummary summary);

    IReadOnlyList<AiEvidenceItem> BuildEvidence(RunArtifactSummary summary);
}
