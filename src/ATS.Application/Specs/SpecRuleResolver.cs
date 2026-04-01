using ATS.Application.Recipes;
using ATS.Core.Models;

namespace ATS.Application.Specs;

internal sealed class SpecRuleResolver
{
    public List<SpecRule> ResolveForStep(
        RecipeDefinition recipe,
        RecipeScriptDefinition step,
        IReadOnlyCollection<string> fullKeys,
        SpecDocument specDocument)
    {
        var fullKeySet = new HashSet<string>(fullKeys, StringComparer.OrdinalIgnoreCase);
        var rules = specDocument.Rules
            .Where(item => !string.IsNullOrWhiteSpace(item.TargetKey) && fullKeySet.Contains(item.TargetKey))
            .ToList();

        if (!string.IsNullOrWhiteSpace(step.SpecKey))
        {
            var legacySpec = specDocument.Specs.FirstOrDefault(item =>
                string.Equals(item.Key, step.SpecKey, StringComparison.OrdinalIgnoreCase));

            if (legacySpec is null)
            {
                throw new InvalidOperationException($"Spec '{step.SpecKey}' was not found.");
            }

            if (fullKeySet.Count != 1)
            {
                throw new InvalidOperationException(
                    $"Legacy specKey '{step.SpecKey}' can only be used with a single measurement.");
            }

            rules.Add(ConvertLegacySpec(step.SpecKey, fullKeySet.Single(), legacySpec));
        }

        return rules;
    }

    private static SpecRule ConvertLegacySpec(string ruleName, string targetKey, SpecDefinition legacySpec)
    {
        return new SpecRule
        {
            Name = ruleName,
            TargetKey = targetKey,
            RuleType = string.IsNullOrWhiteSpace(legacySpec.Operator) ? "Range" : legacySpec.Operator,
            Expected = legacySpec.Expected,
            Min = legacySpec.Minimum,
            Max = legacySpec.Maximum,
            Pattern = legacySpec.Expected,
            ErrorCode = string.Empty,
            Message = string.Empty,
            IgnoreCase = legacySpec.IgnoreCase
        };
    }
}
