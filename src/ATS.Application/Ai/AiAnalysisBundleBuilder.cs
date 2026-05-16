using ATS.Core.Models;

namespace ATS.Application.Ai;

public sealed class AiAnalysisBundleBuilder
{
    private const string CurrentSchemaVersion = "ats.ai-analysis-bundle.v1";

    public AiAnalysisBundle Build(
        RunArtifactSummary summary,
        AiRunAnalysisResult analysis,
        string resultJsonPath,
        string eventsJsonlPath = "",
        string analysisJsonPath = "")
    {
        ArgumentNullException.ThrowIfNull(summary);
        ArgumentNullException.ThrowIfNull(analysis);

        if (string.IsNullOrWhiteSpace(resultJsonPath))
        {
            throw new ArgumentException("Result JSON path is required.", nameof(resultJsonPath));
        }

        return new AiAnalysisBundle
        {
            SchemaVersion = CurrentSchemaVersion,
            GeneratedAtUtc = DateTimeOffset.UtcNow,
            AnalyzerName = analysis.AnalyzerName,
            ResultJsonPath = Path.GetFullPath(resultJsonPath),
            EventsJsonlPath = NormalizeOptionalPath(eventsJsonlPath),
            AnalysisJsonPath = NormalizeOptionalPath(analysisJsonPath),
            Summary = summary,
            Analysis = analysis
        };
    }

    private static string NormalizeOptionalPath(string path)
    {
        return string.IsNullOrWhiteSpace(path)
            ? string.Empty
            : Path.GetFullPath(path);
    }
}
