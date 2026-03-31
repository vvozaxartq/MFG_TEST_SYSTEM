namespace ATS.Application.Recipes;

public sealed class SpecDefinition
{
    public string Key { get; set; } = string.Empty;

    public string Operator { get; set; } = string.Empty;

    public string Expected { get; set; } = string.Empty;

    public decimal? Minimum { get; set; }

    public decimal? Maximum { get; set; }

    public bool IgnoreCase { get; set; }
}
