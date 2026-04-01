namespace ATS.Core.Models;

public sealed class MeasurementSet
{
    public string Source { get; init; } = string.Empty;

    public string Command { get; init; } = string.Empty;

    public DateTimeOffset CollectedAt { get; init; }

    public string RawPayload { get; init; } = string.Empty;

    public List<MeasurementItem> Items { get; init; } = new();
}
