using ATS.Core.Models;

namespace ATS.Application.Ai;

public sealed class FakeBundleAnalysisProvider : IAiBundleAnalysisProvider
{
    public string Name => "fake";

    public Task<AiProviderResponse> AnalyzeAsync(AiProviderRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var bundle = request.Bundle ?? throw new InvalidOperationException("Provider request bundle is required.");
        var matchedRuleCount = bundle.Analysis.MatchedRules.Count;
        var evidenceCount = bundle.Analysis.Evidence.Count;
        var highlights = new List<string>
        {
            $"Bundle schema {Fallback(bundle.SchemaVersion)} for session {Fallback(bundle.Summary.SessionId)} was consumed by the fake provider.",
            $"Primary category {Fallback(bundle.Analysis.PrimaryCategory)} with run status {Fallback(bundle.Summary.RunStatus)} was carried through the provider adapter."
        };

        if (matchedRuleCount > 0)
        {
            highlights.Add($"Matched rules: {string.Join(", ", bundle.Analysis.MatchedRules)}.");
        }

        if (evidenceCount > 0)
        {
            var firstEvidence = bundle.Analysis.Evidence[0];
            highlights.Add($"First evidence: {Fallback(firstEvidence.Source)} = {Fallback(firstEvidence.Value)}.");
        }

        return Task.FromResult(new AiProviderResponse
        {
            ProviderName = Name,
            BundleSchemaVersion = bundle.SchemaVersion,
            PrimaryCategory = bundle.Analysis.PrimaryCategory,
            PrimaryCause = bundle.Analysis.PrimaryCause,
            Summary = $"Fake provider consumed bundle schema={Fallback(bundle.SchemaVersion)}, runStatus={Fallback(bundle.Summary.RunStatus)}, primaryCategory={Fallback(bundle.Analysis.PrimaryCategory)}, matchedRules={matchedRuleCount}, evidence={evidenceCount}.",
            Highlights = highlights,
            RecommendedActions = bundle.Analysis.RecommendedActions
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.Ordinal)
                .ToList()
        });
    }

    private static string Fallback(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "N/A"
            : value;
    }
}
