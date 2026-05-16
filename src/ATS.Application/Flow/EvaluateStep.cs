using ATS.Application.Recipes;
using ATS.Application.Specs;
using ATS.Core.Models;

namespace ATS.Application.Flow;

internal sealed class EvaluateStep
{
    private readonly SpecEngine _specEngine;

    public EvaluateStep(SpecEngine specEngine)
    {
        _specEngine = specEngine;
    }

    public StepResult Execute(
        RecipeDefinition recipe,
        RecipeScriptDefinition step,
        MeasurementSet measurementSet,
        IReadOnlyCollection<SpecRule> rules,
        DateTimeOffset startedAtUtc,
        DateTimeOffset completedAtUtc,
        TestContext context)
    {
        var measurements = measurementSet.Items.ToList();
        var itemsByFullKey = measurements.ToDictionary(item => item.FullKey, item => item, StringComparer.OrdinalIgnoreCase);
        var specResults = new List<SpecEvaluationResult>();

        foreach (var measurement in measurements)
        {
            context.Log(
                $"Measurement '{measurement.FullKey}' = '{measurement.Value}' ({measurement.ValueType}).",
                step.Name);
            context.LogEvent(
                "INFO",
                StructuredLogEntryType.MeasurementCollected,
                $"Measurement '{measurement.FullKey}' = '{measurement.Value}'.",
                step.Name,
                stepName: step.Name,
                fullKey: measurement.FullKey,
                status: "Collected",
                data: new Dictionary<string, object?>
                {
                    ["fullKey"] = measurement.FullKey,
                    ["value"] = measurement.Value,
                    ["unit"] = measurement.Unit,
                    ["valueType"] = measurement.ValueType.ToString()
                });
        }

        foreach (var rule in rules)
        {
            SpecEvaluationResult evaluationResult;

            if (!itemsByFullKey.TryGetValue(rule.TargetKey, out var measurement))
            {
                evaluationResult = new SpecEvaluationResult
                {
                    RuleName = rule.Name,
                    TargetKey = rule.TargetKey,
                    RuleType = rule.RuleType,
                    ActualValue = string.Empty,
                    PassFail = "Failed",
                    ErrorCode = string.IsNullOrWhiteSpace(rule.ErrorCode) ? "MEASUREMENT_NOT_FOUND" : rule.ErrorCode,
                    Reason = $"Measurement '{rule.TargetKey}' was not found."
                };
            }
            else
            {
                evaluationResult = _specEngine.Evaluate(measurement, rule);
            }

            specResults.Add(evaluationResult);
            context.Log(
                $"Rule '{evaluationResult.RuleName}' on '{evaluationResult.TargetKey}' => {evaluationResult.PassFail} | Spec: {BuildSpecSummary(evaluationResult)} | Actual: '{evaluationResult.ActualValue}' | Reason: {evaluationResult.Reason}",
                step.Name);
            context.LogEvent(
                string.Equals(evaluationResult.PassFail, "Failed", StringComparison.OrdinalIgnoreCase) ? "ERROR" : "INFO",
                StructuredLogEntryType.SpecEvaluated,
                $"Rule '{evaluationResult.RuleName}' on '{evaluationResult.TargetKey}' => {evaluationResult.PassFail}.",
                step.Name,
                stepName: step.Name,
                fullKey: evaluationResult.TargetKey,
                status: evaluationResult.PassFail,
                data: new Dictionary<string, object?>
                {
                    ["ruleName"] = evaluationResult.RuleName,
                    ["targetKey"] = evaluationResult.TargetKey,
                    ["ruleType"] = evaluationResult.RuleType,
                    ["expected"] = evaluationResult.Expected,
                    ["minimum"] = evaluationResult.Minimum,
                    ["maximum"] = evaluationResult.Maximum,
                    ["pattern"] = evaluationResult.Pattern,
                    ["actualValue"] = evaluationResult.ActualValue,
                    ["reason"] = evaluationResult.Reason,
                    ["errorCode"] = evaluationResult.ErrorCode
                });
        }

        var finalStatus = specResults.Any(item => string.Equals(item.PassFail, "Failed", StringComparison.OrdinalIgnoreCase))
            ? "Failed"
            : "Passed";

        return new StepResult
        {
            StepName = step.Name,
            Command = step.Command,
            Prefix = RecipeStepDefinitionHelper.GetEffectivePrefix(recipe, step),
            MeasurementSet = measurementSet,
            Measurements = measurements,
            SpecResults = specResults,
            FinalStatus = finalStatus,
            StartedAtUtc = startedAtUtc,
            CompletedAtUtc = completedAtUtc
        };
    }

    private static string BuildSpecSummary(SpecEvaluationResult result)
    {
        return result.RuleType switch
        {
            "Range" => $"min={result.Minimum}, max={result.Maximum}",
            "Regex" => $"pattern={result.Pattern}",
            "GreaterThan" => $"expected>{result.Expected}",
            "LessThan" => $"expected<{result.Expected}",
            "Bypass" => "bypass",
            _ => $"expected={result.Expected}"
        };
    }
}
