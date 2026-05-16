using ATS.Application.Recipes;
using ATS.Core.Models;

namespace ATS.Application.Flow;

internal sealed class FlowConditionEvaluator
{
    public FlowConditionEvaluationResult Evaluate(
        FlowConditionDefinition condition,
        TestContext context,
        DutExecutionRuntime dutRuntime)
    {
        ArgumentNullException.ThrowIfNull(condition);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(dutRuntime);

        var normalizedType = NormalizeConditionType(condition.Type);

        return normalizedType switch
        {
            "previousstepstatus" => EvaluatePreviousStepStatus(condition, dutRuntime),
            "dataexists" => EvaluateDataExists(condition, context),
            "dataequals" => EvaluateDataEquals(condition, context),
            _ => throw new InvalidOperationException(
                $"Unsupported flow condition type '{condition.Type}'. Supported values: previousStepStatus, dataExists, dataEquals.")
        };
    }

    private static FlowConditionEvaluationResult EvaluatePreviousStepStatus(
        FlowConditionDefinition condition,
        DutExecutionRuntime dutRuntime)
    {
        var actualStatus = dutRuntime.State.LastStepStatus;
        var expectedStatus = condition.Status;
        var matched = !string.IsNullOrWhiteSpace(expectedStatus) &&
                      string.Equals(actualStatus, expectedStatus, StringComparison.OrdinalIgnoreCase);
        var message = string.IsNullOrWhiteSpace(actualStatus)
            ? $"No previous step status was available. Expected '{expectedStatus}'."
            : $"Previous step status '{actualStatus}' {(matched ? "matched" : "did not match")} expected '{expectedStatus}'.";

        return new FlowConditionEvaluationResult(
            "previousStepStatus",
            matched,
            "DutExecutionRuntime.State.LastStepStatus",
            actualStatus,
            expectedStatus,
            message);
    }

    private static FlowConditionEvaluationResult EvaluateDataExists(
        FlowConditionDefinition condition,
        TestContext context)
    {
        var key = condition.Key;
        var exists = context.Data.TryGetValue(key, out var actualValue);
        var message = exists
            ? $"Data key '{key}' exists with value '{actualValue}'."
            : $"Data key '{key}' does not exist.";

        return new FlowConditionEvaluationResult(
            "dataExists",
            exists,
            $"DataCollection[{key}]",
            actualValue ?? string.Empty,
            "Exists",
            message);
    }

    private static FlowConditionEvaluationResult EvaluateDataEquals(
        FlowConditionDefinition condition,
        TestContext context)
    {
        var key = condition.Key;
        var expectedValue = condition.Value;
        var exists = context.Data.TryGetValue(key, out var actualValue);
        var matched = exists &&
                      string.Equals(actualValue, expectedValue, StringComparison.OrdinalIgnoreCase);
        var message = !exists
            ? $"Data key '{key}' does not exist. Expected '{expectedValue}'."
            : $"Data key '{key}' value '{actualValue}' {(matched ? "matched" : "did not match")} expected '{expectedValue}'.";

        return new FlowConditionEvaluationResult(
            "dataEquals",
            matched,
            $"DataCollection[{key}]",
            actualValue ?? string.Empty,
            expectedValue,
            message);
    }

    private static string NormalizeConditionType(string value)
    {
        return string.Concat(value.Where(character => !char.IsWhiteSpace(character)))
            .ToLowerInvariant();
    }
}

internal sealed record FlowConditionEvaluationResult(
    string ConditionType,
    bool Matched,
    string Source,
    string ActualValue,
    string ExpectedValue,
    string Message);
