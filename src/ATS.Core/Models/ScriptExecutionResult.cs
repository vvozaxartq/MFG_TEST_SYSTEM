namespace ATS.Core.Models;

public sealed class ScriptExecutionResult
{
    public string ScriptName { get; init; } = string.Empty;

    public string Command { get; init; } = string.Empty;

    public string Prefix { get; init; } = string.Empty;

    public string SpecKey { get; init; } = string.Empty;

    public MeasurementSet MeasurementSet { get; init; } = new();
}
