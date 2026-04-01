namespace ATS.Core.Models;

public sealed class MeasurementItem
{
    public string Key { get; init; } = string.Empty;

    public string Prefix { get; init; } = string.Empty;

    public string FullKey { get; init; } = string.Empty;

    public string Value { get; init; } = string.Empty;

    public MeasurementValueType ValueType { get; init; }

    public string Unit { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string RawText { get; init; } = string.Empty;
}
