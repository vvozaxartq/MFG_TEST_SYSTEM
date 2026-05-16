using ATS.Application.Measurements;
using ATS.Application.Execution;
using ATS.Application.Recipes;
using ATS.Application.Scripts;
using ATS.Application.Specs;
using ATS.Core.Devices;
using ATS.Core.Models;

namespace ATS.Application.Flow;

internal sealed class FlowNodeExecutor
{
    private readonly MeasurementSetBuilder _measurementSetBuilder;
    private readonly VariableResolver _variableResolver;
    private readonly FlowStepExecutor _flowStepExecutor;
    private readonly FlowConditionEvaluator _conditionEvaluator = new();

    public FlowNodeExecutor(
        MeasurementSetBuilder measurementSetBuilder,
        VariableResolver variableResolver,
        FlowStepExecutor flowStepExecutor)
    {
        _measurementSetBuilder = measurementSetBuilder;
        _variableResolver = variableResolver;
        _flowStepExecutor = flowStepExecutor;
    }

    public async Task<FlowNodeExecutionOutcome> ExecuteSequenceAsync(
        RecipeDefinition recipe,
        SpecDocument specDocument,
        IReadOnlyList<FlowNodeDefinition> nodes,
        IReadOnlyDictionary<string, RecipeScriptDefinition> scriptLookup,
        IDeviceSession deviceSession,
        TestContext context,
        DutExecutionRuntime dutRuntime,
        bool logContainerEvents,
        string containerName,
        string outcomePolicy,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        ArgumentNullException.ThrowIfNull(specDocument);
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(scriptLookup);
        ArgumentNullException.ThrowIfNull(deviceSession);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(dutRuntime);

        var resolvedContainerName = string.IsNullOrWhiteSpace(containerName)
            ? "Sequence"
            : containerName;
        var canonicalOutcomePolicy = CanonicalizeOutcomePolicy(outcomePolicy);
        if (logContainerEvents)
        {
            LogContainerStarted(context, resolvedContainerName, nodes.Count, canonicalOutcomePolicy, dutRuntime.Metadata.Id);
        }

        var outcome = new FlowNodeExecutionOutcome();
        foreach (var node in nodes)
        {
            var childOutcome = await ExecuteNodeAsync(
                recipe,
                specDocument,
                node,
                scriptLookup,
                deviceSession,
                context,
                dutRuntime,
                cancellationToken);

            outcome.Append(childOutcome);
            if (!childOutcome.ContinueFlow)
            {
                outcome.ContinueFlow = false;
                break;
            }

            var policyEvaluation = EvaluateOutcomePolicy(canonicalOutcomePolicy, childOutcome);
            if (policyEvaluation.ShouldTerminate)
            {
                outcome.StopReason = policyEvaluation.StopReason;
                outcome.OutcomePolicy = policyEvaluation.PolicyName;
                outcome.TriggeredByNodeName = ResolveFlowNodeName(node);
                break;
            }
        }

        if (logContainerEvents)
        {
            LogContainerCompleted(context, resolvedContainerName, outcome, dutRuntime.Metadata.Id);
        }

        return outcome;
    }

    private async Task<FlowNodeExecutionOutcome> ExecuteNodeAsync(
        RecipeDefinition recipe,
        SpecDocument specDocument,
        FlowNodeDefinition node,
        IReadOnlyDictionary<string, RecipeScriptDefinition> scriptLookup,
        IDeviceSession deviceSession,
        TestContext context,
        DutExecutionRuntime dutRuntime,
        CancellationToken cancellationToken)
    {
        return node switch
        {
            FlowStepNodeDefinition stepNode => await ExecuteStepNodeAsync(
                recipe,
                specDocument,
                stepNode,
                scriptLookup,
                deviceSession,
                context,
                dutRuntime,
                cancellationToken),
            FlowSequenceNodeDefinition sequenceNode => await ExecuteSequenceNodeAsync(
                recipe,
                specDocument,
                sequenceNode,
                scriptLookup,
                deviceSession,
                context,
                dutRuntime,
                cancellationToken),
            FlowConditionNodeDefinition conditionNode => await ExecuteConditionNodeAsync(
                recipe,
                specDocument,
                conditionNode,
                scriptLookup,
                deviceSession,
                context,
                dutRuntime,
                cancellationToken),
            FlowRepeatUntilNodeDefinition repeatUntilNode => await ExecuteRepeatUntilNodeAsync(
                recipe,
                specDocument,
                repeatUntilNode,
                scriptLookup,
                deviceSession,
                context,
                dutRuntime,
                cancellationToken),
            _ => throw new InvalidOperationException($"Unsupported flow node type '{node.GetType().Name}'.")
        };
    }

    private async Task<FlowNodeExecutionOutcome> ExecuteSequenceNodeAsync(
        RecipeDefinition recipe,
        SpecDocument specDocument,
        FlowSequenceNodeDefinition sequenceNode,
        IReadOnlyDictionary<string, RecipeScriptDefinition> scriptLookup,
        IDeviceSession deviceSession,
        TestContext context,
        DutExecutionRuntime dutRuntime,
        CancellationToken cancellationToken)
    {
        var nodeStartedAtUtc = DateTimeOffset.UtcNow;
        var sequenceName = ResolveSequenceName(sequenceNode);
        var childOutcome = await ExecuteSequenceAsync(
            recipe,
            specDocument,
            sequenceNode.Nodes,
            scriptLookup,
            deviceSession,
            context,
            dutRuntime,
            logContainerEvents: true,
            sequenceName,
            sequenceNode.OutcomePolicy,
            cancellationToken);
        var nodeCompletedAtUtc = DateTimeOffset.UtcNow;

        return CreateWrappedOutcome(
            childOutcome,
            new FlowNodeResult
            {
                NodeKind = "Sequence",
                NodeName = sequenceName,
                Status = childOutcome.DetermineStatus(),
                OutcomePolicy = childOutcome.OutcomePolicy,
                TriggeredByNodeName = childOutcome.TriggeredByNodeName,
                StartedAtUtc = nodeStartedAtUtc,
                CompletedAtUtc = nodeCompletedAtUtc,
                Children = childOutcome.NodeResults.ToList(),
                StopReason = childOutcome.StopReason
            });
    }

    private async Task<FlowNodeExecutionOutcome> ExecuteStepNodeAsync(
        RecipeDefinition recipe,
        SpecDocument specDocument,
        FlowStepNodeDefinition stepNode,
        IReadOnlyDictionary<string, RecipeScriptDefinition> scriptLookup,
        IDeviceSession deviceSession,
        TestContext context,
        DutExecutionRuntime dutRuntime,
        CancellationToken cancellationToken)
    {
        if (!scriptLookup.TryGetValue(stepNode.Step, out var definition))
        {
            throw new InvalidOperationException($"Flow step node references unknown script '{stepNode.Step}'.");
        }

        var activeDefinition = definition;
        var nodeName = ResolveStepNodeName(stepNode);
        try
        {
            var variableContext = FlowRuntimeContextBuilder.BuildVariableContext(recipe, definition, context, dutRuntime.Metadata);
            var resolvedDefinition = _variableResolver.ResolveStepDefinition(
                definition,
                variableContext,
                context,
                variableContext.DutContext.Id);
            activeDefinition = resolvedDefinition;
            var script = new RecipeScript(recipe, resolvedDefinition, _measurementSetBuilder);
            var stepOutcome = await _flowStepExecutor.ExecuteAsync(
                recipe,
                definition,
                resolvedDefinition,
                specDocument,
                script,
                deviceSession,
                context,
                dutRuntime,
                cancellationToken);

            var outcome = new FlowNodeExecutionOutcome();
            outcome.StepResults.Add(stepOutcome.StepResult);
            outcome.Scripts.Add(CreateSummary(stepOutcome.StepResult));
            outcome.NodeResults.Add(
                new FlowNodeResult
                {
                    NodeKind = "Step",
                    NodeName = nodeName,
                    NodeReference = definition.Name,
                    Status = stepOutcome.StepResult.FinalStatus,
                    CountsTowardFinalStatus = stepOutcome.StepResult.CountsTowardFinalStatus,
                    StartedAtUtc = stepOutcome.StepResult.StartedAtUtc,
                    CompletedAtUtc = stepOutcome.StepResult.CompletedAtUtc
                });

            if (!string.IsNullOrWhiteSpace(stepOutcome.ErrorMessage))
            {
                var errorMessage = $"Script '{definition.Name}' failed: {stepOutcome.ErrorMessage}";
                outcome.Errors.Add(errorMessage);
                outcome.ContinueFlow = stepOutcome.ContinueFlow;

                if (stepOutcome.ContinueFlow)
                {
                    context.Log(
                        $"Continuing flow after step '{definition.Name}' failure because continueOnFailure=true.",
                        definition.Name);
                }
            }

            return outcome;
        }
        catch (Exception exception)
        {
            var errorMessage = $"Script '{definition.Name}' failed: {exception.Message}";
            context.LogError(errorMessage, definition.Name);
            dutRuntime.State.ActiveStepName = string.Empty;
            dutRuntime.State.LastStepName = definition.Name;
            dutRuntime.State.LastStepStatus = "Error";
            dutRuntime.State.LastError = exception.Message;
            dutRuntime.State.FailedStepCount += 1;

            var failedStep = new StepResult
            {
                StepName = definition.Name,
                Command = activeDefinition.Command,
                Prefix = RecipeStepDefinitionHelper.GetEffectivePrefix(recipe, definition),
                MeasurementSet = new MeasurementSet
                {
                    Source = definition.Name,
                    Command = activeDefinition.Command,
                    CollectedAt = DateTimeOffset.UtcNow,
                    RawPayload = string.Empty,
                    Items = new List<MeasurementItem>()
                },
                Measurements = new List<MeasurementItem>(),
                SpecResults = new List<SpecEvaluationResult>(),
                FinalStatus = "Error",
                FailureMessage = exception.Message,
                StartedAtUtc = DateTimeOffset.UtcNow,
                CompletedAtUtc = DateTimeOffset.UtcNow
            };

            var outcome = new FlowNodeExecutionOutcome
            {
                ContinueFlow = false
            };
            outcome.StepResults.Add(failedStep);
            outcome.Scripts.Add(CreateSummary(failedStep));
            outcome.NodeResults.Add(
                new FlowNodeResult
                {
                    NodeKind = "Step",
                    NodeName = nodeName,
                    NodeReference = definition.Name,
                    Status = failedStep.FinalStatus,
                    CountsTowardFinalStatus = failedStep.CountsTowardFinalStatus,
                    StartedAtUtc = failedStep.StartedAtUtc,
                    CompletedAtUtc = failedStep.CompletedAtUtc
                });
            outcome.Errors.Add(errorMessage);
            return outcome;
        }
    }

    private async Task<FlowNodeExecutionOutcome> ExecuteConditionNodeAsync(
        RecipeDefinition recipe,
        SpecDocument specDocument,
        FlowConditionNodeDefinition conditionNode,
        IReadOnlyDictionary<string, RecipeScriptDefinition> scriptLookup,
        IDeviceSession deviceSession,
        TestContext context,
        DutExecutionRuntime dutRuntime,
        CancellationToken cancellationToken)
    {
        var nodeStartedAtUtc = DateTimeOffset.UtcNow;
        var evaluationResult = _conditionEvaluator.Evaluate(conditionNode.Condition, context, dutRuntime);
        var conditionName = ResolveConditionName(conditionNode);
        LogBranchEvaluated(context, conditionName, evaluationResult, dutRuntime.Metadata.Id);

        var selectedNodes = evaluationResult.Matched
            ? conditionNode.WhenTrue
            : conditionNode.WhenFalse;
        var selectedBranch = evaluationResult.Matched ? "True" : "False";
        LogBranchSelected(context, conditionName, selectedBranch, selectedNodes.Count, dutRuntime.Metadata.Id);

        var childOutcome = await ExecuteSequenceAsync(
            recipe,
            specDocument,
            selectedNodes,
            scriptLookup,
            deviceSession,
            context,
            dutRuntime,
            logContainerEvents: false,
            string.Empty,
            string.Empty,
            cancellationToken);
        var nodeCompletedAtUtc = DateTimeOffset.UtcNow;

        return CreateWrappedOutcome(
            childOutcome,
            new FlowNodeResult
            {
                NodeKind = "Condition",
                NodeName = conditionName,
                Status = childOutcome.DetermineStatus(),
                StartedAtUtc = nodeStartedAtUtc,
                CompletedAtUtc = nodeCompletedAtUtc,
                Children = childOutcome.NodeResults.ToList(),
                ConditionType = evaluationResult.ConditionType,
                SelectedBranch = selectedBranch
            });
    }

    private async Task<FlowNodeExecutionOutcome> ExecuteRepeatUntilNodeAsync(
        RecipeDefinition recipe,
        SpecDocument specDocument,
        FlowRepeatUntilNodeDefinition repeatUntilNode,
        IReadOnlyDictionary<string, RecipeScriptDefinition> scriptLookup,
        IDeviceSession deviceSession,
        TestContext context,
        DutExecutionRuntime dutRuntime,
        CancellationToken cancellationToken)
    {
        var loopName = ResolveRepeatUntilName(repeatUntilNode);
        var nodeStartedAtUtc = DateTimeOffset.UtcNow;
        LogLoopStarted(context, loopName, repeatUntilNode, dutRuntime.Metadata.Id);

        var aggregateOutcome = new FlowNodeExecutionOutcome();
        var iterationResults = new List<FlowIterationResult>();
        for (var iterationNumber = 1; iterationNumber <= repeatUntilNode.MaxIterations; iterationNumber++)
        {
            var iterationStartedAtUtc = DateTimeOffset.UtcNow;
            LogLoopIterationStarted(context, loopName, iterationNumber, repeatUntilNode.MaxIterations, dutRuntime.Metadata.Id);

            var iterationOutcome = await ExecuteSequenceAsync(
                recipe,
                specDocument,
                repeatUntilNode.Nodes,
                scriptLookup,
                deviceSession,
                context,
                dutRuntime,
                logContainerEvents: false,
                string.Empty,
                string.Empty,
                cancellationToken);

            var iterationStatus = iterationOutcome.DetermineStatus();
            LogLoopIterationCompleted(context, loopName, iterationNumber, iterationOutcome, dutRuntime.Metadata.Id);

            if (!iterationOutcome.ContinueFlow)
            {
                AppendExecutionData(aggregateOutcome, iterationOutcome);
                aggregateOutcome.ContinueFlow = false;
                var failedIterationCompletedAtUtc = DateTimeOffset.UtcNow;
                iterationResults.Add(
                    CreateIterationResult(
                        iterationNumber,
                        iterationStatus,
                        countsTowardFinalStatus: true,
                        iterationStartedAtUtc,
                        failedIterationCompletedAtUtc,
                        iterationOutcome.NodeResults));
                LogLoopCompleted(
                    context,
                    loopName,
                    aggregateOutcome.DetermineStatus(),
                    iterationNumber,
                    "IterationFailure",
                    dutRuntime.Metadata.Id);
                return CreateRepeatUntilOutcome(
                    aggregateOutcome,
                    loopName,
                    repeatUntilNode.MaxIterations,
                    iterationResults,
                    "IterationFailure",
                    string.Empty,
                    string.Empty,
                    nodeStartedAtUtc,
                    failedIterationCompletedAtUtc);
            }

            var evaluationResult = _conditionEvaluator.Evaluate(repeatUntilNode.Until, context, dutRuntime);
            LogLoopConditionEvaluated(context, loopName, iterationNumber, evaluationResult, dutRuntime.Metadata.Id);
            var iterationCompletedAtUtc = DateTimeOffset.UtcNow;

            if (evaluationResult.Matched)
            {
                AppendExecutionData(aggregateOutcome, iterationOutcome);
                iterationResults.Add(
                    CreateIterationResult(
                        iterationNumber,
                        iterationStatus,
                        countsTowardFinalStatus: true,
                        iterationStartedAtUtc,
                        iterationCompletedAtUtc,
                        iterationOutcome.NodeResults));
                LogLoopCompleted(
                    context,
                    loopName,
                    aggregateOutcome.DetermineStatus(),
                    iterationNumber,
                    "ConditionSatisfied",
                    dutRuntime.Metadata.Id);
                return CreateRepeatUntilOutcome(
                    aggregateOutcome,
                    loopName,
                    repeatUntilNode.MaxIterations,
                    iterationResults,
                    "ConditionSatisfied",
                    string.Empty,
                    string.Empty,
                    nodeStartedAtUtc,
                    iterationCompletedAtUtc);
            }

            var policyEvaluation = EvaluateOutcomePolicy(repeatUntilNode.OutcomePolicy, iterationOutcome);
            if (policyEvaluation.ShouldTerminate)
            {
                AppendExecutionData(aggregateOutcome, iterationOutcome);
                iterationResults.Add(
                    CreateIterationResult(
                        iterationNumber,
                        iterationStatus,
                        countsTowardFinalStatus: true,
                        iterationStartedAtUtc,
                        iterationCompletedAtUtc,
                        iterationOutcome.NodeResults));
                LogLoopCompleted(
                    context,
                    loopName,
                    aggregateOutcome.DetermineStatus(),
                    iterationNumber,
                    policyEvaluation.StopReason,
                    dutRuntime.Metadata.Id);
                return CreateRepeatUntilOutcome(
                    aggregateOutcome,
                    loopName,
                    repeatUntilNode.MaxIterations,
                    iterationResults,
                    policyEvaluation.StopReason,
                    policyEvaluation.PolicyName,
                    GetTriggeredByNodeName(iterationOutcome),
                    nodeStartedAtUtc,
                    iterationCompletedAtUtc);
            }

            MarkOutcomeAsTransient(iterationOutcome);
            AppendExecutionData(aggregateOutcome, iterationOutcome, includeErrors: false);
            iterationResults.Add(
                CreateIterationResult(
                    iterationNumber,
                    iterationStatus,
                    countsTowardFinalStatus: false,
                    iterationStartedAtUtc,
                    iterationCompletedAtUtc,
                    iterationOutcome.NodeResults));

            if (iterationNumber == repeatUntilNode.MaxIterations)
            {
                var message =
                    $"RepeatUntil '{loopName}' reached maxIterations={repeatUntilNode.MaxIterations} without satisfying the stop condition.";
                LogLoopMaxIterationsReached(
                    context,
                    loopName,
                    iterationNumber,
                    repeatUntilNode.FailOnMaxIterations,
                    message,
                    dutRuntime.Metadata.Id);

                if (repeatUntilNode.FailOnMaxIterations)
                {
                    aggregateOutcome.Errors.Add(message);
                    aggregateOutcome.ContinueFlow = false;
                }

                LogLoopCompleted(
                    context,
                    loopName,
                    aggregateOutcome.DetermineStatus(),
                    iterationNumber,
                    "MaxIterationsReached",
                    dutRuntime.Metadata.Id);
                return CreateRepeatUntilOutcome(
                    aggregateOutcome,
                    loopName,
                    repeatUntilNode.MaxIterations,
                    iterationResults,
                    "MaxIterationsReached",
                    string.Empty,
                    string.Empty,
                    nodeStartedAtUtc,
                    iterationCompletedAtUtc);
            }
        }

        return CreateRepeatUntilOutcome(
            aggregateOutcome,
            loopName,
            repeatUntilNode.MaxIterations,
            iterationResults,
            "Completed",
            string.Empty,
            string.Empty,
            nodeStartedAtUtc,
            DateTimeOffset.UtcNow);
    }

    private static ScriptResult CreateSummary(StepResult stepResult)
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
            Expected = primarySpecResult?.Expected ?? string.Empty,
            Minimum = primarySpecResult?.Minimum,
            Maximum = primarySpecResult?.Maximum,
            ErrorCode = primarySpecResult?.ErrorCode ?? string.Empty,
            Status = stepResult.FinalStatus,
            Message = primarySpecResult?.Reason ??
                      stepResult.FailureMessage ??
                      $"{stepResult.StepName} completed with status '{stepResult.FinalStatus}'."
        };
    }

    private static void LogContainerStarted(
        TestContext context,
        string containerName,
        int childCount,
        string outcomePolicy,
        string dutId)
    {
        context.Log($"Sequence container '{containerName}' started with {childCount} child node(s).", containerName);
        context.LogEvent(
            "INFO",
            StructuredLogEntryType.ContainerStarted,
            $"Sequence container '{containerName}' started.",
            containerName,
            stepName: containerName,
            dutId: dutId,
            status: "Started",
            data: new Dictionary<string, object?>
            {
                ["containerName"] = containerName,
                ["containerType"] = "Sequence",
                ["childCount"] = childCount,
                ["outcomePolicy"] = outcomePolicy
            });
    }

    private static void LogContainerCompleted(
        TestContext context,
        string containerName,
        FlowNodeExecutionOutcome outcome,
        string dutId)
    {
        var status = outcome.DetermineStatus();
        context.Log($"Sequence container '{containerName}' completed with status '{status}'.", containerName);
        context.LogEvent(
            string.Equals(status, "Passed", StringComparison.OrdinalIgnoreCase) ? "INFO" : "ERROR",
            StructuredLogEntryType.ContainerCompleted,
            $"Sequence container '{containerName}' completed with status '{status}'.",
            containerName,
            stepName: containerName,
            dutId: dutId,
            status: status,
            data: new Dictionary<string, object?>
            {
                ["containerName"] = containerName,
                ["containerType"] = "Sequence",
                ["executedStepCount"] = outcome.StepResults.Count,
                ["errorCount"] = outcome.Errors.Count,
                ["stopReason"] = outcome.StopReason,
                ["outcomePolicy"] = outcome.OutcomePolicy,
                ["triggeredByNodeName"] = outcome.TriggeredByNodeName
            });
    }

    private static void LogBranchEvaluated(
        TestContext context,
        string conditionName,
        FlowConditionEvaluationResult evaluationResult,
        string dutId)
    {
        context.Log($"Branch '{conditionName}' evaluated: {evaluationResult.Message}", conditionName);
        context.LogEvent(
            "INFO",
            StructuredLogEntryType.BranchEvaluated,
            $"Branch '{conditionName}' evaluated.",
            conditionName,
            stepName: conditionName,
            dutId: dutId,
            status: evaluationResult.Matched ? "True" : "False",
            data: new Dictionary<string, object?>
            {
                ["conditionName"] = conditionName,
                ["conditionType"] = evaluationResult.ConditionType,
                ["matched"] = evaluationResult.Matched,
                ["source"] = evaluationResult.Source,
                ["actualValue"] = evaluationResult.ActualValue,
                ["expectedValue"] = evaluationResult.ExpectedValue,
                ["message"] = evaluationResult.Message
            });
    }

    private static void LogBranchSelected(
        TestContext context,
        string conditionName,
        string selectedBranch,
        int selectedNodeCount,
        string dutId)
    {
        context.Log(
            $"Branch '{conditionName}' selected '{selectedBranch}' path with {selectedNodeCount} child node(s).",
            conditionName);
        context.LogEvent(
            "INFO",
            StructuredLogEntryType.BranchSelected,
            $"Branch '{conditionName}' selected '{selectedBranch}' path.",
            conditionName,
            stepName: conditionName,
            dutId: dutId,
            status: selectedBranch,
            data: new Dictionary<string, object?>
            {
                ["conditionName"] = conditionName,
                ["selectedBranch"] = selectedBranch,
                ["selectedNodeCount"] = selectedNodeCount
            });
    }

    private static void LogLoopStarted(
        TestContext context,
        string loopName,
        FlowRepeatUntilNodeDefinition repeatUntilNode,
        string dutId)
    {
        context.Log(
            $"RepeatUntil loop '{loopName}' started with maxIterations={repeatUntilNode.MaxIterations}.",
            loopName);
        context.LogEvent(
            "INFO",
            StructuredLogEntryType.LoopStarted,
            $"RepeatUntil loop '{loopName}' started.",
            loopName,
            stepName: loopName,
            dutId: dutId,
            status: "Started",
            data: new Dictionary<string, object?>
            {
                ["loopName"] = loopName,
                ["maxIterations"] = repeatUntilNode.MaxIterations,
                ["failOnMaxIterations"] = repeatUntilNode.FailOnMaxIterations,
                ["outcomePolicy"] = CanonicalizeOutcomePolicy(repeatUntilNode.OutcomePolicy),
                ["childNodeCount"] = repeatUntilNode.Nodes.Count
            });
    }

    private static void LogLoopIterationStarted(
        TestContext context,
        string loopName,
        int iterationNumber,
        int maxIterations,
        string dutId)
    {
        context.Log(
            $"RepeatUntil loop '{loopName}' iteration {iterationNumber}/{maxIterations} started.",
            loopName);
        context.LogEvent(
            "INFO",
            StructuredLogEntryType.LoopIterationStarted,
            $"RepeatUntil loop '{loopName}' iteration {iterationNumber} started.",
            loopName,
            stepName: loopName,
            dutId: dutId,
            status: "Started",
            data: new Dictionary<string, object?>
            {
                ["loopName"] = loopName,
                ["iterationNumber"] = iterationNumber,
                ["maxIterations"] = maxIterations
            });
    }

    private static void LogLoopIterationCompleted(
        TestContext context,
        string loopName,
        int iterationNumber,
        FlowNodeExecutionOutcome iterationOutcome,
        string dutId)
    {
        var status = iterationOutcome.DetermineStatus();
        context.Log(
            $"RepeatUntil loop '{loopName}' iteration {iterationNumber} completed with status '{status}'.",
            loopName);
        context.LogEvent(
            string.Equals(status, "Passed", StringComparison.OrdinalIgnoreCase) ? "INFO" : "ERROR",
            StructuredLogEntryType.LoopIterationCompleted,
            $"RepeatUntil loop '{loopName}' iteration {iterationNumber} completed with status '{status}'.",
            loopName,
            stepName: loopName,
            dutId: dutId,
            status: status,
            data: new Dictionary<string, object?>
            {
                ["loopName"] = loopName,
                ["iterationNumber"] = iterationNumber,
                ["executedStepCount"] = iterationOutcome.StepResults.Count,
                ["errorCount"] = iterationOutcome.Errors.Count
            });
    }

    private static void LogLoopConditionEvaluated(
        TestContext context,
        string loopName,
        int iterationNumber,
        FlowConditionEvaluationResult evaluationResult,
        string dutId)
    {
        context.Log(
            $"RepeatUntil loop '{loopName}' iteration {iterationNumber} condition evaluated: {evaluationResult.Message}",
            loopName);
        context.LogEvent(
            "INFO",
            StructuredLogEntryType.LoopConditionEvaluated,
            $"RepeatUntil loop '{loopName}' condition evaluated.",
            loopName,
            stepName: loopName,
            dutId: dutId,
            status: evaluationResult.Matched ? "True" : "False",
            data: new Dictionary<string, object?>
            {
                ["loopName"] = loopName,
                ["iterationNumber"] = iterationNumber,
                ["conditionType"] = evaluationResult.ConditionType,
                ["matched"] = evaluationResult.Matched,
                ["source"] = evaluationResult.Source,
                ["actualValue"] = evaluationResult.ActualValue,
                ["expectedValue"] = evaluationResult.ExpectedValue,
                ["message"] = evaluationResult.Message
            });
    }

    private static void LogLoopMaxIterationsReached(
        TestContext context,
        string loopName,
        int iterationNumber,
        bool failOnMaxIterations,
        string message,
        string dutId)
    {
        var level = failOnMaxIterations ? "ERROR" : "INFO";
        if (failOnMaxIterations)
        {
            context.LogError(message, loopName);
        }
        else
        {
            context.Log(message, loopName);
        }

        context.LogEvent(
            level,
            StructuredLogEntryType.LoopMaxIterationsReached,
            message,
            loopName,
            stepName: loopName,
            dutId: dutId,
            status: failOnMaxIterations ? "Failed" : "Stopped",
            data: new Dictionary<string, object?>
            {
                ["loopName"] = loopName,
                ["iterationNumber"] = iterationNumber,
                ["failOnMaxIterations"] = failOnMaxIterations
            });
    }

    private static void LogLoopCompleted(
        TestContext context,
        string loopName,
        string status,
        int iterationCount,
        string stopReason,
        string dutId)
    {
        context.Log(
            $"RepeatUntil loop '{loopName}' completed with status '{status}' after {iterationCount} iteration(s).",
            loopName);
        context.LogEvent(
            string.Equals(status, "Passed", StringComparison.OrdinalIgnoreCase) ? "INFO" : "ERROR",
            StructuredLogEntryType.LoopCompleted,
            $"RepeatUntil loop '{loopName}' completed with status '{status}'.",
            loopName,
            stepName: loopName,
            dutId: dutId,
            status: status,
            data: new Dictionary<string, object?>
            {
                ["loopName"] = loopName,
                ["iterationCount"] = iterationCount,
                ["stopReason"] = stopReason
            });
    }

    private static string ResolveSequenceName(FlowSequenceNodeDefinition sequenceNode)
    {
        return string.IsNullOrWhiteSpace(sequenceNode.Name)
            ? "Sequence"
            : sequenceNode.Name;
    }

    private static string ResolveConditionName(FlowConditionNodeDefinition conditionNode)
    {
        return string.IsNullOrWhiteSpace(conditionNode.Name)
            ? "Condition"
            : conditionNode.Name;
    }

    private static string ResolveStepNodeName(FlowStepNodeDefinition stepNode)
    {
        return string.IsNullOrWhiteSpace(stepNode.Name)
            ? stepNode.Step
            : stepNode.Name;
    }

    private static string ResolveRepeatUntilName(FlowRepeatUntilNodeDefinition repeatUntilNode)
    {
        return string.IsNullOrWhiteSpace(repeatUntilNode.Name)
            ? "RepeatUntil"
            : repeatUntilNode.Name;
    }

    private static string ResolveFlowNodeName(FlowNodeDefinition node)
    {
        return node switch
        {
            FlowStepNodeDefinition stepNode => ResolveStepNodeName(stepNode),
            FlowSequenceNodeDefinition sequenceNode => ResolveSequenceName(sequenceNode),
            FlowConditionNodeDefinition conditionNode => ResolveConditionName(conditionNode),
            FlowRepeatUntilNodeDefinition repeatUntilNode => ResolveRepeatUntilName(repeatUntilNode),
            _ => node.GetType().Name
        };
    }

    private static OutcomePolicyEvaluationResult EvaluateOutcomePolicy(
        string outcomePolicy,
        FlowNodeExecutionOutcome childOutcome)
    {
        var canonicalPolicy = CanonicalizeOutcomePolicy(outcomePolicy);
        var childStatus = childOutcome.DetermineStatus();

        return canonicalPolicy switch
        {
            "breakOnStepFailure" when string.Equals(childStatus, "Failed", StringComparison.OrdinalIgnoreCase) =>
                new OutcomePolicyEvaluationResult(true, canonicalPolicy, "StepFailureBreak"),
            "breakOnStepSuccess" when string.Equals(childStatus, "Passed", StringComparison.OrdinalIgnoreCase) =>
                new OutcomePolicyEvaluationResult(true, canonicalPolicy, "StepSuccessBreak"),
            _ => new OutcomePolicyEvaluationResult(false, canonicalPolicy, string.Empty)
        };
    }

    private static string CanonicalizeOutcomePolicy(string outcomePolicy)
    {
        return NormalizeOutcomePolicy(outcomePolicy) switch
        {
            "breakonstepfailure" => "breakOnStepFailure",
            "breakonstepsuccess" => "breakOnStepSuccess",
            _ => string.Empty
        };
    }

    private static string NormalizeOutcomePolicy(string outcomePolicy)
    {
        return string.Concat((outcomePolicy ?? string.Empty)
                .Where(character => !char.IsWhiteSpace(character) && character is not '-' and not '_'))
            .ToLowerInvariant();
    }

    private static FlowNodeExecutionOutcome CreateWrappedOutcome(
        FlowNodeExecutionOutcome childOutcome,
        FlowNodeResult nodeResult)
    {
        var outcome = new FlowNodeExecutionOutcome
        {
            ContinueFlow = childOutcome.ContinueFlow,
            StopReason = childOutcome.StopReason,
            OutcomePolicy = childOutcome.OutcomePolicy,
            TriggeredByNodeName = childOutcome.TriggeredByNodeName
        };
        outcome.StepResults.AddRange(childOutcome.StepResults);
        outcome.Scripts.AddRange(childOutcome.Scripts);
        outcome.Errors.AddRange(childOutcome.Errors);
        outcome.NodeResults.Add(nodeResult);
        return outcome;
    }

    private static FlowNodeExecutionOutcome CreateRepeatUntilOutcome(
        FlowNodeExecutionOutcome aggregateOutcome,
        string loopName,
        int maxIterations,
        IReadOnlyList<FlowIterationResult> iterationResults,
        string stopReason,
        string outcomePolicy,
        string triggeredByNodeName,
        DateTimeOffset startedAtUtc,
        DateTimeOffset completedAtUtc)
    {
        return CreateWrappedOutcome(
            aggregateOutcome,
            new FlowNodeResult
            {
                NodeKind = "RepeatUntil",
                NodeName = loopName,
                Status = aggregateOutcome.DetermineStatus(),
                OutcomePolicy = outcomePolicy,
                TriggeredByNodeName = triggeredByNodeName,
                StartedAtUtc = startedAtUtc,
                CompletedAtUtc = completedAtUtc,
                Children = iterationResults.SelectMany(item => item.Children).ToList(),
                MaxIterations = maxIterations,
                CompletedIterations = iterationResults.Count,
                StopReason = stopReason,
                Iterations = iterationResults.ToList()
            });
    }

    private static FlowIterationResult CreateIterationResult(
        int iterationNumber,
        string status,
        bool countsTowardFinalStatus,
        DateTimeOffset startedAtUtc,
        DateTimeOffset completedAtUtc,
        IReadOnlyList<FlowNodeResult> children)
    {
        return new FlowIterationResult
        {
            IterationNumber = iterationNumber,
            Status = status,
            CountsTowardFinalStatus = countsTowardFinalStatus,
            StartedAtUtc = startedAtUtc,
            CompletedAtUtc = completedAtUtc,
            Children = children.ToList()
        };
    }

    private static string GetTriggeredByNodeName(FlowNodeExecutionOutcome outcome)
    {
        if (!string.IsNullOrWhiteSpace(outcome.TriggeredByNodeName))
        {
            return outcome.TriggeredByNodeName;
        }

        return outcome.NodeResults.LastOrDefault()?.NodeName ?? string.Empty;
    }

    private static void AppendExecutionData(
        FlowNodeExecutionOutcome aggregateOutcome,
        FlowNodeExecutionOutcome childOutcome,
        bool includeErrors = true)
    {
        aggregateOutcome.StepResults.AddRange(childOutcome.StepResults);
        aggregateOutcome.Scripts.AddRange(childOutcome.Scripts);
        if (includeErrors)
        {
            aggregateOutcome.Errors.AddRange(childOutcome.Errors);
        }
    }

    private static void MarkOutcomeAsTransient(FlowNodeExecutionOutcome outcome)
    {
        for (var index = 0; index < outcome.StepResults.Count; index++)
        {
            var stepResult = outcome.StepResults[index];
            outcome.StepResults[index] = new StepResult
            {
                StepName = stepResult.StepName,
                Command = stepResult.Command,
                Prefix = stepResult.Prefix,
                MeasurementSet = stepResult.MeasurementSet,
                Measurements = stepResult.Measurements,
                SpecResults = stepResult.SpecResults,
                FinalStatus = stepResult.FinalStatus,
                CountsTowardFinalStatus = false,
                AttemptCount = stepResult.AttemptCount,
                FailureMessage = stepResult.FailureMessage,
                StartedAtUtc = stepResult.StartedAtUtc,
                CompletedAtUtc = stepResult.CompletedAtUtc
            };
        }

        for (var index = 0; index < outcome.Scripts.Count; index++)
        {
            var scriptResult = outcome.Scripts[index];
            outcome.Scripts[index] = new ScriptResult
            {
                ScriptName = scriptResult.ScriptName,
                Command = scriptResult.Command,
                Prefix = scriptResult.Prefix,
                MeasurementKey = scriptResult.MeasurementKey,
                FullKey = scriptResult.FullKey,
                SpecKey = scriptResult.SpecKey,
                RuleName = scriptResult.RuleName,
                ActualValue = scriptResult.ActualValue,
                NumericValue = scriptResult.NumericValue,
                Unit = scriptResult.Unit,
                Operator = scriptResult.Operator,
                Expected = scriptResult.Expected,
                Minimum = scriptResult.Minimum,
                Maximum = scriptResult.Maximum,
                ErrorCode = scriptResult.ErrorCode,
                Status = scriptResult.Status,
                CountsTowardFinalStatus = false,
                Message = scriptResult.Message
            };
        }

        for (var index = 0; index < outcome.NodeResults.Count; index++)
        {
            outcome.NodeResults[index] = CloneNodeResultAsTransient(outcome.NodeResults[index]);
        }
    }

    private static FlowNodeResult CloneNodeResultAsTransient(FlowNodeResult nodeResult)
    {
        return new FlowNodeResult
        {
            NodeKind = nodeResult.NodeKind,
            NodeName = nodeResult.NodeName,
            NodeReference = nodeResult.NodeReference,
            Status = nodeResult.Status,
            CountsTowardFinalStatus = false,
            OutcomePolicy = nodeResult.OutcomePolicy,
            TriggeredByNodeName = nodeResult.TriggeredByNodeName,
            StartedAtUtc = nodeResult.StartedAtUtc,
            CompletedAtUtc = nodeResult.CompletedAtUtc,
            Children = nodeResult.Children.Select(CloneNodeResultAsTransient).ToList(),
            ConditionType = nodeResult.ConditionType,
            SelectedBranch = nodeResult.SelectedBranch,
            MaxIterations = nodeResult.MaxIterations,
            CompletedIterations = nodeResult.CompletedIterations,
            StopReason = nodeResult.StopReason,
            Iterations = nodeResult.Iterations.Select(CloneIterationResultAsTransient).ToList()
        };
    }

    private static FlowIterationResult CloneIterationResultAsTransient(FlowIterationResult iterationResult)
    {
        return new FlowIterationResult
        {
            IterationNumber = iterationResult.IterationNumber,
            Status = iterationResult.Status,
            CountsTowardFinalStatus = false,
            StartedAtUtc = iterationResult.StartedAtUtc,
            CompletedAtUtc = iterationResult.CompletedAtUtc,
            Children = iterationResult.Children.Select(CloneNodeResultAsTransient).ToList()
        };
    }
}

internal sealed record OutcomePolicyEvaluationResult(
    bool ShouldTerminate,
    string PolicyName,
    string StopReason);

internal sealed class FlowNodeExecutionOutcome
{
    public List<StepResult> StepResults { get; } = new();

    public List<ScriptResult> Scripts { get; } = new();

    public List<string> Errors { get; } = new();

    public List<FlowNodeResult> NodeResults { get; } = new();

    public bool ContinueFlow { get; set; } = true;

    public string StopReason { get; set; } = string.Empty;

    public string OutcomePolicy { get; set; } = string.Empty;

    public string TriggeredByNodeName { get; set; } = string.Empty;

    public void Append(FlowNodeExecutionOutcome childOutcome, bool includeErrors = true)
    {
        StepResults.AddRange(childOutcome.StepResults);
        Scripts.AddRange(childOutcome.Scripts);
        NodeResults.AddRange(childOutcome.NodeResults);
        if (includeErrors)
        {
            Errors.AddRange(childOutcome.Errors);
        }
    }

    public string DetermineStatus()
    {
        if (Errors.Count > 0 || StepResults.Any(item =>
                item.CountsTowardFinalStatus &&
                string.Equals(item.FinalStatus, "Error", StringComparison.OrdinalIgnoreCase)))
        {
            return "Error";
        }

        return StepResults.Any(item =>
                item.CountsTowardFinalStatus &&
                string.Equals(item.FinalStatus, "Failed", StringComparison.OrdinalIgnoreCase))
            ? "Failed"
            : "Passed";
    }
}
