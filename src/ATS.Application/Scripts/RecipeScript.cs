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
        ScriptExecutionRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var context = request.Context;
        var startedAtUtc = DateTimeOffset.UtcNow;
        context.Log(
            $"Executing script '{Name}' ({Command}) attempt {request.AttemptNumber}/{request.Policy.MaxAttempts}.",
            Name);
        context.LogEvent(
            "INFO",
            StructuredLogEntryType.StepStarted,
            $"Executing script '{Name}' ({Command}) attempt {request.AttemptNumber}/{request.Policy.MaxAttempts}.",
            Name,
            stepName: Name,
            status: "Started",
            data: new Dictionary<string, object?>
            {
                ["command"] = Command,
                ["attemptNumber"] = request.AttemptNumber,
                ["maxAttempts"] = request.Policy.MaxAttempts,
                ["timeoutMs"] = request.Policy.TimeoutMs,
                ["continueOnFailure"] = request.Policy.ContinueOnFailure
            });

        var response = await request.DeviceSession.ExecuteAsync(
            new DeviceCommandRequest
            {
                Command = request.Command,
                SimulatedResponse = request.SimulatedResponse
            },
            cancellationToken);

        if (!response.Success)
        {
            throw new InvalidOperationException(response.Message);
        }

        var collectedAt = DateTimeOffset.UtcNow;
        var measurementSet = _measurementSetBuilder.Build(_recipe, _definition, response.Response, collectedAt);
        var completedAtUtc = DateTimeOffset.UtcNow;

        return new ScriptExecutionResult
        {
            ScriptName = Name,
            Command = request.Command,
            Prefix = _definition.Prefix,
            SpecKey = SpecKey,
            StartedAtUtc = startedAtUtc,
            CompletedAtUtc = completedAtUtc,
            AttemptCount = request.AttemptNumber,
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
