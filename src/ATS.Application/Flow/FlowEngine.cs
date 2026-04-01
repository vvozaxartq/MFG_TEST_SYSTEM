using ATS.Application.Measurements;
using ATS.Application.Recipes;
using ATS.Application.Scripts;
using ATS.Application.Specs;
using ATS.Core.Devices;
using ATS.Core.Models;

namespace ATS.Application.Flow;

public sealed class FlowEngine
{
    private readonly EvaluateStep _evaluateStep;
    private readonly MeasurementSetBuilder _measurementSetBuilder;
    private readonly SpecRuleResolver _specRuleResolver;

    public FlowEngine(SpecEngine specEngine)
        : this(specEngine, new EvaluateStep(specEngine), new MeasurementSetBuilder(), new SpecRuleResolver())
    {
    }

    internal FlowEngine(
        SpecEngine specEngine,
        EvaluateStep evaluateStep,
        MeasurementSetBuilder measurementSetBuilder,
        SpecRuleResolver specRuleResolver)
    {
        _evaluateStep = evaluateStep;
        _measurementSetBuilder = measurementSetBuilder;
        _specRuleResolver = specRuleResolver;
    }

    public async Task<FlowExecutionResult> RunAsync(
        RecipeDefinition recipe,
        SpecDocument specDocument,
        IDevice device,
        TestContext context,
        string selectedScriptName,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        ArgumentNullException.ThrowIfNull(specDocument);
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(context);

        var scriptsToRun = string.IsNullOrWhiteSpace(selectedScriptName)
            ? recipe.Scripts
            : recipe.Scripts
                .Where(item => string.Equals(item.Name, selectedScriptName, StringComparison.OrdinalIgnoreCase))
                .ToList();

        context.Log($"Starting recipe '{recipe.Name}' with device '{device.Name}'.");
        await device.ConnectAsync(cancellationToken);
        context.Log("Device connected.");

        var stepResults = new List<StepResult>();
        var scriptResults = new List<ScriptResult>();
        var errors = new List<string>();

        try
        {
            foreach (var definition in scriptsToRun)
            {
                try
                {
                    var script = new RecipeScript(recipe, definition, _measurementSetBuilder);
                    var executionResult = await script.ExecuteAsync(device, context, cancellationToken);
                    context.Data.Set(executionResult.MeasurementSet);

                    var fullKeys = executionResult.MeasurementSet.Items.Select(item => item.FullKey).ToList();
                    var rules = _specRuleResolver.ResolveForStep(recipe, definition, fullKeys, specDocument);
                    var stepResult = _evaluateStep.Execute(
                        recipe,
                        definition,
                        executionResult.MeasurementSet,
                        rules,
                        context);

                    stepResults.Add(stepResult);
                    scriptResults.Add(CreateSummary(stepResult));
                    context.Log(
                        $"Step '{stepResult.StepName}' completed with status '{stepResult.FinalStatus}'.");
                }
                catch (Exception exception)
                {
                    var errorMessage = $"Script '{definition.Name}' failed: {exception.Message}";
                    errors.Add(errorMessage);
                    context.LogError(errorMessage);
                    var failedStep = new StepResult
                    {
                        StepName = definition.Name,
                        Command = definition.Command,
                        Prefix = RecipeStepDefinitionHelper.GetEffectivePrefix(recipe, definition),
                        MeasurementSet = new MeasurementSet
                        {
                            Source = definition.Name,
                            Command = definition.Command,
                            CollectedAt = DateTimeOffset.UtcNow,
                            RawPayload = string.Empty,
                            Items = new List<MeasurementItem>()
                        },
                        Measurements = new List<MeasurementItem>(),
                        SpecResults = new List<SpecEvaluationResult>(),
                        FinalStatus = "Error"
                    };

                    stepResults.Add(failedStep);
                    scriptResults.Add(new ScriptResult
                    {
                        ScriptName = definition.Name,
                        Command = definition.Command,
                        Prefix = RecipeStepDefinitionHelper.GetEffectivePrefix(recipe, definition),
                        MeasurementKey = definition.MeasurementKey,
                        FullKey = string.IsNullOrWhiteSpace(definition.MeasurementKey)
                            ? string.Empty
                            : RecipeStepDefinitionHelper.BuildFullKey(
                                RecipeStepDefinitionHelper.GetEffectivePrefix(recipe, definition),
                                definition.MeasurementKey),
                        SpecKey = definition.SpecKey,
                        RuleName = definition.SpecKey,
                        ActualValue = string.Empty,
                        NumericValue = null,
                        Unit = definition.Unit,
                        Operator = string.Empty,
                        Expected = string.Empty,
                        Minimum = null,
                        Maximum = null,
                        ErrorCode = string.Empty,
                        Status = "Error",
                        Message = exception.Message
                    });
                    break;
                }
            }
        }
        finally
        {
            try
            {
                await device.DisconnectAsync(cancellationToken);
                context.Log("Device disconnected.");
            }
            catch (Exception exception)
            {
                var disconnectError = $"Device disconnect failed: {exception.Message}";
                errors.Add(disconnectError);
                context.LogError(disconnectError);
            }
        }

        var overallStatus = errors.Count > 0
            ? "Error"
            : scriptResults.Any(item => string.Equals(item.Status, "Failed", StringComparison.OrdinalIgnoreCase))
                ? "Failed"
                : "Passed";

        context.Log($"Recipe '{recipe.Name}' completed with status '{overallStatus}'.");

        return new FlowExecutionResult
        {
            DeviceName = device.Name,
            Status = overallStatus,
            Steps = stepResults,
            Scripts = scriptResults,
            Errors = errors
        };
    }

    private ScriptResult CreateSummary(StepResult stepResult)
    {
        var primaryMeasurement = stepResult.Measurements.FirstOrDefault();
        var primarySpecResult = stepResult.SpecResults.FirstOrDefault(item =>
            string.Equals(item.PassFail, "Failed", StringComparison.OrdinalIgnoreCase))
            ?? stepResult.SpecResults.FirstOrDefault();

        return new ScriptResult
        {
            ScriptName = stepResult.StepName,
            Command = stepResult.Command,
            Prefix = stepResult.Prefix,
            MeasurementKey = primaryMeasurement?.Key ?? string.Empty,
            FullKey = primaryMeasurement?.FullKey ?? string.Empty,
            SpecKey = primarySpecResult?.TargetKey ?? string.Empty,
            RuleName = primarySpecResult?.RuleName ?? string.Empty,
            ActualValue = primaryMeasurement?.Value ?? string.Empty,
            NumericValue = primaryMeasurement is null
                ? null
                : decimal.TryParse(primaryMeasurement.Value, out var parsedValue)
                    ? parsedValue
                    : null,
            Unit = primaryMeasurement?.Unit ?? string.Empty,
            Operator = primarySpecResult?.RuleType ?? string.Empty,
            Expected = primarySpecResult?.Reason ?? string.Empty,
            Minimum = null,
            Maximum = null,
            ErrorCode = primarySpecResult?.ErrorCode ?? string.Empty,
            Status = stepResult.FinalStatus,
            Message = primarySpecResult?.Reason ?? $"{stepResult.StepName} completed with status '{stepResult.FinalStatus}'."
        };
    }
}
