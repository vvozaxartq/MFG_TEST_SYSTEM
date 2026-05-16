namespace ATS.Application.Recipes;

public sealed class RecipeScriptDefinition
{
    public string Name { get; set; } = string.Empty;

    public string Command { get; set; } = string.Empty;

    public string Prefix { get; set; } = string.Empty;

    public Dictionary<string, string> Variables { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public string MeasurementKey { get; set; } = string.Empty;

    public string Unit { get; set; } = string.Empty;

    public string SpecKey { get; set; } = string.Empty;

    public List<RecipeMeasurementDefinition> Measurements { get; set; } = new();

    public int RetryCount { get; set; }

    public int TimeoutMs { get; set; }

    public bool ContinueOnFailure { get; set; }

    public decimal? SimulatedValue { get; set; }

    public string SimulatedResponse { get; set; } = string.Empty;
}
