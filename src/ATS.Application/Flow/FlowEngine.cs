using ATS.Application.Recipes;
using ATS.Application.Scripts;
using ATS.Application.Specs;
using ATS.Core.Devices;
using ATS.Core.Models;

namespace ATS.Application.Flow;

public sealed class FlowEngine
{
    private readonly SpecEngine _specEngine;

    public FlowEngine(SpecEngine specEngine)
    {
        _specEngine = specEngine;
    }

    public async Task<FlowExecutionResult> RunAsync(
        RecipeDefinition recipe,
        IReadOnlyDictionary<string, SpecDefinition> specsByKey,
        IDevice device,
        TestContext context,
        string selectedScriptName,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        ArgumentNullException.ThrowIfNull(specsByKey);
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

        var scriptResults = new List<ScriptResult>();
        var errors = new List<string>();

        try
        {
            foreach (var definition in scriptsToRun)
            {
                try
                {
                    var script = new RecipeScript(definition);
                    var executionResult = await script.ExecuteAsync(device, context, cancellationToken);

                    if (!specsByKey.TryGetValue(executionResult.SpecKey, out var spec))
                    {
                        throw new InvalidOperationException($"Spec '{executionResult.SpecKey}' was not found.");
                    }

                    var evaluatedResult = _specEngine.Evaluate(executionResult, spec);
                    scriptResults.Add(evaluatedResult);
                    context.Log(
                        $"Script '{evaluatedResult.ScriptName}' produced '{evaluatedResult.ActualValue}' and {evaluatedResult.Status.ToLowerInvariant()}.");
                }
                catch (Exception exception)
                {
                    var errorMessage = $"Script '{definition.Name}' failed: {exception.Message}";
                    errors.Add(errorMessage);
                    context.LogError(errorMessage);
                    scriptResults.Add(new ScriptResult
                    {
                        ScriptName = definition.Name,
                        Command = definition.Command,
                        MeasurementKey = definition.MeasurementKey,
                        SpecKey = definition.SpecKey,
                        ActualValue = string.Empty,
                        NumericValue = null,
                        Unit = definition.Unit,
                        Operator = string.Empty,
                        Expected = string.Empty,
                        Minimum = null,
                        Maximum = null,
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
            Scripts = scriptResults,
            Errors = errors
        };
    }
}
