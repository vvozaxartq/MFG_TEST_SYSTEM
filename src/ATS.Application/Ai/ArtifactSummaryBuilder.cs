using System.Text.Json;
using System.Text.Json.Serialization;
using ATS.Core.Models;

namespace ATS.Application.Ai;

public sealed class ArtifactSummaryBuilder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public RunArtifactSummary Build(
        TestResult result,
        string sourcePath,
        string eventsJsonlPath = "")
    {
        ArgumentNullException.ThrowIfNull(result);

        var failedSteps = result.Steps
            .Where(item => string.Equals(item.FinalStatus, "Failed", StringComparison.OrdinalIgnoreCase))
            .Select(item => item.StepName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var errorSteps = result.Steps
            .Where(item => string.Equals(item.FinalStatus, "Error", StringComparison.OrdinalIgnoreCase))
            .Select(item => item.StepName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var failedSpecs = result.Steps
            .SelectMany(item => item.SpecResults)
            .Where(item => string.Equals(item.PassFail, "Failed", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var summary = new RunArtifactSummary
        {
            SourcePath = sourcePath,
            SessionId = result.SessionId,
            CommandName = result.CommandName,
            RecipeName = result.RecipeName,
            DeviceName = result.DeviceName,
            RunStatus = result.Status,
            SerialNumber = result.RunInput.SerialNumber,
            Station = result.RunInput.Station,
            Mode = result.RunInput.Mode,
            DurationSeconds = result.DurationSeconds,
            StepCount = result.Steps.Count,
            PassedStepCount = result.Steps.Count(item => string.Equals(item.FinalStatus, "Passed", StringComparison.OrdinalIgnoreCase)),
            FailedStepCount = failedSteps.Count,
            ErrorStepCount = errorSteps.Count,
            MeasurementCount = result.Steps.Sum(item => item.Measurements.Count),
            SpecCount = result.Steps.Sum(item => item.SpecResults.Count),
            FailedSpecCount = failedSpecs.Count,
            ErrorCount = result.Errors.Count,
            ResultJsonPath = result.ResultJsonPath,
            ResultCsvPath = result.ResultCsvPath,
            SessionLogPath = result.SessionLogPath,
            StructuredLogPath = result.StructuredLogPath,
            FailedStepNames = failedSteps,
            ErrorStepNames = errorSteps,
            FailedRuleNames = failedSpecs
                .Select(item => item.RuleName)
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList(),
            FailedTargetKeys = failedSpecs
                .Select(item => item.TargetKey)
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList(),
            ErrorMessages = result.Errors
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.Ordinal)
                .ToList()
        };

        if (string.IsNullOrWhiteSpace(eventsJsonlPath))
        {
            return summary;
        }

        var fullEventsPath = Path.GetFullPath(eventsJsonlPath);
        if (!File.Exists(fullEventsPath))
        {
            throw new FileNotFoundException("Events JSONL file was not found.", fullEventsPath);
        }

        var entries = LoadStructuredLogEntries(fullEventsPath);
        return MergeEventSummary(summary, entries, fullEventsPath);
    }

    private static IReadOnlyList<StructuredLogEntry> LoadStructuredLogEntries(string eventsJsonlPath)
    {
        var entries = new List<StructuredLogEntry>();

        foreach (var line in File.ReadLines(eventsJsonlPath))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var entry = JsonSerializer.Deserialize<StructuredLogEntry>(line, JsonOptions)
                ?? throw new InvalidOperationException($"Structured log line in '{eventsJsonlPath}' could not be parsed.");
            entries.Add(entry);
        }

        return entries
            .OrderBy(item => item.Sequence)
            .ThenBy(item => item.TimestampUtc)
            .ToList();
    }

    private static RunArtifactSummary MergeEventSummary(
        RunArtifactSummary summary,
        IReadOnlyList<StructuredLogEntry> entries,
        string eventsJsonlPath)
    {
        var variableResolvedCount = entries.Count(item => item.EntryType == StructuredLogEntryType.VariableResolved);
        var variableResolutionFailedEvents = entries
            .Where(item => item.EntryType == StructuredLogEntryType.VariableResolutionFailed)
            .ToList();
        var exceptionEvents = entries
            .Where(IsExceptionEvent)
            .ToList();
        var warningCount = entries.Count(IsWarningEvent);

        var failedStepNames = summary.FailedStepNames
            .Concat(entries
                .Where(item =>
                    item.EntryType == StructuredLogEntryType.StepCompleted &&
                    string.Equals(item.Status, "Failed", StringComparison.OrdinalIgnoreCase))
                .Select(item => item.StepName))
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var errorStepNames = summary.ErrorStepNames
            .Concat(entries
                .Where(item =>
                    item.EntryType == StructuredLogEntryType.StepCompleted &&
                    string.Equals(item.Status, "Error", StringComparison.OrdinalIgnoreCase))
                .Select(item => item.StepName))
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var firstFailureMessage = entries
            .Where(IsFailureEvent)
            .Select(item => item.Message)
            .FirstOrDefault(item => !string.IsNullOrWhiteSpace(item))
            ?? string.Empty;

        var firstExceptionMessage = exceptionEvents
            .Select(item => item.Message)
            .FirstOrDefault(item => !string.IsNullOrWhiteSpace(item))
            ?? string.Empty;

        return summary with
        {
            StructuredLogPath = eventsJsonlPath,
            FailedStepNames = failedStepNames,
            ErrorStepNames = errorStepNames,
            VariableResolvedCount = variableResolvedCount,
            VariableResolutionFailedCount = variableResolutionFailedEvents.Count,
            ExceptionCount = exceptionEvents.Count,
            WarningCount = warningCount,
            FirstFailureMessage = firstFailureMessage,
            FirstExceptionMessage = firstExceptionMessage,
            HasVariableResolutionFailures = variableResolutionFailedEvents.Count > 0,
            HasUnhandledException = exceptionEvents.Count > 0
        };
    }

    private static bool IsFailureEvent(StructuredLogEntry entry)
    {
        return entry.EntryType == StructuredLogEntryType.VariableResolutionFailed ||
               entry.Level.Equals("ERROR", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(entry.Status, "Failed", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(entry.Status, "Error", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsWarningEvent(StructuredLogEntry entry)
    {
        return entry.Level.Equals("WARN", StringComparison.OrdinalIgnoreCase) ||
               entry.Level.Equals("WARNING", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsExceptionEvent(StructuredLogEntry entry)
    {
        return entry.EntryType == StructuredLogEntryType.Error &&
               (entry.Message.Contains("exception", StringComparison.OrdinalIgnoreCase) ||
                entry.Message.Contains("unhandled", StringComparison.OrdinalIgnoreCase) ||
                entry.Data.ContainsKey("exceptionType") ||
                entry.Data.ContainsKey("exceptionMessage"));
    }
}
