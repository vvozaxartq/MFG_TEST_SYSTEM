using ATS.Core.Models;

namespace ATS.Application.Ai;

public sealed class FakeRunAnalyzer : IAiRunAnalyzer
{
    public Task<AiRunAnalysisResult> AnalyzeAsync(AiRunAnalysisRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.ArtifactSummary);
        cancellationToken.ThrowIfCancellationRequested();

        var summary = request.ArtifactSummary;
        var observations = new List<AiObservation>();

        if (string.Equals(summary.RunStatus, "Passed", StringComparison.OrdinalIgnoreCase))
        {
            observations.Add(new AiObservation
            {
                Severity = "Info",
                Title = "Run Passed",
                Detail = $"Run completed successfully with {summary.MeasurementCount} measurements and {summary.SpecCount} spec evaluations."
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

        if (summary.FailedStepNames.Count > 0)
        {
            observations.Add(new AiObservation
            {
                Severity = "Warning",
                Title = "Failed Steps Detected",
                Detail = $"Failed steps: {JoinOrFallback(summary.FailedStepNames, "N/A")}. First failure: {Fallback(summary.FirstFailureMessage, "N/A")}."
            });
        }

        if (summary.ErrorCount > 0 || summary.ErrorStepCount > 0)
        {
            observations.Add(new AiObservation
            {
                Severity = "Error",
                Title = "Execution Errors Detected",
                Detail = $"Detected {summary.ErrorCount} recorded errors. Error steps: {JoinOrFallback(summary.ErrorStepNames, "N/A")}."
            });
        }

        if (summary.HasVariableResolutionFailures)
        {
            observations.Add(new AiObservation
            {
                Severity = "Error",
                Title = "Variable Resolution Failures",
                Detail = $"Detected {summary.VariableResolutionFailedCount} variable resolution failures. First failure: {Fallback(summary.FirstFailureMessage, "N/A")}. Recommendation: inspect session.events.jsonl and verify Step, DUT, and Global variable inputs."
            });
        }

        if (summary.HasUnhandledException)
        {
            observations.Add(new AiObservation
            {
                Severity = "Error",
                Title = "Unhandled Exception Detected",
                Detail = $"Detected {summary.ExceptionCount} exception events. First exception: {Fallback(summary.FirstExceptionMessage, "N/A")}. Recommendation: inspect session.log and structured events around the first exception before retrying."
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
                Detail = "The deterministic placeholder analyzer did not detect failed specs, runtime errors, or missing step results."
            });
        }

        return Task.FromResult(new AiRunAnalysisResult
        {
            AnalyzerName = "FakeRunAnalyzer",
            Summary = BuildSummary(summary),
            Observations = observations
        });
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
}
