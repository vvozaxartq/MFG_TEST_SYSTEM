using System.Globalization;
using ATS.Application.Measurements;
using ATS.Application.Recipes;
using ATS.Core.Devices;
using ATS.Core.Models;
using ATS.Core.Scripts;

namespace ATS.Application.Scripts;

internal sealed class RecipeScript : ScriptBase
{
    private readonly RecipeScriptDefinition _definition;
    private readonly MeasurementSetBuilder _measurementSetBuilder;
    private readonly RecipeDefinition _recipe;

    public RecipeScript(
        RecipeDefinition recipe,
        RecipeScriptDefinition definition,
        MeasurementSetBuilder measurementSetBuilder)
        : base(
            definition.Name,
            definition.Command,
            definition.MeasurementKey,
            definition.Unit,
            definition.SpecKey,
            ResolveSimulatedResponse(definition))
    {
        _definition = definition;
        _recipe = recipe;
        _measurementSetBuilder = measurementSetBuilder;
    }

    public override async Task<ScriptExecutionResult> ExecuteAsync(
        IDevice device,
        TestContext context,
        CancellationToken cancellationToken)
    {
        context.Log($"Executing script '{Name}' ({Command}).");
        var response = await device.ExecuteAsync(
            new DeviceCommandRequest
            {
                Command = Command,
                SimulatedResponse = SimulatedResponse
            },
            cancellationToken);

        if (!response.Success)
        {
            throw new InvalidOperationException(response.Message);
        }

        var collectedAt = DateTimeOffset.UtcNow;
        var measurementSet = _measurementSetBuilder.Build(_recipe, _definition, response.Response, collectedAt);

        return new ScriptExecutionResult
        {
            ScriptName = Name,
            Command = Command,
            Prefix = _definition.Prefix,
            SpecKey = SpecKey,
            MeasurementSet = measurementSet
        };
    }

    private static string ResolveSimulatedResponse(RecipeScriptDefinition definition)
    {
        if (!string.IsNullOrWhiteSpace(definition.SimulatedResponse))
        {
            return definition.SimulatedResponse;
        }

        if (definition.SimulatedValue.HasValue)
        {
            return definition.SimulatedValue.Value.ToString(CultureInfo.InvariantCulture);
        }

        return string.Empty;
    }
}
