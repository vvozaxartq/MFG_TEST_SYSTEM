using ATS.Core.Specs;
using ATS.Core.Models;

namespace ATS.Application.Specs;

internal static class SpecOperatorParser
{
    public static SpecOperator Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return SpecOperator.Range;
        }

        if (Enum.TryParse<SpecOperator>(value, true, out var parsedValue))
        {
            return parsedValue;
        }

        throw new InvalidOperationException($"Spec operator '{value}' is not supported.");
    }

    public static bool TryParse(string value, out SpecOperator specOperator)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            specOperator = SpecOperator.Range;
            return true;
        }

        return Enum.TryParse(value, true, out specOperator);
    }

    public static string BuildExpectedDescription(SpecOperator specOperator, SpecRule rule)
    {
        return specOperator switch
        {
            SpecOperator.Bypass => "Bypass",
            SpecOperator.Range => $"{rule.Min} to {rule.Max}",
            SpecOperator.Regex => string.IsNullOrWhiteSpace(rule.Pattern) ? rule.Expected : rule.Pattern,
            _ => rule.Expected
        };
    }
}
