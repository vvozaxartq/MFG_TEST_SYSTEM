namespace ATS.Core.Models;

public sealed record DutContext
{
    public string Id { get; init; } = string.Empty;

    public int Index { get; init; }

    public string SerialNumber { get; init; } = string.Empty;

    public string Station { get; init; } = string.Empty;

    public string Slot { get; init; } = string.Empty;

    public bool IsSimulated { get; init; }
}
