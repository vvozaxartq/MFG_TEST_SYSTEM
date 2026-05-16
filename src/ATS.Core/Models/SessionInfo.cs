namespace ATS.Core.Models;

public sealed class SessionInfo
{
    public string SessionId { get; init; } = string.Empty;

    public string CommandName { get; init; } = string.Empty;

    public string RecipeName { get; init; } = string.Empty;

    public string RecipePath { get; init; } = string.Empty;

    public string SpecPath { get; init; } = string.Empty;

    public string SerialNumber { get; init; } = string.Empty;

    public string Station { get; init; } = string.Empty;

    public string Mode { get; init; } = string.Empty;

    public string FinalStatus { get; init; } = string.Empty;

    public string PrimaryDutId { get; init; } = string.Empty;

    public Dictionary<string, string> Inputs { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public SessionArtifactManifest Artifacts { get; init; } = new();

    public DateTimeOffset StartedAtUtc { get; init; }

    public DateTimeOffset CompletedAtUtc { get; init; }
}
