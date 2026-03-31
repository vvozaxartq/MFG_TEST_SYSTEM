using System.Globalization;
using ATS.Application.Recipes;
using ATS.Core.Devices;
using ATS.Core.Models;
using ATS.Core.Scripts;

namespace ATS.Application.Scripts;

internal sealed class RecipeScript : ScriptBase
{
    public RecipeScript(RecipeScriptDefinition definition)
        : base(
            definition.Name,
            definition.Command,
            definition.MeasurementKey,
            definition.Unit,
            definition.SpecKey,
            ResolveSimulatedResponse(definition))
    {
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

        context.Data.Set(MeasurementKey, response.Response);

        var numericValue = decimal.TryParse(
            response.Response,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var parsedValue)
            ? (decimal?)parsedValue
            : null;

        return new ScriptExecutionResult
        {
            ScriptName = Name,
            Command = Command,
            MeasurementKey = MeasurementKey,
            Unit = Unit,
            SpecKey = SpecKey,
            RawValue = response.Response,
            NumericValue = numericValue
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
