using ATS.Application.Recipes;

namespace ATS.Application.Specs;

public sealed class SpecDocument
{
    public string Name { get; set; } = string.Empty;

    public List<SpecDefinition> Specs { get; set; } = new();
}
