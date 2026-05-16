namespace ATS.Core.Models;

public sealed class ValidationResult
{
    public string SessionId { get; init; } = string.Empty;

    public string CommandName { get; init; } = string.Empty;

    public string ValidationType { get; init; } = string.Empty;

    public string TargetPath { get; init; } = string.Empty;

    public string RelatedPath { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public string OutputDirectory { get; init; } = string.Empty;

    public string ResultJsonPath { get; init; } = string.Empty;

    public string ResultCsvPath { get; init; } = string.Empty;

    public string SessionLogPath { get; init; } = string.Empty;

    public string StructuredLogPath { get; init; } = string.Empty;

    public RunInputModel RunInput { get; init; } = new();

    public SessionInfo SessionInfo { get; init; } = new();

    public DateTimeOffset StartedAtUtc { get; init; }

    public DateTimeOffset CompletedAtUtc { get; init; }

    public double DurationSeconds => (CompletedAtUtc - StartedAtUtc).TotalSeconds;

    public List<string> Errors { get; init; } = new();

    public List<string> Warnings { get; init; } = new();
}
