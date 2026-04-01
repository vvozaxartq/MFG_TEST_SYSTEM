namespace ATS.Application.Recipes;

public sealed class RecipeMeasurementDefinition
{
    public string Key { get; set; } = string.Empty;

    public string SourcePath { get; set; } = string.Empty;

    public string ValueType { get; set; } = string.Empty;

    public string Unit { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}
