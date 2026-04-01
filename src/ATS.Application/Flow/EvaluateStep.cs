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
        TestContext context)
    {
        var measurements = measurementSet.Items.ToList();
        var itemsByFullKey = measurements.ToDictionary(item => item.FullKey, item => item, StringComparer.OrdinalIgnoreCase);
        var specResults = new List<SpecEvaluationResult>();

        foreach (var measurement in measurements)
        {
            context.Log($"Measurement '{measurement.FullKey}' = '{measurement.Value}' ({measurement.ValueType}).");
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
                $"Rule '{evaluationResult.RuleName}' on '{evaluationResult.TargetKey}' => {evaluationResult.PassFail}: {evaluationResult.Reason}");
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
            FinalStatus = finalStatus
        };
    }
}
