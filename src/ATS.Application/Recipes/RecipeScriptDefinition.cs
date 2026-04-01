namespace ATS.Application.Recipes;

public sealed class RecipeScriptDefinition
{
    public string Name { get; set; } = string.Empty;

    public string Command { get; set; } = string.Empty;

    public string Prefix { get; set; } = string.Empty;

    public string MeasurementKey { get; set; } = string.Empty;

    public string Unit { get; set; } = string.Empty;

    public string SpecKey { get; set; } = string.Empty;

    public List<RecipeMeasurementDefinition> Measurements { get; set; } = new();

    public decimal? SimulatedValue { get; set; }

    public string SimulatedResponse { get; set; } = string.Empty;
}
