namespace ATS.Application.Recipes;

internal static class RecipeStepDefinitionHelper
{
    public static string GetEffectivePrefix(RecipeDefinition recipe, RecipeScriptDefinition step)
    {
        return string.IsNullOrWhiteSpace(step.Prefix)
            ? recipe.Prefix
            : step.Prefix;
    }

    public static IReadOnlyList<DeclaredMeasurementDefinition> GetDeclaredMeasurements(
        RecipeDefinition recipe,
        RecipeScriptDefinition step)
    {
        var prefix = GetEffectivePrefix(recipe, step);

        if (step.Measurements.Count > 0)
        {
            return step.Measurements
                .Select(item => new DeclaredMeasurementDefinition(
                    item.Key,
                    BuildFullKey(prefix, item.Key),
                    item.Unit,
                    item.Description,
                    item.ValueType,
                    string.IsNullOrWhiteSpace(item.SourcePath) ? item.Key : item.SourcePath))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(step.MeasurementKey))
        {
            return new List<DeclaredMeasurementDefinition>
            {
                new(
                    step.MeasurementKey,
                    BuildFullKey(prefix, step.MeasurementKey),
                    step.Unit,
                    string.Empty,
                    string.Empty,
                    step.MeasurementKey)
            };
        }

        return Array.Empty<DeclaredMeasurementDefinition>();
    }

    public static string BuildFullKey(string prefix, string key)
    {
        return string.IsNullOrWhiteSpace(prefix)
            ? key
            : $"{prefix}.{key}";
    }
}

internal sealed record DeclaredMeasurementDefinition(
    string Key,
    string FullKey,
    string Unit,
    string Description,
    string ValueType,
    string SourcePath);
