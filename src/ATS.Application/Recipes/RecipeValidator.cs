namespace ATS.Application.Recipes;

public sealed class RecipeValidator
{
    public List<string> Validate(
        RecipeDefinition recipe,
        IReadOnlyCollection<SpecDefinition> specs,
        string selectedScriptName)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(recipe.Name))
        {
            errors.Add("Recipe name is required.");
        }

        if (recipe.Scripts.Count == 0)
        {
            errors.Add("Recipe must contain at least one script.");
        }

        var duplicateScriptNames = recipe.Scripts
            .Where(item => !string.IsNullOrWhiteSpace(item.Name))
            .GroupBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        foreach (var duplicateScriptName in duplicateScriptNames)
        {
            errors.Add($"Duplicate script name '{duplicateScriptName}' was found.");
        }

        var specKeys = new HashSet<string>(
            specs.Select(item => item.Key),
            StringComparer.OrdinalIgnoreCase);

        if (specKeys.Count == 0)
        {
            errors.Add("No spec definitions were found. Provide inline recipe specs or use --spec.");
        }

        foreach (var script in recipe.Scripts)
        {
            if (string.IsNullOrWhiteSpace(script.Name))
            {
                errors.Add("Recipe script name is required.");
            }

            if (string.IsNullOrWhiteSpace(script.Command))
            {
                errors.Add($"Script '{script.Name}' command is required.");
            }

            if (string.IsNullOrWhiteSpace(script.MeasurementKey))
            {
                errors.Add($"Script '{script.Name}' measurementKey is required.");
            }

            if (string.IsNullOrWhiteSpace(script.SpecKey))
            {
                errors.Add($"Script '{script.Name}' specKey is required.");
            }
            else if (specKeys.Count > 0 && !specKeys.Contains(script.SpecKey))
            {
                errors.Add($"Script '{script.Name}' references missing spec '{script.SpecKey}'.");
            }
        }

        if (!string.IsNullOrWhiteSpace(selectedScriptName) &&
            !recipe.Scripts.Any(item => string.Equals(item.Name, selectedScriptName, StringComparison.OrdinalIgnoreCase)))
        {
            errors.Add($"Script '{selectedScriptName}' was not found in recipe '{recipe.Name}'.");
        }

        return errors;
    }
}
