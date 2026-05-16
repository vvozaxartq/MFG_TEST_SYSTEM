using ATS.Application.Recipes;
using ATS.Application.Scripts;
using ATS.Application.Specs;
using ATS.Core.Devices;
using ATS.Core.Models;

namespace ATS.Application.Flow;

internal sealed class FlowStepExecutor
{
    private readonly EvaluateStep _evaluateStep;
    private readonly SpecRuleResolver _specRuleResolver;

    public FlowStepExecutor(
        EvaluateStep evaluateStep,
        SpecRuleResolver specRuleResolver)
    {
        _evaluateStep = evaluateStep;
        _specRuleResolver = specRuleResolver;
    }

    public async Task<FlowStepExecutionOutcome> ExecuteAsync(
        RecipeDefinition recipe,
        RecipeScriptDefinition definition,
        RecipeScriptDefinition resolvedDefinition,
        SpecDocument specDocument,
        RecipeScript script,
        IDeviceSession deviceSession,
        TestContext context,
        DutExecutionRuntime dutRuntime,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(resolvedDefinition);
        ArgumentNullException.ThrowIfNull(specDocument);
        ArgumentNullException.ThrowIfNull(script);
        ArgumentNullException.ThrowIfNull(deviceSession);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(dutRuntime);

        var policy = BuildPolicy(definition);
        dutRuntime.State.ActiveStepName = resolvedDefinition.Name;

        Exception? lastException = null;
        for (var attemptNumber = 1; attemptNumber <= policy.MaxAttempts; attemptNumber++)
        {
            using var timeoutCts = policy.TimeoutMs > 0
                ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
                : null;
            if (timeoutCts is not null)
            {
                timeoutCts.CancelAfter(policy.TimeoutMs);
            }

            var executionToken = timeoutCts?.Token ?? cancellationToken;

            try
            {
                var executionResult = await script.ExecuteAsync(
                    new ScriptExecutionRequest
                    {
                        StepName = resolvedDefinition.Name,
                        Command = resolvedDefinition.Command,
                        SimulatedResponse = script.SimulatedResponse,
                        AttemptNumber = attemptNumber,
                        Policy = policy,
                        DutRuntime = dutRuntime,
                        Context = context,
                        DeviceSession = deviceSession
                    },
                    executionToken);

                context.Data.Set(executionResult.MeasurementSet);
                foreach (var measurement in executionResult.MeasurementSet.Items)
                {
                    context.LogEvent(
                        "INFO",
                        StructuredLogEntryType.DataCollectionWrite,
                        $"DataCollection stored '{measurement.FullKey}' = '{measurement.Value}' using last-write-wins.",
                        definition.Name,
                        stepName: definition.Name,
                        fullKey: measurement.FullKey,
                        dutId: dutRuntime.Metadata.Id,
                        status: "Stored",
                        data: new Dictionary<string, object?>
                        {
                            ["fullKey"] = measurement.FullKey,
                            ["value"] = measurement.Value,
                            ["writeMode"] = "LastWriteWins",
                            ["attemptNumber"] = attemptNumber
                        });
                }

                var fullKeys = executionResult.MeasurementSet.Items.Select(item => item.FullKey).ToList();
                var rules = _specRuleResolver.ResolveForStep(recipe, resolvedDefinition, fullKeys, specDocument);
                var evaluatedStep = _evaluateStep.Execute(
                    recipe,
                    resolvedDefinition,
                    executionResult.MeasurementSet,
                    rules,
                    executionResult.StartedAtUtc,
                    executionResult.CompletedAtUtc,
                    context);

                var stepResult = new StepResult
                {
                    StepName = evaluatedStep.StepName,
                    Command = evaluatedStep.Command,
                    Prefix = evaluatedStep.Prefix,
                    MeasurementSet = evaluatedStep.MeasurementSet,
                    Measurements = evaluatedStep.Measurements,
                    SpecResults = evaluatedStep.SpecResults,
                    FinalStatus = evaluatedStep.FinalStatus,
                    AttemptCount = executionResult.AttemptCount,
                    StartedAtUtc = evaluatedStep.StartedAtUtc,
                    CompletedAtUtc = evaluatedStep.CompletedAtUtc
                };

                dutRuntime.State.CompletedStepCount += 1;
                dutRuntime.State.ActiveStepName = string.Empty;
                dutRuntime.State.LastStepName = stepResult.StepName;
                dutRuntime.State.LastStepStatus = stepResult.FinalStatus;
                dutRuntime.State.LastError = string.Empty;
                LogStepCompleted(context, stepResult, dutRuntime.Metadata.Id, attemptNumber);

                return new FlowStepExecutionOutcome(stepResult, string.Empty, ContinueFlow: true);
            }
            catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested && policy.TimeoutMs > 0)
            {
                lastException = exception;
                var timeoutMessage = $"Step '{resolvedDefinition.Name}' exceeded timeout of {policy.TimeoutMs} ms on attempt {attemptNumber}.";
                context.LogError(timeoutMessage, resolvedDefinition.Name);
                context.LogEvent(
                    "ERROR",
                    StructuredLogEntryType.StepTimedOut,
                    timeoutMessage,
                    resolvedDefinition.Name,
                    stepName: resolvedDefinition.Name,
                    dutId: dutRuntime.Metadata.Id,
                    status: "TimedOut",
                    data: new Dictionary<string, object?>
                    {
                        ["attemptNumber"] = attemptNumber,
                        ["timeoutMs"] = policy.TimeoutMs
                    });

                if (attemptNumber < policy.MaxAttempts)
                {
                    LogStepRetry(context, resolvedDefinition.Name, attemptNumber, policy.MaxAttempts, timeoutMessage, dutRuntime.Metadata.Id);
                    continue;
                }

                var timeoutOutcome = BuildFailureOutcome(recipe, definition, resolvedDefinition, timeoutMessage, attemptNumber, policy.ContinueOnFailure, dutRuntime);
                LogStepCompleted(context, timeoutOutcome.StepResult, dutRuntime.Metadata.Id, attemptNumber);
                return timeoutOutcome;
            }
            catch (Exception exception)
            {
                lastException = exception;
                var errorMessage = $"Script '{resolvedDefinition.Name}' failed: {exception.Message}";
                context.LogError(errorMessage, resolvedDefinition.Name);

                if (attemptNumber < policy.MaxAttempts)
                {
                    LogStepRetry(context, resolvedDefinition.Name, attemptNumber, policy.MaxAttempts, exception.Message, dutRuntime.Metadata.Id);
                    continue;
                }

                var failureOutcome = BuildFailureOutcome(recipe, definition, resolvedDefinition, exception.Message, attemptNumber, policy.ContinueOnFailure, dutRuntime);
                LogStepCompleted(context, failureOutcome.StepResult, dutRuntime.Metadata.Id, attemptNumber);
                return failureOutcome;
            }
        }

        var finalOutcome = BuildFailureOutcome(
            recipe,
            definition,
            resolvedDefinition,
            lastException?.Message ?? "Unknown step execution error.",
            policy.MaxAttempts,
            policy.ContinueOnFailure,
            dutRuntime);
        LogStepCompleted(context, finalOutcome.StepResult, dutRuntime.Metadata.Id, policy.MaxAttempts);
        return finalOutcome;
    }

    private static StepExecutionPolicy BuildPolicy(RecipeScriptDefinition definition)
    {
        return new StepExecutionPolicy
        {
            RetryCount = Math.Max(0, definition.RetryCount),
            TimeoutMs = Math.Max(0, definition.TimeoutMs),
            ContinueOnFailure = definition.ContinueOnFailure
        };
    }

    private static FlowStepExecutionOutcome BuildFailureOutcome(
        RecipeDefinition recipe,
        RecipeScriptDefinition definition,
        RecipeScriptDefinition resolvedDefinition,
        string failureMessage,
        int attemptCount,
        bool continueOnFailure,
        DutExecutionRuntime dutRuntime)
    {
        dutRuntime.State.FailedStepCount += 1;
        dutRuntime.State.ActiveStepName = string.Empty;
        dutRuntime.State.LastStepName = definition.Name;
        dutRuntime.State.LastStepStatus = "Error";
        dutRuntime.State.LastError = failureMessage;

        return new FlowStepExecutionOutcome(
            new StepResult
            {
                StepName = definition.Name,
                Command = resolvedDefinition.Command,
                Prefix = RecipeStepDefinitionHelper.GetEffectivePrefix(recipe, definition),
                MeasurementSet = new MeasurementSet
                {
                    Source = definition.Name,
                    Command = resolvedDefinition.Command,
                    CollectedAt = DateTimeOffset.UtcNow,
                    RawPayload = string.Empty,
                    Items = new List<MeasurementItem>()
                },
                Measurements = new List<MeasurementItem>(),
                SpecResults = new List<SpecEvaluationResult>(),
                FinalStatus = "Error",
                AttemptCount = attemptCount,
                FailureMessage = failureMessage,
                StartedAtUtc = DateTimeOffset.UtcNow,
                CompletedAtUtc = DateTimeOffset.UtcNow
            },
            failureMessage,
            continueOnFailure);
    }

    private static void LogStepRetry(
        TestContext context,
        string stepName,
        int attemptNumber,
        int maxAttempts,
        string reason,
        string dutId)
    {
        var message = $"Retrying step '{stepName}' after attempt {attemptNumber}/{maxAttempts} because: {reason}";
        context.Log(message, stepName);
        context.LogEvent(
            "ERROR",
            StructuredLogEntryType.StepRetried,
            message,
            stepName,
            stepName: stepName,
            dutId: dutId,
            status: "Retrying",
            data: new Dictionary<string, object?>
            {
                ["attemptNumber"] = attemptNumber,
                ["maxAttempts"] = maxAttempts,
                ["reason"] = reason
            });
    }

    private static void LogStepCompleted(
        TestContext context,
        StepResult stepResult,
        string dutId,
        int attemptNumber)
    {
        context.Log(
            $"Step '{stepResult.StepName}' completed with status '{stepResult.FinalStatus}' after {attemptNumber} attempt(s).",
            stepResult.StepName);
        context.LogEvent(
            string.Equals(stepResult.FinalStatus, "Failed", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(stepResult.FinalStatus, "Error", StringComparison.OrdinalIgnoreCase)
                ? "ERROR"
                : "INFO",
            StructuredLogEntryType.StepCompleted,
            $"Step '{stepResult.StepName}' completed with status '{stepResult.FinalStatus}'.",
            stepResult.StepName,
            stepName: stepResult.StepName,
            dutId: dutId,
            status: stepResult.FinalStatus,
            data: new Dictionary<string, object?>
            {
                ["stepName"] = stepResult.StepName,
                ["command"] = stepResult.Command,
                ["durationSeconds"] = stepResult.DurationSeconds,
                ["attemptCount"] = stepResult.AttemptCount,
                ["retryCount"] = stepResult.RetryCount
            });
    }
}

internal sealed record FlowStepExecutionOutcome(
    StepResult StepResult,
    string ErrorMessage,
    bool ContinueFlow);
