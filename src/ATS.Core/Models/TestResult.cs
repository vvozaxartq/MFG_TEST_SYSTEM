namespace ATS.Core.Models;

public sealed class TestResult
{
    public string SessionId { get; init; } = string.Empty;

    public string CommandName { get; init; } = string.Empty;

    public string RecipeName { get; init; } = string.Empty;

    public string RecipePath { get; init; } = string.Empty;

    public string SpecPath { get; init; } = string.Empty;

    public string DeviceName { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public DateTimeOffset StartedAtUtc { get; init; }

    public DateTimeOffset CompletedAtUtc { get; init; }

    public double DurationSeconds => (CompletedAtUtc - StartedAtUtc).TotalSeconds;

    public List<ScriptResult> Scripts { get; init; } = new();

    public List<string> Errors { get; init; } = new();
}
