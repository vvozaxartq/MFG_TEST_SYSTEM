namespace ATS.Core.Models;

public sealed class ValidationResult
{
    public string SessionId { get; init; } = string.Empty;

    public string CommandName { get; init; } = string.Empty;

    public string ValidationType { get; init; } = string.Empty;

    public string TargetPath { get; init; } = string.Empty;

    public string RelatedPath { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public DateTimeOffset StartedAtUtc { get; init; }

    public DateTimeOffset CompletedAtUtc { get; init; }

    public double DurationSeconds => (CompletedAtUtc - StartedAtUtc).TotalSeconds;

    public List<string> Errors { get; init; } = new();

    public List<string> Warnings { get; init; } = new();
}
