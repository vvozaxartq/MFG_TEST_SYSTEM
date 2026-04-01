using System.Text.Json;

namespace ATS.Application.Specs;

public sealed class SpecLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public SpecDocument Load(string specPath)
    {
        if (string.IsNullOrWhiteSpace(specPath))
        {
            throw new ArgumentException("Spec path is required.", nameof(specPath));
        }

        if (!File.Exists(specPath))
        {
            throw new FileNotFoundException("Spec file was not found.", specPath);
        }

        var json = File.ReadAllText(specPath);
        var specDocument = JsonSerializer.Deserialize<SpecDocument>(json, JsonOptions)
            ?? throw new InvalidOperationException("Spec file could not be parsed.");

        specDocument.Rules ??= new List<ATS.Core.Models.SpecRule>();
        specDocument.Specs ??= new List<Recipes.SpecDefinition>();
        return specDocument;
    }
}
