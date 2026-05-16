using ATS.Application.Measurements;
using ATS.Application.Recipes;
using ATS.Application.Scripts;
using ATS.Application.Specs;
using ATS.Application.Execution;
using ATS.Application.Devices;
using ATS.Core.Devices;
using ATS.Core.Models;

namespace ATS.Application.Flow;

public sealed class FlowEngine
{
    private readonly FlowNodeExecutor _flowNodeExecutor;

    public FlowEngine(SpecEngine specEngine)
        : this(
            specEngine,
            new EvaluateStep(specEngine),
            new MeasurementSetBuilder(),
            new SpecRuleResolver(),
            new VariableResolver())
    {
    }

    internal FlowEngine(
        SpecEngine specEngine,
        EvaluateStep evaluateStep,
        MeasurementSetBuilder measurementSetBuilder,
        SpecRuleResolver specRuleResolver,
        VariableResolver variableResolver)
    {
        var flowStepExecutor = new FlowStepExecutor(evaluateStep, specRuleResolver);
        _flowNodeExecutor = new FlowNodeExecutor(measurementSetBuilder, variableResolver, flowStepExecutor);
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

        context.Log($"Starting recipe '{recipe.Name}' with device '{device.Name}'.", recipe.Name);
        context.LogEvent(
            "INFO",
            StructuredLogEntryType.RecipeStarted,
            $"Starting recipe '{recipe.Name}' with device '{device.Name}'.",
            recipe.Name,
            status: "Started",
            data: new Dictionary<string, object?>
            {
                ["recipeName"] = recipe.Name,
                ["deviceName"] = device.Name
            });

        var stepResults = new List<StepResult>();
        var scriptResults = new List<ScriptResult>();
        var errors = new List<string>();
        FlowNodeResult? flowResultTree = null;
        var dutRuntime = new DutExecutionRuntime
        {
            Metadata = FlowRuntimeContextBuilder.BuildDutContext(context)
        };
        var scriptLookup = recipe.Scripts
            .Where(item => !string.IsNullOrWhiteSpace(item.Name))
            .ToDictionary(item => item.Name, StringComparer.OrdinalIgnoreCase);

        var deviceSession = new DeviceSession(device);
        var deviceConnected = false;
        try
        {
            await deviceSession.ConnectAsync(cancellationToken);
            deviceConnected = true;
            context.Log("Device connected.", recipe.Name);
            context.LogEvent(
                "INFO",
                StructuredLogEntryType.DeviceConnected,
                $"Device '{device.Name}' connected.",
                recipe.Name,
                status: "Connected",
                data: new Dictionary<string, object?>
                        {
                            ["deviceName"] = device.Name
                        });

            if (recipe.Flow is not null && string.IsNullOrWhiteSpace(selectedScriptName))
            {
                var flowStartedAtUtc = DateTimeOffset.UtcNow;
                var flowOutcome = await _flowNodeExecutor.ExecuteSequenceAsync(
                    recipe,
                    specDocument,
                    recipe.Flow.Nodes,
                    scriptLookup,
                    deviceSession,
                    context,
                    dutRuntime,
                    logContainerEvents: true,
                    containerName: ResolveRootContainerName(recipe),
                    outcomePolicy: recipe.Flow.OutcomePolicy,
                    cancellationToken);
                var flowCompletedAtUtc = DateTimeOffset.UtcNow;

                stepResults.AddRange(flowOutcome.StepResults);
                scriptResults.AddRange(flowOutcome.Scripts);
                errors.AddRange(flowOutcome.Errors);
                flowResultTree = BuildExplicitRootFlowResultTree(recipe, flowOutcome, flowStartedAtUtc, flowCompletedAtUtc);
            }
            else
            {
                var rootNodes = BuildImplicitRootNodes(recipe, selectedScriptName);
                var flowStartedAtUtc = DateTimeOffset.UtcNow;
                var flowOutcome = await _flowNodeExecutor.ExecuteSequenceAsync(
                    recipe,
                    specDocument,
                    rootNodes,
                    scriptLookup,
                    deviceSession,
                    context,
                    dutRuntime,
                    logContainerEvents: false,
                    containerName: string.Empty,
                    outcomePolicy: string.Empty,
                    cancellationToken);
                var flowCompletedAtUtc = DateTimeOffset.UtcNow;

                stepResults.AddRange(flowOutcome.StepResults);
                scriptResults.AddRange(flowOutcome.Scripts);
                errors.AddRange(flowOutcome.Errors);
                flowResultTree = BuildImplicitFlowResultTree(recipe, selectedScriptName, flowOutcome, flowStartedAtUtc, flowCompletedAtUtc);
            }
        }
        finally
        {
            try
            {
                await deviceSession.DisposeAsync();

                if (deviceConnected)
                {
                    context.Log("Device disconnected.", recipe.Name);
                    context.LogEvent(
                        "INFO",
                        StructuredLogEntryType.DeviceDisconnected,
                        $"Device '{device.Name}' disconnected.",
                        recipe.Name,
                        status: "Disconnected",
                        data: new Dictionary<string, object?>
                        {
                            ["deviceName"] = device.Name
                        });
                }
            }
            catch (Exception exception)
            {
                var disconnectError = $"Device disconnect failed: {exception.Message}";
                errors.Add(disconnectError);
                context.LogError(disconnectError, recipe.Name);
            }
        }

        var overallStatus = errors.Count > 0
            ? "Error"
            : scriptResults.Any(item =>
                item.CountsTowardFinalStatus &&
                string.Equals(item.Status, "Failed", StringComparison.OrdinalIgnoreCase))
                ? "Failed"
                : "Passed";

        context.Log(
            $"Recipe '{recipe.Name}' completed with status '{overallStatus}' in {FormatDuration(context.StartedAtUtc, DateTimeOffset.UtcNow)}.",
            recipe.Name);
        context.LogEvent(
            "INFO",
            StructuredLogEntryType.RecipeCompleted,
            $"Recipe '{recipe.Name}' completed with status '{overallStatus}'.",
            recipe.Name,
            status: overallStatus,
            data: new Dictionary<string, object?>
            {
                ["recipeName"] = recipe.Name,
                ["duration"] = FormatDuration(context.StartedAtUtc, DateTimeOffset.UtcNow)
            });

        return new FlowExecutionResult
        {
            DeviceName = device.Name,
            Status = overallStatus,
            Steps = stepResults,
            Scripts = scriptResults,
            FlowResultTree = flowResultTree,
            Errors = errors
        };
    }

    private static string FormatDuration(DateTimeOffset startedAtUtc, DateTimeOffset completedAtUtc)
    {
        var elapsed = completedAtUtc - startedAtUtc;
        return $"{(int)elapsed.TotalHours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}.{elapsed.Milliseconds:000}";
    }

    private static List<FlowNodeDefinition> BuildImplicitRootNodes(RecipeDefinition recipe, string selectedScriptName)
    {
        if (!string.IsNullOrWhiteSpace(selectedScriptName))
        {
            return new List<FlowNodeDefinition>
            {
                new FlowStepNodeDefinition
                {
                    Name = selectedScriptName,
                    Step = selectedScriptName
                }
            };
        }

        return recipe.Scripts
            .Select(item => (FlowNodeDefinition)new FlowStepNodeDefinition
            {
                Name = item.Name,
                Step = item.Name
            })
            .ToList();
    }

    private static string ResolveRootContainerName(RecipeDefinition recipe)
    {
        return recipe.Flow is null || string.IsNullOrWhiteSpace(recipe.Flow.Name)
            ? $"{recipe.Name} Flow"
            : recipe.Flow.Name;
    }

    private static FlowNodeResult BuildExplicitRootFlowResultTree(
        RecipeDefinition recipe,
        FlowNodeExecutionOutcome flowOutcome,
        DateTimeOffset startedAtUtc,
        DateTimeOffset completedAtUtc)
    {
        return new FlowNodeResult
        {
            NodeKind = "Sequence",
            NodeName = ResolveRootContainerName(recipe),
            Status = flowOutcome.DetermineStatus(),
            OutcomePolicy = flowOutcome.OutcomePolicy,
            TriggeredByNodeName = flowOutcome.TriggeredByNodeName,
            StartedAtUtc = startedAtUtc,
            CompletedAtUtc = completedAtUtc,
            Children = flowOutcome.NodeResults.ToList(),
            StopReason = flowOutcome.StopReason
        };
    }

    private static FlowNodeResult? BuildImplicitFlowResultTree(
        RecipeDefinition recipe,
        string selectedScriptName,
        FlowNodeExecutionOutcome flowOutcome,
        DateTimeOffset startedAtUtc,
        DateTimeOffset completedAtUtc)
    {
        if (!string.IsNullOrWhiteSpace(selectedScriptName) && flowOutcome.NodeResults.Count == 1)
        {
            return flowOutcome.NodeResults[0];
        }

        return new FlowNodeResult
        {
            NodeKind = "Sequence",
            NodeName = string.IsNullOrWhiteSpace(selectedScriptName)
                ? $"{recipe.Name} Steps"
                : $"{selectedScriptName} Execution",
            Status = flowOutcome.DetermineStatus(),
            OutcomePolicy = flowOutcome.OutcomePolicy,
            TriggeredByNodeName = flowOutcome.TriggeredByNodeName,
            StartedAtUtc = startedAtUtc,
            CompletedAtUtc = completedAtUtc,
            Children = flowOutcome.NodeResults.ToList(),
            StopReason = flowOutcome.StopReason
        };
    }
}
