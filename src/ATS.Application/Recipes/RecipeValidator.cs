using ATS.Application.Specs;

namespace ATS.Application.Recipes;

public sealed class RecipeValidator
{
    public List<string> Validate(
        RecipeDefinition recipe,
        SpecDocument specDocument,
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

        var legacySpecKeys = new HashSet<string>(
            specDocument.Specs.Select(item => item.Key),
            StringComparer.OrdinalIgnoreCase);

        if (legacySpecKeys.Count == 0 && specDocument.Rules.Count == 0)
        {
            errors.Add("No spec definitions were found. Provide inline recipe rules/specs or use --spec.");
        }

        var declaredFullKeys = new List<(string StepName, string FullKey)>();

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

            var measurements = RecipeStepDefinitionHelper.GetDeclaredMeasurements(recipe, script);

            if (measurements.Count == 0)
            {
                errors.Add($"Script '{script.Name}' must declare at least one measurement.");
            }

            var duplicateMeasurementKeys = measurements
                .GroupBy(item => item.FullKey, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();

            foreach (var duplicateMeasurementKey in duplicateMeasurementKeys)
            {
                errors.Add($"Script '{script.Name}' contains duplicate measurement fullKey '{duplicateMeasurementKey}'.");
            }

            foreach (var measurement in measurements)
            {
                declaredFullKeys.Add((script.Name, measurement.FullKey));
            }

            if (!string.IsNullOrWhiteSpace(script.SpecKey))
            {
                if (measurements.Count != 1)
                {
                    errors.Add(
                        $"Script '{script.Name}' uses legacy specKey '{script.SpecKey}' but declares multiple measurements.");
                }
                else if (!legacySpecKeys.Contains(script.SpecKey))
                {
                    errors.Add($"Script '{script.Name}' references missing spec '{script.SpecKey}'.");
                }
            }
            else
            {
                foreach (var measurement in measurements)
                {
                    var hasMatchingRule = specDocument.Rules.Any(item =>
                        string.Equals(item.TargetKey, measurement.FullKey, StringComparison.OrdinalIgnoreCase));

                    if (!hasMatchingRule)
                    {
                        errors.Add(
                            $"Script '{script.Name}' measurement '{measurement.FullKey}' does not have a matching spec rule.");
                    }
                }
            }
        }

        var duplicateFullKeysAcrossSteps = declaredFullKeys
            .GroupBy(item => item.FullKey, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        foreach (var duplicateFullKey in duplicateFullKeysAcrossSteps)
        {
            errors.Add($"Duplicate measurement fullKey '{duplicateFullKey}' was found across recipe steps.");
        }

        if (!string.IsNullOrWhiteSpace(selectedScriptName) &&
            !recipe.Scripts.Any(item => string.Equals(item.Name, selectedScriptName, StringComparison.OrdinalIgnoreCase)))
        {
            errors.Add($"Script '{selectedScriptName}' was not found in recipe '{recipe.Name}'.");
        }

        return errors;
    }
}
