namespace ATS.Application.Recipes;

public sealed class RecipeDefinition
{
    public string Name { get; set; } = string.Empty;

    public string Prefix { get; set; } = string.Empty;

    public List<RecipeScriptDefinition> Scripts { get; set; } = new();

    public List<ATS.Core.Models.SpecRule> Rules { get; set; } = new();

    public List<SpecDefinition> Specs { get; set; } = new();
}
