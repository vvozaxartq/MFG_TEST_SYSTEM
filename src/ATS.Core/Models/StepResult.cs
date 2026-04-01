namespace ATS.Core.Models;

public sealed class StepResult
{
    public string StepName { get; init; } = string.Empty;

    public string Command { get; init; } = string.Empty;

    public string Prefix { get; init; } = string.Empty;

    public MeasurementSet MeasurementSet { get; init; } = new();

    public List<MeasurementItem> Measurements { get; init; } = new();

    public List<SpecEvaluationResult> SpecResults { get; init; } = new();

    public string FinalStatus { get; init; } = string.Empty;
}
