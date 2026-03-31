using System.Globalization;
using ATS.Application.Recipes;
using ATS.Core.Specs;

namespace ATS.Application.Specs;

public sealed class SpecValidator
{
    public List<string> Validate(IReadOnlyCollection<SpecDefinition> specs)
    {
        var errors = new List<string>();

        if (specs.Count == 0)
        {
            errors.Add("Spec file must contain at least one spec definition.");
            return errors;
        }

        var duplicateKeys = specs
            .Where(item => !string.IsNullOrWhiteSpace(item.Key))
            .GroupBy(item => item.Key, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        foreach (var duplicateKey in duplicateKeys)
        {
            errors.Add($"Duplicate spec key '{duplicateKey}' was found.");
        }

        foreach (var spec in specs)
        {
            if (string.IsNullOrWhiteSpace(spec.Key))
            {
                errors.Add("Spec key is required.");
                continue;
            }

            if (!SpecOperatorParser.TryParse(spec.Operator, out var specOperator))
            {
                errors.Add($"Spec '{spec.Key}' uses unsupported operator '{spec.Operator}'.");
                continue;
            }

            ValidateOperator(spec, specOperator, errors);
        }

        return errors;
    }

    private static void ValidateOperator(
        SpecDefinition spec,
        SpecOperator specOperator,
        List<string> errors)
    {
        switch (specOperator)
        {
            case SpecOperator.Bypass:
                return;
            case SpecOperator.Range:
                if (!spec.Minimum.HasValue || !spec.Maximum.HasValue)
                {
                    errors.Add($"Spec '{spec.Key}' requires both minimum and maximum values.");
                }

                return;
            case SpecOperator.Equal:
            case SpecOperator.NotEqual:
            case SpecOperator.Contain:
            case SpecOperator.Regex:
                if (string.IsNullOrWhiteSpace(spec.Expected))
                {
                    errors.Add($"Spec '{spec.Key}' requires expected value.");
                }

                return;
            case SpecOperator.GreaterThan:
            case SpecOperator.LessThan:
                if (string.IsNullOrWhiteSpace(spec.Expected))
                {
                    errors.Add($"Spec '{spec.Key}' requires expected numeric value.");
                    return;
                }

                if (!decimal.TryParse(spec.Expected, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
                {
                    errors.Add($"Spec '{spec.Key}' expected numeric value '{spec.Expected}'.");
                }

                return;
            default:
                errors.Add($"Spec '{spec.Key}' uses unsupported operator '{specOperator}'.");
                return;
        }
    }
}
