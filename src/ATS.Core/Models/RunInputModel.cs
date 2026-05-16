namespace ATS.Core.Models;

public sealed class RunInputModel
{
    public string SerialNumber { get; init; } = string.Empty;

    public string Station { get; init; } = string.Empty;

    public string Mode { get; init; } = string.Empty;

    public Dictionary<string, string> Values { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
