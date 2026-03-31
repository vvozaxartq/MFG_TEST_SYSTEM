using System.Globalization;
using System.Text.RegularExpressions;
using ATS.Application.Recipes;
using ATS.Core.Models;
using ATS.Core.Specs;

namespace ATS.Application.Specs;

public sealed class SpecEngine
{
    public ScriptResult Evaluate(ScriptExecutionResult executionResult, SpecDefinition spec)
    {
        var specOperator = SpecOperatorParser.Parse(spec.Operator);
        var actualValue = executionResult.RawValue;
        var comparison = spec.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var expected = SpecOperatorParser.BuildExpectedDescription(specOperator, spec);

        var passed = specOperator switch
        {
            SpecOperator.Bypass => true,
            SpecOperator.Equal => string.Equals(actualValue, spec.Expected, comparison),
            SpecOperator.NotEqual => !string.Equals(actualValue, spec.Expected, comparison),
            SpecOperator.Contain => actualValue.Contains(spec.Expected, comparison),
            SpecOperator.Regex => Regex.IsMatch(
                actualValue,
                spec.Expected,
                spec.IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None,
                TimeSpan.FromSeconds(1)),
            SpecOperator.Range => EvaluateRange(executionResult, spec),
            SpecOperator.GreaterThan => EvaluateGreaterThan(executionResult, spec),
            SpecOperator.LessThan => EvaluateLessThan(executionResult, spec),
            _ => throw new InvalidOperationException($"Spec operator '{spec.Operator}' is not supported.")
        };

        var status = passed ? "Passed" : "Failed";
        var message = BuildMessage(specOperator, passed, executionResult, expected);

        return new ScriptResult
        {
            ScriptName = executionResult.ScriptName,
            Command = executionResult.Command,
            MeasurementKey = executionResult.MeasurementKey,
            SpecKey = executionResult.SpecKey,
            ActualValue = actualValue,
            NumericValue = executionResult.NumericValue,
            Unit = executionResult.Unit,
            Operator = specOperator.ToString(),
            Expected = expected,
            Minimum = spec.Minimum,
            Maximum = spec.Maximum,
            Status = status,
            Message = message
        };
    }

    private static bool EvaluateRange(ScriptExecutionResult executionResult, SpecDefinition spec)
    {
        var actualValue = ReadNumericValue(executionResult);

        if (!spec.Minimum.HasValue || !spec.Maximum.HasValue)
        {
            throw new InvalidOperationException($"Spec '{spec.Key}' requires both minimum and maximum values.");
        }

        return actualValue >= spec.Minimum.Value && actualValue <= spec.Maximum.Value;
    }

    private static bool EvaluateGreaterThan(ScriptExecutionResult executionResult, SpecDefinition spec)
    {
        var actualValue = ReadNumericValue(executionResult);
        var expectedValue = ParseExpectedDecimal(spec);
        return actualValue > expectedValue;
    }

    private static bool EvaluateLessThan(ScriptExecutionResult executionResult, SpecDefinition spec)
    {
        var actualValue = ReadNumericValue(executionResult);
        var expectedValue = ParseExpectedDecimal(spec);
        return actualValue < expectedValue;
    }

    private static decimal ReadNumericValue(ScriptExecutionResult executionResult)
    {
        if (!executionResult.NumericValue.HasValue)
        {
            throw new InvalidOperationException(
                $"Script '{executionResult.ScriptName}' returned non-numeric value '{executionResult.RawValue}'.");
        }

        return executionResult.NumericValue.Value;
    }

    private static decimal ParseExpectedDecimal(SpecDefinition spec)
    {
        if (!decimal.TryParse(spec.Expected, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedValue))
        {
            throw new InvalidOperationException($"Spec '{spec.Key}' expected numeric value '{spec.Expected}'.");
        }

        return parsedValue;
    }

    private static string BuildMessage(
        SpecOperator specOperator,
        bool passed,
        ScriptExecutionResult executionResult,
        string expected)
    {
        if (specOperator == SpecOperator.Bypass)
        {
            return $"{executionResult.MeasurementKey} bypassed spec evaluation.";
        }

        return passed
            ? $"{executionResult.MeasurementKey} matched {specOperator} requirement '{expected}'."
            : $"{executionResult.MeasurementKey} value '{executionResult.RawValue}' did not match {specOperator} requirement '{expected}'.";
    }
}
