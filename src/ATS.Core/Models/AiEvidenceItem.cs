namespace ATS.Core.Models;

public sealed record AiEvidenceItem
{
    public string Type { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public string Source { get; init; } = string.Empty;

    public string Value { get; init; } = string.Empty;
}
