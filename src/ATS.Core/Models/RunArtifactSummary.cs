namespace ATS.Core.Models;

public sealed record RunArtifactSummary
{
    public string SourcePath { get; init; } = string.Empty;

    public string SessionId { get; init; } = string.Empty;

    public string CommandName { get; init; } = string.Empty;

    public string RecipeName { get; init; } = string.Empty;

    public string DeviceName { get; init; } = string.Empty;

    public string RunStatus { get; init; } = string.Empty;

    public string SerialNumber { get; init; } = string.Empty;

    public string Station { get; init; } = string.Empty;

    public string Mode { get; init; } = string.Empty;

    public double DurationSeconds { get; init; }

    public int StepCount { get; init; }

    public int PassedStepCount { get; init; }

    public int FailedStepCount { get; init; }

    public int ErrorStepCount { get; init; }

    public int MeasurementCount { get; init; }

    public int SpecCount { get; init; }

    public int FailedSpecCount { get; init; }

    public int ErrorCount { get; init; }

    public int VariableResolvedCount { get; init; }

    public int VariableResolutionFailedCount { get; init; }

    public int ExceptionCount { get; init; }

    public int WarningCount { get; init; }

    public string ResultJsonPath { get; init; } = string.Empty;

    public string ResultCsvPath { get; init; } = string.Empty;

    public string SessionLogPath { get; init; } = string.Empty;

    public string StructuredLogPath { get; init; } = string.Empty;

    public List<string> FailedStepNames { get; init; } = new();

    public List<string> ErrorStepNames { get; init; } = new();

    public List<string> FailedRuleNames { get; init; } = new();

    public List<string> FailedTargetKeys { get; init; } = new();

    public List<string> ErrorMessages { get; init; } = new();

    public string FirstFailureMessage { get; init; } = string.Empty;

    public string FirstExceptionMessage { get; init; } = string.Empty;

    public bool HasVariableResolutionFailures { get; init; }

    public bool HasUnhandledException { get; init; }
}
