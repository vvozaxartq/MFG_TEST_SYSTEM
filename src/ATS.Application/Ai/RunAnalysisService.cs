using System.Text.Json;
using System.Text.Json.Serialization;
using ATS.Core.Models;

namespace ATS.Application.Ai;

public sealed class RunAnalysisService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    private readonly ArtifactSummaryBuilder _artifactSummaryBuilder;
    private readonly IAiRunAnalyzer _runAnalyzer;
    private readonly AiAnalysisBundleBuilder _bundleBuilder;

    public RunAnalysisService()
        : this(new ArtifactSummaryBuilder(), new RuleBasedRunAnalyzer(), new AiAnalysisBundleBuilder())
    {
    }

    public RunAnalysisService(
        ArtifactSummaryBuilder artifactSummaryBuilder,
        IAiRunAnalyzer runAnalyzer)
        : this(artifactSummaryBuilder, runAnalyzer, new AiAnalysisBundleBuilder())
    {
    }

    internal RunAnalysisService(
        ArtifactSummaryBuilder artifactSummaryBuilder,
        IAiRunAnalyzer runAnalyzer,
        AiAnalysisBundleBuilder bundleBuilder)
    {
        _artifactSummaryBuilder = artifactSummaryBuilder;
        _runAnalyzer = runAnalyzer;
        _bundleBuilder = bundleBuilder;
    }

    public async Task<AiRunAnalysisResult> AnalyzeAsync(string resultJsonPath, CancellationToken cancellationToken)
    {
        return await AnalyzeAsync(resultJsonPath, string.Empty, cancellationToken);
    }

    public async Task<AiRunAnalysisResult> AnalyzeAsync(
        string resultJsonPath,
        string eventsJsonlPath,
        CancellationToken cancellationToken)
    {
        var outcome = await AnalyzeInternalAsync(resultJsonPath, eventsJsonlPath, cancellationToken);
        return outcome.Analysis;
    }

    public async Task<AiAnalysisBundle> AnalyzeBundleAsync(
        string resultJsonPath,
        string eventsJsonlPath,
        string analysisJsonPath,
        CancellationToken cancellationToken)
    {
        var outcome = await AnalyzeInternalAsync(resultJsonPath, eventsJsonlPath, cancellationToken);
        return _bundleBuilder.Build(
            outcome.Summary,
            outcome.Analysis,
            outcome.ResultJsonPath,
            outcome.EventsJsonlPath,
            analysisJsonPath);
    }

    private async Task<(RunArtifactSummary Summary, AiRunAnalysisResult Analysis, string ResultJsonPath, string EventsJsonlPath)> AnalyzeInternalAsync(
        string resultJsonPath,
        string eventsJsonlPath,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(resultJsonPath))
        {
            throw new ArgumentException("Result JSON path is required.", nameof(resultJsonPath));
        }

        var fullPath = Path.GetFullPath(resultJsonPath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Result JSON file was not found.", fullPath);
        }

        var json = await File.ReadAllTextAsync(fullPath, cancellationToken);
        var result = JsonSerializer.Deserialize<TestResult>(json, JsonOptions)
            ?? throw new InvalidOperationException("Result JSON file could not be parsed.");

        EnsureSupportedRunResult(result, fullPath);

        var fullEventsPath = string.IsNullOrWhiteSpace(eventsJsonlPath)
            ? string.Empty
            : Path.GetFullPath(eventsJsonlPath);
        var summary = _artifactSummaryBuilder.Build(result, fullPath, fullEventsPath);
        var analysis = await _runAnalyzer.AnalyzeAsync(
            new AiRunAnalysisRequest
            {
                ResultJsonPath = fullPath,
                ArtifactSummary = summary
            },
            cancellationToken);

        return (summary, analysis, fullPath, fullEventsPath);
    }

    private static void EnsureSupportedRunResult(TestResult result, string fullPath)
    {
        if (string.IsNullOrWhiteSpace(result.SessionId) || string.IsNullOrWhiteSpace(result.CommandName))
        {
            throw new InvalidOperationException($"Result JSON '{fullPath}' does not contain a supported run artifact.");
        }

        var isSupportedCommand =
            string.Equals(result.CommandName, "test simulate", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(result.CommandName, "test run", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(result.CommandName, "script run", StringComparison.OrdinalIgnoreCase);

        if (!isSupportedCommand)
        {
            throw new InvalidOperationException(
                $"Result JSON '{fullPath}' contains command '{result.CommandName}', but 'ats ai analyze' only supports test/script run artifacts.");
        }
    }
}
