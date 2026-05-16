namespace ATS.Core.Models;

public sealed record AiObservation
{
    public string Severity { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string Detail { get; init; } = string.Empty;
}
