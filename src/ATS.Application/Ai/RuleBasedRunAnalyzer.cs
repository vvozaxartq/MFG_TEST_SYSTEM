using ATS.Core.Models;

namespace ATS.Application.Ai;

public sealed class RuleBasedRunAnalyzer : IAiRunAnalyzer
{
    private readonly IReadOnlyList<IRunAnalysisRule> _rules;

    public RuleBasedRunAnalyzer()
        : this(
            new IRunAnalysisRule[]
            {
                new VariableResolutionFailureRule(),
                new UnhandledExceptionRule(),
                new StepFailureRule(),
                new SpecFailureRule(),
                new SuccessRule()
            })
    {
    }

    internal RuleBasedRunAnalyzer(IReadOnlyList<IRunAnalysisRule> rules)
    {
        _rules = rules;
    }

    public Task<AiRunAnalysisResult> AnalyzeAsync(AiRunAnalysisRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.ArtifactSummary);
        cancellationToken.ThrowIfCancellationRequested();

        var summary = request.ArtifactSummary;
        var observations = BuildObservations(summary);
        var matchingRules = _rules
            .Where(rule => rule.IsMatch(summary))
            .OrderByDescending(rule => rule.Priority)
            .ToList();

        var primaryRule = matchingRules.FirstOrDefault();
        var recommendedActions = matchingRules
            .SelectMany(rule => rule.BuildRecommendedActions(summary))
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .ToList();
        var evidence = matchingRules
            .SelectMany(rule => rule.BuildEvidence(summary))
            .Where(item => !string.IsNullOrWhiteSpace(item.Type) || !string.IsNullOrWhiteSpace(item.Message))
            .Distinct()
            .ToList();
        var matchedRules = matchingRules
            .Select(rule => rule.GetType().Name)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        return Task.FromResult(new AiRunAnalysisResult
        {
            AnalyzerName = "RuleBasedRunAnalyzer",
            PrimaryCategory = primaryRule?.Category ?? "General",
            PrimaryCause = primaryRule?.Cause ?? "No specific cause was classified.",
            Confidence = primaryRule?.Confidence,
            Summary = BuildSummary(summary),
            Observations = observations,
            RecommendedActions = recommendedActions,
            Evidence = evidence,
            MatchedRules = matchedRules
        });
    }

    private static List<AiObservation> BuildObservations(RunArtifactSummary summary)
    {
        var observations = new List<AiObservation>();

        if (summary.HasVariableResolutionFailures)
        {
            observations.Add(new AiObservation
            {
                Severity = "Error",
                Title = "Variable Resolution Failures",
                Detail = $"Detected {summary.VariableResolutionFailedCount} variable resolution failures. First failure: {Fallback(summary.FirstFailureMessage, "N/A")}."
            });
        }

        if (summary.HasUnhandledException)
        {
            observations.Add(new AiObservation
            {
                Severity = "Error",
                Title = "Unhandled Exception Detected",
                Detail = $"Detected {summary.ExceptionCount} exception events. First exception: {Fallback(summary.FirstExceptionMessage, "N/A")}."
            });
        }

        if (summary.FailedStepNames.Count > 0)
        {
            observations.Add(new AiObservation
            {
                Severity = "Warning",
                Title = "Failed Steps Detected",
                Detail = $"Failed steps: {JoinOrFallback(summary.FailedStepNames, "N/A")}."
            });
        }

        if (summary.FailedSpecCount > 0)
        {
            observations.Add(new AiObservation
            {
                Severity = "Warning",
                Title = "Spec Failures Detected",
                Detail = $"Detected {summary.FailedSpecCount} failed spec evaluations. Failed targets: {JoinOrFallback(summary.FailedTargetKeys, "N/A")}."
            });
        }

        if (string.Equals(summary.RunStatus, "Passed", StringComparison.OrdinalIgnoreCase))
        {
            observations.Add(new AiObservation
            {
                Severity = "Info",
                Title = "Run Passed",
                Detail = $"Run completed successfully with {summary.MeasurementCount} measurements and {summary.SpecCount} spec evaluations."
            });
        }

        if (summary.WarningCount > 0)
        {
            observations.Add(new AiObservation
            {
                Severity = "Warning",
                Title = "Structured Warning Events",
                Detail = $"Detected {summary.WarningCount} warning-level structured log events in the analyzed artifact set."
            });
        }

        if (summary.StepCount == 0)
        {
            observations.Add(new AiObservation
            {
                Severity = "Warning",
                Title = "No Step Results",
                Detail = "The result artifact does not contain any executed steps, so analysis is limited."
            });
        }

        if (summary.MeasurementCount == 0)
        {
            observations.Add(new AiObservation
            {
                Severity = "Warning",
                Title = "No Measurements Collected",
                Detail = "No measurement items were recorded in the analyzed result artifact."
            });
        }

        if (observations.Count == 0)
        {
            observations.Add(new AiObservation
            {
                Severity = "Info",
                Title = "No Special Findings",
                Detail = "The rule-based analyzer did not detect failed specs, runtime errors, or missing step results."
            });
        }

        return observations;
    }

    private static string BuildSummary(RunArtifactSummary summary)
    {
        return $"Run status={summary.RunStatus}, steps={summary.StepCount}, failedSpecs={summary.FailedSpecCount}, errors={summary.ErrorCount}, variableFailures={summary.VariableResolutionFailedCount}, exceptions={summary.ExceptionCount}, warnings={summary.WarningCount}.";
    }

    private static string JoinOrFallback(IReadOnlyCollection<string> values, string fallback)
    {
        return values.Count == 0
            ? fallback
            : string.Join(", ", values);
    }

    private static string Fallback(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value)
            ? fallback
            : value;
    }

    private sealed class VariableResolutionFailureRule : IRunAnalysisRule
    {
        public bool IsMatch(RunArtifactSummary summary) => summary.HasVariableResolutionFailures;

        public int Priority => 500;

        public string Category => "Configuration";

        public string Cause => "Variable resolution failed before the run could complete.";

        public double Confidence => 0.98;

        public string BuildObservationDetail(RunArtifactSummary summary)
        {
            return $"Detected {summary.VariableResolutionFailedCount} variable resolution failures. First failure: {Fallback(summary.FirstFailureMessage, "N/A")}.";
        }

        public IReadOnlyList<string> BuildRecommendedActions(RunArtifactSummary summary)
        {
            return
            [
                "Inspect session.events.jsonl for VariableResolutionFailed entries and confirm the first missing placeholder.",
                "Verify the required value exists in Step, DUT, or Global inputs before rerunning.",
                "If the missing placeholder uses dut.*, confirm --sn, --station, and --vars DutId=..., DutIndex=..., Slot=... were supplied."
            ];
        }

        public IReadOnlyList<AiEvidenceItem> BuildEvidence(RunArtifactSummary summary)
        {
            return
            [
                CreateEvidence("Metric", "Variable resolution failure count", "RunArtifactSummary.VariableResolutionFailedCount", summary.VariableResolutionFailedCount.ToString()),
                CreateEvidence("Flag", "Variable resolution failure flag", "RunArtifactSummary.HasVariableResolutionFailures", summary.HasVariableResolutionFailures.ToString()),
                CreateEvidence("Message", "First failure message", "RunArtifactSummary.FirstFailureMessage", summary.FirstFailureMessage)
            ];
        }
    }

    private sealed class UnhandledExceptionRule : IRunAnalysisRule
    {
        public bool IsMatch(RunArtifactSummary summary) => summary.HasUnhandledException;

        public int Priority => 400;

        public string Category => "Runtime";

        public string Cause => "An unhandled exception was recorded during execution.";

        public double Confidence => 0.95;

        public string BuildObservationDetail(RunArtifactSummary summary)
        {
            return $"Detected {summary.ExceptionCount} exception events. First exception: {Fallback(summary.FirstExceptionMessage, "N/A")}.";
        }

        public IReadOnlyList<string> BuildRecommendedActions(RunArtifactSummary summary)
        {
            return
            [
                "Inspect session.log and session.events.jsonl around the first exception timestamp.",
                "Review the failing step and command path before retrying the same recipe.",
                "Capture the first exception message in the changelog or issue tracker if the fault is reproducible."
            ];
        }

        public IReadOnlyList<AiEvidenceItem> BuildEvidence(RunArtifactSummary summary)
        {
            return
            [
                CreateEvidence("Metric", "Exception count", "RunArtifactSummary.ExceptionCount", summary.ExceptionCount.ToString()),
                CreateEvidence("Flag", "Unhandled exception flag", "RunArtifactSummary.HasUnhandledException", summary.HasUnhandledException.ToString()),
                CreateEvidence("Message", "First exception message", "RunArtifactSummary.FirstExceptionMessage", summary.FirstExceptionMessage)
            ];
        }
    }

    private sealed class StepFailureRule : IRunAnalysisRule
    {
        public bool IsMatch(RunArtifactSummary summary) => summary.FailedStepNames.Count > 0 || summary.ErrorStepNames.Count > 0;

        public int Priority => 300;

        public string Category => "Execution";

        public string Cause => "One or more steps did not complete successfully.";

        public double Confidence => 0.88;

        public string BuildObservationDetail(RunArtifactSummary summary)
        {
            var stepNames = summary.FailedStepNames.Count > 0
                ? summary.FailedStepNames
                : summary.ErrorStepNames;
            return $"Affected steps: {JoinOrFallback(stepNames, "N/A")}. First failure: {Fallback(summary.FirstFailureMessage, "N/A")}.";
        }

        public IReadOnlyList<string> BuildRecommendedActions(RunArtifactSummary summary)
        {
            return
            [
                "Inspect the failed step names in result.json and compare them with session.log.",
                "Review command text, parsed measurements, and per-step structured events for the first failed step."
            ];
        }

        public IReadOnlyList<AiEvidenceItem> BuildEvidence(RunArtifactSummary summary)
        {
            var stepNames = summary.FailedStepNames.Count > 0
                ? summary.FailedStepNames
                : summary.ErrorStepNames;

            return
            [
                CreateEvidence("List", "Failed step names", "RunArtifactSummary.FailedStepNames", JoinOrFallback(stepNames, "N/A")),
                CreateEvidence("Message", "First failure message", "RunArtifactSummary.FirstFailureMessage", summary.FirstFailureMessage)
            ];
        }
    }

    private sealed class SpecFailureRule : IRunAnalysisRule
    {
        public bool IsMatch(RunArtifactSummary summary) => summary.FailedSpecCount > 0;

        public int Priority => 200;

        public string Category => "Spec";

        public string Cause => "Measurements were collected, but one or more spec rules failed.";

        public double Confidence => 0.84;

        public string BuildObservationDetail(RunArtifactSummary summary)
        {
            return $"Detected {summary.FailedSpecCount} failed spec evaluations. Failed targets: {JoinOrFallback(summary.FailedTargetKeys, "N/A")}.";
        }

        public IReadOnlyList<string> BuildRecommendedActions(RunArtifactSummary summary)
        {
            return
            [
                "Review failed target keys and compare measured values against the expected rule limits.",
                "Confirm the recipe/spec pair uses the intended targetKey mappings and thresholds."
            ];
        }

        public IReadOnlyList<AiEvidenceItem> BuildEvidence(RunArtifactSummary summary)
        {
            return
            [
                CreateEvidence("Metric", "Failed spec count", "RunArtifactSummary.FailedSpecCount", summary.FailedSpecCount.ToString()),
                CreateEvidence("List", "Failed target keys", "RunArtifactSummary.FailedTargetKeys", JoinOrFallback(summary.FailedTargetKeys, "N/A"))
            ];
        }
    }

    private sealed class SuccessRule : IRunAnalysisRule
    {
        public bool IsMatch(RunArtifactSummary summary)
        {
            return string.Equals(summary.RunStatus, "Passed", StringComparison.OrdinalIgnoreCase) &&
                   !summary.HasVariableResolutionFailures &&
                   !summary.HasUnhandledException &&
                   summary.FailedSpecCount == 0 &&
                   summary.FailedStepNames.Count == 0 &&
                   summary.ErrorStepNames.Count == 0;
        }

        public int Priority => 100;

        public string Category => "Success";

        public string Cause => "The run completed successfully without detected failures.";

        public double Confidence => 0.99;

        public string BuildObservationDetail(RunArtifactSummary summary)
        {
            return $"Run completed successfully with {summary.MeasurementCount} measurements and {summary.SpecCount} spec evaluations.";
        }

        public IReadOnlyList<string> BuildRecommendedActions(RunArtifactSummary summary)
        {
            return
            [
                "Archive the run artifacts if this session should be retained as a known-good baseline.",
                "Use the same recipe/spec combination for regression verification when future changes are made."
            ];
        }

        public IReadOnlyList<AiEvidenceItem> BuildEvidence(RunArtifactSummary summary)
        {
            return
            [
                CreateEvidence("Status", "Run status", "RunArtifactSummary.RunStatus", summary.RunStatus),
                CreateEvidence("Metric", "Step count", "RunArtifactSummary.StepCount", summary.StepCount.ToString()),
                CreateEvidence("Metric", "Failed spec count", "RunArtifactSummary.FailedSpecCount", summary.FailedSpecCount.ToString())
            ];
        }
    }

    private static AiEvidenceItem CreateEvidence(string type, string message, string source, string value)
    {
        return new AiEvidenceItem
        {
            Type = type,
            Message = message,
            Source = source,
            Value = value ?? string.Empty
        };
    }
}
