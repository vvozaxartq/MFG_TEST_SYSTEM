using System.Globalization;
using System.Text.RegularExpressions;
using ATS.Application.Recipes;
using ATS.Core.Models;
using ATS.Core.Specs;

namespace ATS.Application.Specs;

public sealed class SpecEngine
{
    public SpecEvaluationResult Evaluate(MeasurementItem measurement, SpecRule rule)
    {
        var specOperator = SpecOperatorParser.Parse(rule.RuleType);
        var actualValue = measurement.Value;
        var comparison = rule.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var expected = SpecOperatorParser.BuildExpectedDescription(specOperator, rule);

        var passed = specOperator switch
        {
            SpecOperator.Bypass => true,
            SpecOperator.Equal => string.Equals(actualValue, rule.Expected, comparison),
            SpecOperator.NotEqual => !string.Equals(actualValue, rule.Expected, comparison),
            SpecOperator.Contain => actualValue.Contains(rule.Expected, comparison),
            SpecOperator.Regex => Regex.IsMatch(
                actualValue,
                string.IsNullOrWhiteSpace(rule.Pattern) ? rule.Expected : rule.Pattern,
                rule.IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None,
                TimeSpan.FromSeconds(1)),
            SpecOperator.Range => EvaluateRange(measurement, rule),
            SpecOperator.GreaterThan => EvaluateGreaterThan(measurement, rule),
            SpecOperator.LessThan => EvaluateLessThan(measurement, rule),
            _ => throw new InvalidOperationException($"Spec operator '{rule.RuleType}' is not supported.")
        };

        return new SpecEvaluationResult
        {
            RuleName = rule.Name,
            TargetKey = rule.TargetKey,
            RuleType = specOperator.ToString(),
            ActualValue = actualValue,
            PassFail = passed ? "Passed" : "Failed",
            ErrorCode = passed ? string.Empty : rule.ErrorCode,
            Reason = BuildMessage(specOperator, passed, measurement.FullKey, actualValue, expected, rule.Message)
        };
    }

    public ScriptResult Evaluate(ScriptExecutionResult executionResult, SpecDefinition spec)
    {
        var measurement = executionResult.MeasurementSet.Items.FirstOrDefault()
            ?? throw new InvalidOperationException($"Script '{executionResult.ScriptName}' did not produce any measurements.");

        var evaluationResult = Evaluate(
            measurement,
            new SpecRule
            {
                Name = spec.Key,
                TargetKey = measurement.FullKey,
                RuleType = string.IsNullOrWhiteSpace(spec.Operator) ? "Range" : spec.Operator,
                Expected = spec.Expected,
                Min = spec.Minimum,
                Max = spec.Maximum,
                Pattern = spec.Expected,
                IgnoreCase = spec.IgnoreCase
            });

        return new ScriptResult
        {
            ScriptName = executionResult.ScriptName,
            Command = executionResult.Command,
            Prefix = measurement.Prefix,
            MeasurementKey = measurement.Key,
            FullKey = measurement.FullKey,
            SpecKey = spec.Key,
            RuleName = evaluationResult.RuleName,
            ActualValue = measurement.Value,
            NumericValue = TryReadNumericValue(measurement),
            Unit = measurement.Unit,
            Operator = evaluationResult.RuleType,
            Expected = evaluationResult.RuleType == SpecOperator.Range.ToString()
                ? $"{spec.Minimum} to {spec.Maximum}"
                : spec.Expected,
            Minimum = spec.Minimum,
            Maximum = spec.Maximum,
            ErrorCode = evaluationResult.ErrorCode,
            Status = evaluationResult.PassFail,
            Message = evaluationResult.Reason
        };
    }

    private static bool EvaluateRange(MeasurementItem measurement, SpecRule rule)
    {
        var actualValue = ReadNumericValue(measurement);

        if (!rule.Min.HasValue || !rule.Max.HasValue)
        {
            throw new InvalidOperationException($"Spec rule '{rule.Name}' requires both min and max values.");
        }

        return actualValue >= rule.Min.Value && actualValue <= rule.Max.Value;
    }

    private static bool EvaluateGreaterThan(MeasurementItem measurement, SpecRule rule)
    {
        var actualValue = ReadNumericValue(measurement);
        var expectedValue = ParseExpectedDecimal(rule);
        return actualValue > expectedValue;
    }

    private static bool EvaluateLessThan(MeasurementItem measurement, SpecRule rule)
    {
        var actualValue = ReadNumericValue(measurement);
        var expectedValue = ParseExpectedDecimal(rule);
        return actualValue < expectedValue;
    }

    private static decimal ReadNumericValue(MeasurementItem measurement)
    {
        if (!TryReadNumericValue(measurement).HasValue)
        {
            throw new InvalidOperationException(
                $"Measurement '{measurement.FullKey}' returned non-numeric value '{measurement.Value}'.");
        }

        return TryReadNumericValue(measurement)!.Value;
    }

    private static decimal ParseExpectedDecimal(SpecRule rule)
    {
        if (!decimal.TryParse(rule.Expected, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedValue))
        {
            throw new InvalidOperationException($"Spec rule '{rule.Name}' expected numeric value '{rule.Expected}'.");
        }

        return parsedValue;
    }

    private static decimal? TryReadNumericValue(MeasurementItem measurement)
    {
        return decimal.TryParse(measurement.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedValue)
            ? parsedValue
            : null;
    }

    private static string BuildMessage(
        SpecOperator specOperator,
        bool passed,
        string targetKey,
        string actualValue,
        string expected,
        string configuredMessage)
    {
        if (!string.IsNullOrWhiteSpace(configuredMessage))
        {
            return configuredMessage;
        }

        if (specOperator == SpecOperator.Bypass)
        {
            return $"{targetKey} bypassed spec evaluation.";
        }

        return passed
            ? $"{targetKey} matched {specOperator} requirement '{expected}'."
            : $"{targetKey} value '{actualValue}' did not match {specOperator} requirement '{expected}'.";
    }
}
