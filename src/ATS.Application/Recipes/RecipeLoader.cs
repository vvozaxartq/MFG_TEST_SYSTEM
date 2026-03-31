using System.Text.Json;

namespace ATS.Application.Recipes;

public sealed class RecipeLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public RecipeDefinition Load(string recipePath)
    {
        if (string.IsNullOrWhiteSpace(recipePath))
        {
            throw new ArgumentException("Recipe path is required.", nameof(recipePath));
        }

        if (!File.Exists(recipePath))
        {
            throw new FileNotFoundException("Recipe file was not found.", recipePath);
        }

        var json = File.ReadAllText(recipePath);
        var recipe = JsonSerializer.Deserialize<RecipeDefinition>(json, JsonOptions)
            ?? throw new InvalidOperationException("Recipe file could not be parsed.");

        recipe.Scripts ??= new List<RecipeScriptDefinition>();
        recipe.Specs ??= new List<SpecDefinition>();

        return recipe;
    }
}
