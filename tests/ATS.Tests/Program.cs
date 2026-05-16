using ATS.Application.Ai;
using ATS.Application.Execution;
using ATS.Application.Flow;
using ATS.Application.Recipes;
using ATS.Application.Simulation;
using ATS.Application.Specs;
using ATS.Core.Devices;
using ATS.Core.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

return await TestSuite.RunAsync();

internal static class TestSuite
{
    public static async Task<int> RunAsync()
    {
        var failures = new List<string>();

        await RunTestAsync("SpecEngine supports all Phase 2 operators", TestSpecEngineSupportsAllOperatorsAsync, failures);
        await RunTestAsync("Simulation writes expected artifacts", TestSimulationWritesArtifactsAsync, failures);
        await RunTestAsync("TestRunner supports external legacy spec file", TestRunnerSupportsExternalSpecFileAsync, failures);
        await RunTestAsync("TestRunner supports multi-measurement prefixes", TestRunnerSupportsMultiMeasurementPrefixesAsync, failures);
        await RunTestAsync("TestRunner supports all spec types sample", TestRunnerSupportsAllSpecTypesSampleAsync, failures);
        await RunTestAsync("TestRunner returns failed result for all spec types fail sample", TestRunnerReturnsFailedResultForAllSpecTypesFailSampleAsync, failures);
        await RunTestAsync("FullKey prefixes isolate repeated measurement keys", TestFullKeyPrefixesCanCoexistAsync, failures);
        await RunTestAsync("Legacy empty-prefix recipe keeps fullKey equal to key", TestLegacyEmptyPrefixRecipeUsesKeyAsFullKeyAsync, failures);
        await RunTestAsync("ScriptRunner executes selected script only", TestScriptRunnerExecutesSelectedScriptAsync, failures);
        await RunTestAsync("DeviceExecutor writes device artifacts", TestDeviceExecutorWritesArtifactsAsync, failures);
        await RunTestAsync("Recipe validation passes for Phase 2 sample", TestRecipeValidationPassesAsync, failures);
        await RunTestAsync("Spec validation passes for Phase 2 sample", TestSpecValidationPassesAsync, failures);
        await RunTestAsync("Recipe validation passes for multi-measurement sample", TestMultiMeasurementRecipeValidationPassesAsync, failures);
        await RunTestAsync("Spec validation passes for multi-measurement sample", TestMultiMeasurementSpecValidationPassesAsync, failures);
        await RunTestAsync("Recipe validation passes for all spec types sample", TestAllSpecTypesRecipeValidationPassesAsync, failures);
        await RunTestAsync("Spec validation passes for all spec types sample", TestAllSpecTypesSpecValidationPassesAsync, failures);
        await RunTestAsync("Recipe validation fails for duplicate fullKey values", TestDuplicateMeasurementFullKeysFailValidationAsync, failures);
        await RunTestAsync("Recipe validation fails for missing spec targetKey fullKey", TestMissingSpecTargetKeyFailsValidationAsync, failures);
        await RunTestAsync("DataCollection supports concurrent fullKey access", TestDataCollectionSupportsConcurrentFullKeyAccessAsync, failures);
        await RunTestAsync("Session artifact templates resolve paths and create directories", TestSessionArtifactTemplatesResolvePathsAndCreateDirectoriesAsync, failures);
        await RunTestAsync("Repeated runs create unique artifact paths", TestRepeatedRunsCreateUniqueArtifactPathsAsync, failures);
        await RunTestAsync("Session log uses local timestamp elapsed and item name", TestSessionLogUsesReadableTimestampElapsedAndItemNameAsync, failures);
        await RunTestAsync("Structured log uses versioned global sequence", TestStructuredLogUsesVersionedGlobalSequenceAsync, failures);
        await RunTestAsync("SessionInfo is canonical session metadata source", TestSessionInfoIsCanonicalMetadataSourceAsync, failures);
        await RunTestAsync("Flow execution retries transient step and records retry count", TestFlowExecutionRetriesTransientStepAndRecordsRetryCountAsync, failures);
        await RunTestAsync("Flow execution continues after step error when continueOnFailure is enabled", TestFlowExecutionContinuesAfterStepErrorWhenContinueOnFailureIsEnabledAsync, failures);
        await RunTestAsync("Flow execution respects step timeout policy", TestFlowExecutionRespectsStepTimeoutPolicyAsync, failures);
        await RunTestAsync("DeviceExecutor uses injected device factory", TestDeviceExecutorUsesInjectedDeviceFactoryAsync, failures);
        await RunTestAsync("Nested sequence executes in declared order", TestNestedSequenceExecutesInDeclaredOrderAsync, failures);
        await RunTestAsync("Sequence container preserves continue-on-failure behavior", TestSequenceContainerPreservesContinueOnFailureBehaviorAsync, failures);
        await RunTestAsync("Condition node executes true branch", TestConditionNodeExecutesTrueBranchAsync, failures);
        await RunTestAsync("Condition node executes false branch from previous step status", TestConditionNodeExecutesFalseBranchFromPreviousStepStatusAsync, failures);
        await RunTestAsync("Retry behavior works inside condition sequence branch", TestRetryBehaviorWorksInsideConditionSequenceBranchAsync, failures);
        await RunTestAsync("Structured events capture container and branch lifecycle", TestStructuredEventsCaptureContainerAndBranchLifecycleAsync, failures);
        await RunTestAsync("RepeatUntil stops when previous step status becomes passed", TestRepeatUntilStopsWhenPreviousStepStatusBecomesPassedAsync, failures);
        await RunTestAsync("RepeatUntil stops when dataExists becomes true", TestRepeatUntilStopsWhenDataExistsAsync, failures);
        await RunTestAsync("RepeatUntil reports deterministic failure at max iterations", TestRepeatUntilReportsDeterministicFailureAtMaxIterationsAsync, failures);
        await RunTestAsync("RepeatUntil works inside a sequence", TestRepeatUntilWorksInsideSequenceAsync, failures);
        await RunTestAsync("RepeatUntil writes structured loop events", TestRepeatUntilWritesStructuredLoopEventsAsync, failures);
        await RunTestAsync("RepeatUntil breaks on failed iteration when policy is configured", TestRepeatUntilBreaksOnFailedIterationWhenPolicyConfiguredAsync, failures);
        await RunTestAsync("RepeatUntil breaks on passed iteration when policy is configured", TestRepeatUntilBreaksOnPassedIterationWhenPolicyConfiguredAsync, failures);
        await RunTestAsync("RepeatUntil default behavior remains backward compatible without outcome policy", TestRepeatUntilDefaultBehaviorRemainsBackwardCompatibleWithoutOutcomePolicyAsync, failures);
        await RunTestAsync("RepeatUntil policy termination takes precedence over max iterations", TestRepeatUntilPolicyTerminationTakesPrecedenceOverMaxIterationsAsync, failures);
        await RunTestAsync("Sequence breaks on failed child when policy is configured", TestSequenceBreaksOnFailedChildWhenPolicyConfiguredAsync, failures);
        await RunTestAsync("Sequence breaks on passed child when policy is configured", TestSequenceBreaksOnPassedChildWhenPolicyConfiguredAsync, failures);
        await RunTestAsync("Sequence default behavior remains backward compatible without outcome policy", TestSequenceDefaultBehaviorRemainsBackwardCompatibleWithoutOutcomePolicyAsync, failures);
        await RunTestAsync("Sequence outcome policy does not override continue-on-failure error handling", TestSequenceOutcomePolicyDoesNotOverrideContinueOnFailureErrorHandlingAsync, failures);
        await RunTestAsync("Flow result tree captures sequence policy stop reason and executed children", TestFlowResultTreeCapturesSequencePolicyStopReasonAndExecutedChildrenAsync, failures);
        await RunTestAsync("Flow result tree captures nested sequence shape", TestFlowResultTreeCapturesNestedSequenceShapeAsync, failures);
        await RunTestAsync("Flow result tree captures selected condition branch", TestFlowResultTreeCapturesSelectedConditionBranchAsync, failures);
        await RunTestAsync("Flow result tree captures repeat iterations", TestFlowResultTreeCapturesRepeatIterationsAsync, failures);
        await RunTestAsync("Flow result tree captures repeat max-iterations stop reason", TestFlowResultTreeCapturesRepeatMaxIterationsStopReasonAsync, failures);
        await RunTestAsync("Result json emits flow result tree while preserving flat summaries", TestResultJsonEmitsFlowResultTreeAndPreservesFlatSummariesAsync, failures);
        await RunTestAsync("ArtifactSummaryBuilder normalizes run artifacts", TestArtifactSummaryBuilderNormalizesRunArtifactsAsync, failures);
        await RunTestAsync("RuleBasedRunAnalyzer classifies variable resolution failures", TestRuleBasedRunAnalyzerClassifiesVariableResolutionFailuresAsync, failures);
        await RunTestAsync("RuleBasedRunAnalyzer classifies unhandled exceptions", TestRuleBasedRunAnalyzerClassifiesUnhandledExceptionsAsync, failures);
        await RunTestAsync("RuleBasedRunAnalyzer classifies step failures", TestRuleBasedRunAnalyzerClassifiesStepFailuresAsync, failures);
        await RunTestAsync("RuleBasedRunAnalyzer classifies success", TestRuleBasedRunAnalyzerClassifiesSuccessAsync, failures);
        await RunTestAsync("RuleBasedRunAnalyzer uses mixed failure precedence", TestRuleBasedRunAnalyzerUsesMixedFailurePrecedenceAsync, failures);
        await RunTestAsync("RuleBasedRunAnalyzer emits evidence for matched rules", TestRuleBasedRunAnalyzerEmitsEvidenceForMatchedRulesAsync, failures);
        await RunTestAsync("RuleBasedRunAnalyzer keeps precedence and evidence consistent", TestRuleBasedRunAnalyzerKeepsPrecedenceAndEvidenceConsistentAsync, failures);
        await RunTestAsync("AiAnalysisBundleBuilder populates metadata and content", TestAiAnalysisBundleBuilderPopulatesMetadataAndContentAsync, failures);
        await RunTestAsync("AiAnalysisBundleWriter writes bundle json", TestAiAnalysisBundleWriterWritesBundleJsonAsync, failures);
        await RunTestAsync("AiAnalysisHtmlRenderer generates html", TestAiAnalysisHtmlRendererGeneratesHtmlAsync, failures);
        await RunTestAsync("AiAnalysisHtmlRenderer includes required sections", TestAiAnalysisHtmlRendererIncludesRequiredSectionsAsync, failures);
        await RunTestAsync("AiAnalysisHtmlRenderer includes interactive sections", TestAiAnalysisHtmlRendererIncludesInteractiveSectionsAsync, failures);
        await RunTestAsync("AiAnalysisHtmlRenderer renders search and filter markup", TestAiAnalysisHtmlRendererRendersSearchAndFilterMarkupAsync, failures);
        await RunTestAsync("AiAnalysisBundleComparisonBuilder handles same bundle comparison", TestAiAnalysisBundleComparisonBuilderHandlesSameBundleComparisonAsync, failures);
        await RunTestAsync("AiAnalysisBundleComparisonBuilder detects category changes", TestAiAnalysisBundleComparisonBuilderDetectsCategoryChangesAsync, failures);
        await RunTestAsync("AiAnalysisBundleComparisonBuilder detects summary count changes", TestAiAnalysisBundleComparisonBuilderDetectsSummaryCountChangesAsync, failures);
        await RunTestAsync("AiAnalysisBundleComparisonBuilder detects matched rule changes", TestAiAnalysisBundleComparisonBuilderDetectsMatchedRuleChangesAsync, failures);
        await RunTestAsync("AiAnalysisComparisonHtmlRenderer includes required sections", TestAiAnalysisComparisonHtmlRendererIncludesRequiredSectionsAsync, failures);
        await RunTestAsync("AiRegressionChecker returns no regression for success to success", TestAiRegressionCheckerReturnsNoRegressionForSuccessToSuccessAsync, failures);
        await RunTestAsync("AiRegressionChecker detects success to step failure regression", TestAiRegressionCheckerDetectsSuccessToStepFailureRegressionAsync, failures);
        await RunTestAsync("AiRegressionChecker detects success to variable resolution failure regression", TestAiRegressionCheckerDetectsSuccessToVariableResolutionFailureRegressionAsync, failures);
        await RunTestAsync("AiRegressionChecker detects summary count regression", TestAiRegressionCheckerDetectsSummaryCountRegressionAsync, failures);
        await RunTestAsync("AiRegressionHtmlRenderer includes required sections", TestAiRegressionHtmlRendererIncludesRequiredSectionsAsync, failures);
        await RunTestAsync("FakeBundleAnalysisProvider generates deterministic response", TestFakeBundleAnalysisProviderGeneratesDeterministicResponseAsync, failures);
        await RunTestAsync("FakeBundleAnalysisProvider stays consistent with bundle analysis", TestFakeBundleAnalysisProviderStaysConsistentWithBundleAnalysisAsync, failures);
        await RunTestAsync("RunAnalysisService analyzes result json artifacts", TestRunAnalysisServiceAnalyzesResultJsonArtifactsAsync, failures);
        await RunTestAsync("RunAnalysisService analyzes result and events artifacts", TestRunAnalysisServiceAnalyzesResultAndEventsArtifactsAsync, failures);
        await RunTestAsync("RunAnalysisService flags variable resolution failures from events", TestRunAnalysisServiceFlagsVariableResolutionFailuresFromEventsAsync, failures);
        await RunTestAsync("RunAnalysisService flags unhandled exceptions from events", TestRunAnalysisServiceFlagsUnhandledExceptionsFromEventsAsync, failures);
        await RunTestAsync("ArtifactSummaryBuilder merges failed step names from events", TestArtifactSummaryBuilderMergesFailedStepNamesFromEventsAsync, failures);
        await RunTestAsync("Cli ai analyze writes output json artifact", TestCliAiAnalyzeWritesOutputJsonArtifactAsync, failures);
        await RunTestAsync("Cli ai analyze writes bundle json artifact", TestCliAiAnalyzeWritesBundleJsonArtifactAsync, failures);
        await RunTestAsync("Cli ai render writes html artifact", TestCliAiRenderWritesHtmlArtifactAsync, failures);
        await RunTestAsync("Cli ai compare writes html artifact", TestCliAiCompareWritesHtmlArtifactAsync, failures);
        await RunTestAsync("Cli ai regress writes json and html artifacts", TestCliAiRegressWritesJsonAndHtmlArtifactsAsync, failures);
        await RunTestAsync("Cli ai analyze supports fake provider path", TestCliAiAnalyzeSupportsFakeProviderPathAsync, failures);
        await RunTestAsync("Cli ai analyze remains backward compatible without provider", TestCliAiAnalyzeRemainsBackwardCompatibleWithoutProviderAsync, failures);
        await RunTestAsync("Cli ai analyze remains backward compatible after ai render addition", TestCliAiAnalyzeRemainsBackwardCompatibleAfterAiRenderAdditionAsync, failures);
        await RunTestAsync("VariableResolver uses Step Global precedence for plain variables", TestVariableResolverUsesStepGlobalPrecedenceAsync, failures);
        await RunTestAsync("VariableResolver resolves canonical DutContext variables without fallback", TestVariableResolverResolvesDutContextVariablesAsync, failures);
        await RunTestAsync("VariableResolver throws when variable is missing", TestVariableResolverThrowsWhenVariableIsMissingAsync, failures);
        await RunTestAsync("TestRunner resolves command simulated response and measurement templates", TestRunnerResolvesVariableTemplatesAsync, failures);
        await RunTestAsync("Structured log captures variable resolution events", TestStructuredLogCapturesVariableResolutionEventsAsync, failures);
        await RunTestAsync("Recipe validation allows runtime-provided dut variables", TestRecipeValidationAllowsRuntimeProvidedDutVariablesAsync, failures);
        await RunTestAsync("Recipe validation fails for malformed variable template syntax", TestRecipeValidationFailsForMalformedVariableTemplateSyntaxAsync, failures);
        await RunTestAsync("Runtime missing variable produces structured failure event", TestRuntimeMissingVariableProducesStructuredFailureEventAsync, failures);
        await RunTestAsync("Invalid test run preserves session artifacts", TestInvalidRunPreservesArtifactsAsync, failures);

        if (failures.Count == 0)
        {
            Console.WriteLine("All tests passed.");
            return 0;
        }

        foreach (var failure in failures)
        {
            Console.Error.WriteLine(failure);
        }

        return 1;
    }

    private static async Task RunTestAsync(
        string name,
        Func<Task> test,
        List<string> failures)
    {
        try
        {
            await test();
            Console.WriteLine($"PASS {name}");
        }
        catch (Exception exception)
        {
            failures.Add($"FAIL {name}: {exception.Message}");
        }
    }

    private static Task TestSpecEngineSupportsAllOperatorsAsync()
    {
        var engine = new SpecEngine();

        AssertEqual("Passed", Evaluate(engine, "12.3", "Range", min: 11.5m, max: 12.8m).Status, "Range should pass.");
        AssertEqual("Passed", Evaluate(engine, "READY", "Equal", expected: "READY").Status, "Equal should pass.");
        AssertEqual("Passed", Evaluate(engine, "READY", "NotEqual", expected: "FAULT").Status, "NotEqual should pass.");
        AssertEqual("Passed", Evaluate(engine, "35.5", "GreaterThan", expected: "30").Status, "GreaterThan should pass.");
        AssertEqual("Passed", Evaluate(engine, "0.2", "LessThan", expected: "0.5").Status, "LessThan should pass.");
        AssertEqual("Passed", Evaluate(engine, "ATS-FAKE-001", "Regex", expected: "^ATS-FAKE-[0-9]{3}$").Status, "Regex should pass.");
        AssertEqual("Passed", Evaluate(engine, "MFG TEST SYSTEM", "Contain", expected: "TEST").Status, "Contain should pass.");
        AssertEqual("Passed", Evaluate(engine, "IGNORED", "Bypass").Status, "Bypass should pass.");

        return Task.CompletedTask;
    }

    private static async Task TestSimulationWritesArtifactsAsync()
    {
        var repositoryRoot = FindRepositoryRoot();
        var recipePath = Path.Combine(repositoryRoot, "samples", "recipes", "demo.recipe.json");
        var outputDirectory = CreateOutputDirectory("simulate");
        var service = new TestSimulationService();

        var result = await service.RunAsync(recipePath, outputDirectory, null, null, CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected demo recipe to pass.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestRunnerSupportsExternalSpecFileAsync()
    {
        var repositoryRoot = FindRepositoryRoot();
        var recipePath = Path.Combine(repositoryRoot, "samples", "recipes", "phase2.recipe.json");
        var specPath = Path.Combine(repositoryRoot, "samples", "specs", "phase2.spec.json");
        var outputDirectory = CreateOutputDirectory("test-run");
        var runner = new TestRunner();

        var result = await runner.RunAsync(
            new TestRunRequest("test run", recipePath, specPath, outputDirectory, string.Empty),
            CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected Phase 2 test run to pass.");
        AssertEqual("9", result.Scripts.Count.ToString(), "Expected all Phase 2 scripts to run.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestRunnerSupportsMultiMeasurementPrefixesAsync()
    {
        var repositoryRoot = FindRepositoryRoot();
        var recipePath = Path.Combine(repositoryRoot, "samples", "recipes", "multi-measurement.recipe.json");
        var specPath = Path.Combine(repositoryRoot, "samples", "specs", "multi-measurement.spec.json");
        var outputDirectory = CreateOutputDirectory("multi-measurement");
        var runner = new TestRunner();

        var result = await runner.RunAsync(
            new TestRunRequest("test run", recipePath, specPath, outputDirectory, string.Empty),
            CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected multi-measurement run to pass.");
        AssertEqual("2", result.Steps.Count.ToString(), "Expected two step results.");
        AssertEqual("4", result.Steps.SelectMany(item => item.Measurements).Count().ToString(), "Expected four measurements.");
        AssertTrue(
            result.Steps.SelectMany(item => item.Measurements).Any(item => item.FullKey == "battery.voltage"),
            "Expected battery.voltage fullKey.");
        AssertTrue(
            result.Steps.SelectMany(item => item.SpecResults).Any(item => item.TargetKey == "load.current"),
            "Expected load.current spec evaluation.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestScriptRunnerExecutesSelectedScriptAsync()
    {
        var repositoryRoot = FindRepositoryRoot();
        var recipePath = Path.Combine(repositoryRoot, "samples", "recipes", "phase2.recipe.json");
        var specPath = Path.Combine(repositoryRoot, "samples", "specs", "phase2.spec.json");
        var outputDirectory = CreateOutputDirectory("script-run");
        var runner = new ScriptRunner();

        var result = await runner.RunAsync(
            recipePath,
            specPath,
            "ReadSerial",
            outputDirectory,
            null,
            null,
            CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected selected script to pass.");
        AssertEqual("1", result.Steps.Count.ToString(), "Expected only one step result.");
        AssertEqual("ReadSerial", result.Steps[0].StepName, "Expected selected script name to match.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestFullKeyPrefixesCanCoexistAsync()
    {
        var outputDirectory = CreateOutputDirectory("fullkey-prefixes");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "fullkey-prefixes.recipe.json",
            """
            {
              "name": "FullKey Prefix Recipe",
              "scripts": [
                {
                  "name": "ReadBatteryVoltage",
                  "command": "READ_VOLTAGE",
                  "prefix": "battery",
                  "measurementKey": "voltage",
                  "unit": "V",
                  "simulatedResponse": "12.3"
                },
                {
                  "name": "ReadUsbVoltage",
                  "command": "READ_VOLTAGE",
                  "prefix": "usb",
                  "measurementKey": "voltage",
                  "unit": "V",
                  "simulatedResponse": "5.1"
                }
              ],
              "rules": [
                {
                  "name": "Battery Voltage",
                  "targetKey": "battery.voltage",
                  "ruleType": "Range",
                  "min": 11.5,
                  "max": 12.8
                },
                {
                  "name": "Usb Voltage",
                  "targetKey": "usb.voltage",
                  "ruleType": "Range",
                  "min": 4.8,
                  "max": 5.2
                }
              ]
            }
            """);

        var runner = new TestRunner();
        var result = await runner.RunAsync(
            new TestRunRequest("test run", recipePath, string.Empty, outputDirectory, string.Empty),
            CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected prefixed fullKey recipe to pass.");
        AssertTrue(
            result.Steps.SelectMany(item => item.Measurements).Any(item => item.FullKey == "battery.voltage"),
            "Expected battery.voltage fullKey.");
        AssertTrue(
            result.Steps.SelectMany(item => item.Measurements).Any(item => item.FullKey == "usb.voltage"),
            "Expected usb.voltage fullKey.");
        AssertTrue(
            result.Steps.SelectMany(item => item.SpecResults).Any(item => item.TargetKey == "battery.voltage"),
            "Expected spec evaluation to use battery.voltage targetKey.");
        AssertTrue(
            result.Steps.SelectMany(item => item.SpecResults).Any(item => item.TargetKey == "usb.voltage"),
            "Expected spec evaluation to use usb.voltage targetKey.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestRunnerSupportsAllSpecTypesSampleAsync()
    {
        var repositoryRoot = FindRepositoryRoot();
        var recipePath = Path.Combine(repositoryRoot, "samples", "recipes", "all-spec-types.recipe.json");
        var specPath = Path.Combine(repositoryRoot, "samples", "specs", "all-spec-types.spec.json");
        var outputDirectory = CreateOutputDirectory("all-spec-types");
        var runner = new TestRunner();

        var result = await runner.RunAsync(
            new TestRunRequest("test run", recipePath, specPath, outputDirectory, string.Empty),
            CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected all spec types sample to pass.");
        AssertEqual("3", result.Steps.Count.ToString(), "Expected three step results.");
        AssertTrue(
            result.Steps.SelectMany(item => item.SpecResults).Any(item => item.RuleType == "Bypass"),
            "Expected bypass rule in spec results.");
        AssertTrue(
            result.Steps.SelectMany(item => item.SpecResults)
                .Any(item => item.TargetKey == "battery.voltage" &&
                             item.Minimum == 11.5m &&
                             item.Maximum == 12.8m),
            "Expected range rule to expose configured minimum and maximum.");
        AssertTrue(
            result.Steps.SelectMany(item => item.SpecResults)
                .Any(item => item.TargetKey == "dut.serial" &&
                             item.Pattern == "^ATS-FAKE-[0-9]{3}$"),
            "Expected regex rule to expose configured pattern.");
        AssertTrue(
            result.Steps.SelectMany(item => item.SpecResults).Any(item => item.RuleType == "Regex"),
            "Expected regex rule in spec results.");
        AssertTrue(
            result.Steps.SelectMany(item => item.SpecResults).Any(item => item.TargetKey == "dut.serial"),
            "Expected recipe prefix fallback fullKey.");
        AssertTrue(
            result.Steps.SelectMany(item => item.SpecResults).Any(item => item.TargetKey == "battery.voltage"),
            "Expected step prefix override fullKey.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestLegacyEmptyPrefixRecipeUsesKeyAsFullKeyAsync()
    {
        var repositoryRoot = FindRepositoryRoot();
        var recipePath = Path.Combine(repositoryRoot, "samples", "recipes", "demo.recipe.json");
        var outputDirectory = CreateOutputDirectory("legacy-empty-prefix");
        var runner = new TestRunner();

        var result = await runner.RunAsync(
            new TestRunRequest("test run", recipePath, string.Empty, outputDirectory, string.Empty),
            CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected legacy empty-prefix recipe to pass.");
        AssertTrue(
            result.Steps.SelectMany(item => item.Measurements).All(item => item.FullKey == item.Key),
            "Expected legacy measurements to keep fullKey equal to key.");
        AssertTrue(
            result.Steps.SelectMany(item => item.SpecResults).Any(item => item.TargetKey == "voltage"),
            "Expected legacy spec resolution to keep targetKey equal to key when prefix is empty.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestRunnerReturnsFailedResultForAllSpecTypesFailSampleAsync()
    {
        var repositoryRoot = FindRepositoryRoot();
        var recipePath = Path.Combine(repositoryRoot, "samples", "recipes", "all-spec-types-fail.recipe.json");
        var specPath = Path.Combine(repositoryRoot, "samples", "specs", "all-spec-types.spec.json");
        var outputDirectory = CreateOutputDirectory("all-spec-types-fail");
        var runner = new TestRunner();

        var result = await runner.RunAsync(
            new TestRunRequest("test run", recipePath, specPath, outputDirectory, string.Empty),
            CancellationToken.None);

        AssertEqual("Failed", result.Status, "Expected all spec types fail sample to fail.");
        AssertTrue(
            result.Steps.SelectMany(item => item.SpecResults)
                .Any(item => string.Equals(item.PassFail, "Failed", StringComparison.Ordinal)),
            "Expected failed spec results.");
        AssertTrue(
            result.Steps.SelectMany(item => item.SpecResults)
                .Any(item => item.TargetKey == "battery.voltage" &&
                             item.Minimum == 11.5m &&
                             item.Maximum == 12.8m &&
                             item.ErrorCode == "BATTERY_VOLTAGE_RANGE" &&
                             string.Equals(item.PassFail, "Failed", StringComparison.Ordinal)),
            "Expected battery.voltage range failure.");
        AssertTrue(
            result.Steps.SelectMany(item => item.SpecResults)
                .Any(item => item.TargetKey == "dut.serial" &&
                             item.Pattern == "^ATS-FAKE-[0-9]{3}$" &&
                             item.ErrorCode == "SERIAL_FORMAT" &&
                             string.Equals(item.PassFail, "Failed", StringComparison.Ordinal)),
            "Expected dut.serial regex failure.");
        AssertTrue(
            result.Steps.SelectMany(item => item.SpecResults)
                .Any(item => item.TargetKey == "dut.calibration" &&
                             string.Equals(item.PassFail, "Passed", StringComparison.Ordinal)),
            "Expected bypass rule to remain passed.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestDeviceExecutorWritesArtifactsAsync()
    {
        var outputDirectory = CreateOutputDirectory("device-exec");
        var executor = new DeviceExecutor();

        var result = await executor.ExecuteAsync(
            new DeviceExecutionRequest("device exec", "PING", outputDirectory),
            CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected device exec to pass.");
        AssertEqual("PONG", result.Response, "Expected FakeDevice PING response.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestFlowExecutionRetriesTransientStepAndRecordsRetryCountAsync()
    {
        var outputDirectory = CreateOutputDirectory("flow-retry");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "flow-retry.recipe.json",
            """
            {
              "name": "Flow Retry Recipe",
              "scripts": [
                {
                  "name": "ReadVoltage",
                  "command": "READ_VOLTAGE",
                  "measurementKey": "voltage",
                  "unit": "V",
                  "retryCount": 1,
                  "simulatedResponse": "12.3"
                }
              ],
              "rules": [
                {
                  "name": "Voltage Range",
                  "targetKey": "voltage",
                  "ruleType": "Range",
                  "min": 11.5,
                  "max": 12.8
                }
              ]
            }
            """);

        var runner = CreateTestRunner(
            new DelegateDeviceFactory(() =>
                new CallbackDevice(
                    "TransientFakeDevice",
                    (request, attemptNumber, cancellationToken) =>
                    {
                        if (attemptNumber == 1)
                        {
                            throw new InvalidOperationException("Transient READ_VOLTAGE failure.");
                        }

                        return Task.FromResult(
                            new DeviceCommandResponse
                            {
                                Command = request.Command,
                                Response = request.SimulatedResponse,
                                Success = true,
                                Message = "Recovered on retry."
                            });
                    })));

        var result = await runner.RunAsync(
            new TestRunRequest("test run", recipePath, string.Empty, outputDirectory, string.Empty),
            CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected retry recipe to pass after transient failure.");
        AssertEqual("1", result.Steps.Count.ToString(), "Expected one step result.");
        AssertEqual("2", result.Steps[0].AttemptCount.ToString(), "Expected step to complete on second attempt.");
        AssertEqual("1", result.Steps[0].RetryCount.ToString(), "Expected retry count to be recorded.");

        var entries = ReadStructuredLogEntries(result.StructuredLogPath);
        AssertTrue(
            entries.Any(item =>
                item.EntryType == StructuredLogEntryType.StepRetried &&
                string.Equals(item.StepName, "ReadVoltage", StringComparison.Ordinal)),
            "Expected structured log to record the retry.");
        AssertTrue(
            entries.Any(item =>
                item.EntryType == StructuredLogEntryType.StepCompleted &&
                string.Equals(item.StepName, "ReadVoltage", StringComparison.Ordinal) &&
                item.Data.TryGetValue("attemptCount", out var attemptCount) &&
                string.Equals(attemptCount?.ToString(), "2", StringComparison.Ordinal)),
            "Expected structured log to record the final attempt count.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestFlowExecutionContinuesAfterStepErrorWhenContinueOnFailureIsEnabledAsync()
    {
        var outputDirectory = CreateOutputDirectory("flow-continue");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "flow-continue.recipe.json",
            """
            {
              "name": "Flow Continue Recipe",
              "scripts": [
                {
                  "name": "PowerOn",
                  "command": "POWER_ON",
                  "measurementKey": "startup.status",
                  "unit": "",
                  "continueOnFailure": true,
                  "simulatedResponse": "READY"
                },
                {
                  "name": "ReadVoltage",
                  "command": "READ_VOLTAGE",
                  "measurementKey": "battery.voltage",
                  "unit": "V",
                  "simulatedResponse": "12.3"
                }
              ],
              "rules": [
                {
                  "name": "Startup Ready",
                  "targetKey": "startup.status",
                  "ruleType": "Equal",
                  "expected": "READY"
                },
                {
                  "name": "Battery Voltage",
                  "targetKey": "battery.voltage",
                  "ruleType": "Range",
                  "min": 11.5,
                  "max": 12.8
                }
              ]
            }
            """);

        var runner = CreateTestRunner(
            new DelegateDeviceFactory(() =>
                new CallbackDevice(
                    "ContinueFakeDevice",
                    (request, _, _) =>
                    {
                        if (string.Equals(request.Command, "POWER_ON", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new InvalidOperationException("Power relay fault.");
                        }

                        return Task.FromResult(
                            new DeviceCommandResponse
                            {
                                Command = request.Command,
                                Response = request.SimulatedResponse,
                                Success = true,
                                Message = "Step completed."
                            });
                    })));

        var result = await runner.RunAsync(
            new TestRunRequest("test run", recipePath, string.Empty, outputDirectory, string.Empty),
            CancellationToken.None);

        AssertEqual("Error", result.Status, "Expected continued execution with an execution error to keep overall status Error.");
        AssertEqual("2", result.Steps.Count.ToString(), "Expected both steps to be present.");
        AssertEqual("Error", result.Steps[0].FinalStatus, "Expected first step to fail with execution error.");
        AssertEqual("Passed", result.Steps[1].FinalStatus, "Expected second step to execute after the first step error.");
        AssertContains(result.Errors, "PowerOn", "Expected error list to record the failing step.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestFlowExecutionRespectsStepTimeoutPolicyAsync()
    {
        var outputDirectory = CreateOutputDirectory("flow-timeout");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "flow-timeout.recipe.json",
            """
            {
              "name": "Flow Timeout Recipe",
              "scripts": [
                {
                  "name": "ReadVoltage",
                  "command": "READ_VOLTAGE",
                  "measurementKey": "battery.voltage",
                  "unit": "V",
                  "timeoutMs": 25,
                  "simulatedResponse": "12.3"
                }
              ],
              "rules": [
                {
                  "name": "Battery Voltage",
                  "targetKey": "battery.voltage",
                  "ruleType": "Range",
                  "min": 11.5,
                  "max": 12.8
                }
              ]
            }
            """);

        var runner = CreateTestRunner(
            new DelegateDeviceFactory(() =>
                new CallbackDevice(
                    "SlowFakeDevice",
                    async (request, _, cancellationToken) =>
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(150), cancellationToken);
                        return new DeviceCommandResponse
                        {
                            Command = request.Command,
                            Response = request.SimulatedResponse,
                            Success = true,
                            Message = "Completed after delay."
                        };
                    })));

        var result = await runner.RunAsync(
            new TestRunRequest("test run", recipePath, string.Empty, outputDirectory, string.Empty),
            CancellationToken.None);

        AssertEqual("Error", result.Status, "Expected timed out step to produce an Error result.");
        AssertEqual("1", result.Steps.Count.ToString(), "Expected one step result for timeout scenario.");
        AssertEqual("Error", result.Steps[0].FinalStatus, "Expected step final status to be Error.");
        AssertTrue(
            result.Steps[0].FailureMessage.Contains("exceeded timeout", StringComparison.OrdinalIgnoreCase),
            "Expected timeout failure message to be recorded on the step result.");

        var entries = ReadStructuredLogEntries(result.StructuredLogPath);
        AssertTrue(
            entries.Any(item =>
                item.EntryType == StructuredLogEntryType.StepTimedOut &&
                string.Equals(item.StepName, "ReadVoltage", StringComparison.Ordinal)),
            "Expected structured log to record the timeout event.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestDeviceExecutorUsesInjectedDeviceFactoryAsync()
    {
        var outputDirectory = CreateOutputDirectory("device-exec-injected");
        var executor = new DeviceExecutor(
            new SessionFactory(),
            new SessionArtifactWriter(),
            new DelegateDeviceFactory(() =>
                new CallbackDevice(
                    "InjectedDevice",
                    (request, _, _) =>
                        Task.FromResult(
                            new DeviceCommandResponse
                            {
                                Command = request.Command,
                                Response = $"CUSTOM:{request.Command}",
                                Success = true,
                                Message = "Injected device executed command."
                            }))));

        var result = await executor.ExecuteAsync(
            new DeviceExecutionRequest("device exec", "PING", outputDirectory),
            CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected injected device execution to pass.");
        AssertEqual("InjectedDevice", result.DeviceName, "Expected injected device factory to control the device instance.");
        AssertEqual("CUSTOM:PING", result.Response, "Expected injected device response to be returned.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestNestedSequenceExecutesInDeclaredOrderAsync()
    {
        var outputDirectory = CreateOutputDirectory("flow-sequence-order");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "flow-sequence-order.recipe.json",
            """
            {
              "name": "Nested Sequence Recipe",
              "scripts": [
                { "name": "ReadA", "command": "READ_A", "measurementKey": "stepA", "unit": "", "simulatedResponse": "A" },
                { "name": "ReadB", "command": "READ_B", "measurementKey": "stepB", "unit": "", "simulatedResponse": "B" },
                { "name": "ReadC", "command": "READ_C", "measurementKey": "stepC", "unit": "", "simulatedResponse": "C" },
                { "name": "ReadD", "command": "READ_D", "measurementKey": "stepD", "unit": "", "simulatedResponse": "D" }
              ],
              "flow": {
                "name": "MainSequence",
                "nodes": [
                  { "type": "step", "step": "ReadA" },
                  {
                    "type": "sequence",
                    "name": "NestedSequence",
                    "nodes": [
                      { "type": "step", "step": "ReadB" },
                      {
                        "type": "sequence",
                        "name": "InnerSequence",
                        "nodes": [
                          { "type": "step", "step": "ReadC" }
                        ]
                      }
                    ]
                  },
                  { "type": "step", "step": "ReadD" }
                ]
              },
              "rules": [
                { "name": "Step A", "targetKey": "stepA", "ruleType": "Equal", "expected": "A" },
                { "name": "Step B", "targetKey": "stepB", "ruleType": "Equal", "expected": "B" },
                { "name": "Step C", "targetKey": "stepC", "ruleType": "Equal", "expected": "C" },
                { "name": "Step D", "targetKey": "stepD", "ruleType": "Equal", "expected": "D" }
              ]
            }
            """);

        var runner = new TestRunner();
        var result = await runner.RunAsync(
            new TestRunRequest("test simulate", recipePath, string.Empty, outputDirectory, string.Empty),
            CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected nested sequence recipe to pass.");
        AssertEqual("ReadA,ReadB,ReadC,ReadD", string.Join(",", result.Steps.Select(item => item.StepName)), "Expected nested sequence execution order to stay deterministic.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestSequenceContainerPreservesContinueOnFailureBehaviorAsync()
    {
        var outputDirectory = CreateOutputDirectory("flow-sequence-continue");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "flow-sequence-continue.recipe.json",
            """
            {
              "name": "Sequence Continue Recipe",
              "scripts": [
                { "name": "StartStep", "command": "START_STEP", "measurementKey": "startValue", "unit": "", "simulatedResponse": "READY" },
                { "name": "FailStep", "command": "FAIL_STEP", "measurementKey": "failValue", "unit": "", "continueOnFailure": true, "simulatedResponse": "FAIL" },
                { "name": "AfterFailure", "command": "AFTER_FAILURE", "measurementKey": "afterValue", "unit": "", "simulatedResponse": "RECOVERED" },
                { "name": "FinalStep", "command": "FINAL_STEP", "measurementKey": "finalValue", "unit": "", "simulatedResponse": "DONE" }
              ],
              "flow": {
                "name": "MainSequence",
                "nodes": [
                  {
                    "type": "sequence",
                    "name": "NestedSequence",
                    "nodes": [
                      { "type": "step", "step": "StartStep" },
                      { "type": "step", "step": "FailStep" },
                      { "type": "step", "step": "AfterFailure" }
                    ]
                  },
                  { "type": "step", "step": "FinalStep" }
                ]
              },
              "rules": [
                { "name": "Start Value", "targetKey": "startValue", "ruleType": "Equal", "expected": "READY" },
                { "name": "Fail Value", "targetKey": "failValue", "ruleType": "Equal", "expected": "FAIL" },
                { "name": "After Value", "targetKey": "afterValue", "ruleType": "Equal", "expected": "RECOVERED" },
                { "name": "Final Value", "targetKey": "finalValue", "ruleType": "Equal", "expected": "DONE" }
              ]
            }
            """);

        var runner = CreateTestRunner(
            new DelegateDeviceFactory(() =>
                new CallbackDevice(
                    "SequenceContinueDevice",
                    (request, _, _) =>
                    {
                        if (string.Equals(request.Command, "FAIL_STEP", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new InvalidOperationException("Intentional nested sequence failure.");
                        }

                        return Task.FromResult(
                            new DeviceCommandResponse
                            {
                                Command = request.Command,
                                Response = request.SimulatedResponse,
                                Success = true,
                                Message = "Sequence step completed."
                            });
                    })));

        var result = await runner.RunAsync(
            new TestRunRequest("test run", recipePath, string.Empty, outputDirectory, string.Empty),
            CancellationToken.None);

        AssertEqual("Error", result.Status, "Expected nested sequence execution error to keep overall status Error.");
        AssertEqual("StartStep,FailStep,AfterFailure,FinalStep", string.Join(",", result.Steps.Select(item => item.StepName)), "Expected sequence children and following siblings to continue after continueOnFailure.");
        AssertEqual("Error", result.Steps[1].FinalStatus, "Expected failing nested step to record an Error status.");
        AssertEqual("Passed", result.Steps[2].FinalStatus, "Expected nested sequence sibling after the failing step to still run.");
        AssertEqual("Passed", result.Steps[3].FinalStatus, "Expected outer sequence sibling to still run.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestConditionNodeExecutesTrueBranchAsync()
    {
        var outputDirectory = CreateOutputDirectory("flow-condition-true");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "flow-condition-true.recipe.json",
            """
            {
              "name": "Condition True Recipe",
              "scripts": [
                { "name": "ReadGate", "command": "READ_GATE", "measurementKey": "gateValue", "unit": "", "simulatedResponse": "ENABLED" },
                { "name": "TruePath", "command": "TRUE_PATH", "measurementKey": "trueValue", "unit": "", "simulatedResponse": "TRUE" },
                { "name": "FalsePath", "command": "FALSE_PATH", "measurementKey": "falseValue", "unit": "", "simulatedResponse": "FALSE" }
              ],
              "flow": {
                "name": "MainFlow",
                "nodes": [
                  { "type": "step", "step": "ReadGate" },
                  {
                    "type": "condition",
                    "name": "GateEnabled",
                    "condition": { "type": "dataEquals", "key": "gateValue", "value": "ENABLED" },
                    "whenTrue": [
                      { "type": "step", "step": "TruePath" }
                    ],
                    "whenFalse": [
                      { "type": "step", "step": "FalsePath" }
                    ]
                  }
                ]
              },
              "rules": [
                { "name": "Gate Value", "targetKey": "gateValue", "ruleType": "Equal", "expected": "ENABLED" },
                { "name": "True Value", "targetKey": "trueValue", "ruleType": "Equal", "expected": "TRUE" },
                { "name": "False Value", "targetKey": "falseValue", "ruleType": "Equal", "expected": "FALSE" }
              ]
            }
            """);

        var runner = new TestRunner();
        var result = await runner.RunAsync(
            new TestRunRequest("test simulate", recipePath, string.Empty, outputDirectory, string.Empty),
            CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected true branch recipe to pass.");
        AssertEqual("ReadGate,TruePath", string.Join(",", result.Steps.Select(item => item.StepName)), "Expected condition true branch to execute the true path only.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestConditionNodeExecutesFalseBranchFromPreviousStepStatusAsync()
    {
        var outputDirectory = CreateOutputDirectory("flow-condition-false");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "flow-condition-false.recipe.json",
            """
            {
              "name": "Condition False Recipe",
              "scripts": [
                { "name": "ReadVoltage", "command": "READ_VOLTAGE", "measurementKey": "voltage", "unit": "V", "simulatedResponse": "10.0" },
                { "name": "UnexpectedPath", "command": "UNEXPECTED_PATH", "measurementKey": "unexpectedValue", "unit": "", "simulatedResponse": "UNEXPECTED" },
                { "name": "FallbackPath", "command": "FALLBACK_PATH", "measurementKey": "fallbackValue", "unit": "", "simulatedResponse": "FALLBACK" }
              ],
              "flow": {
                "name": "MainFlow",
                "nodes": [
                  { "type": "step", "step": "ReadVoltage" },
                  {
                    "type": "condition",
                    "name": "PreviousStepPassed",
                    "condition": { "type": "previousStepStatus", "status": "Passed" },
                    "whenTrue": [
                      { "type": "step", "step": "UnexpectedPath" }
                    ],
                    "whenFalse": [
                      { "type": "step", "step": "FallbackPath" }
                    ]
                  }
                ]
              },
              "rules": [
                { "name": "Voltage Range", "targetKey": "voltage", "ruleType": "Range", "min": 11.5, "max": 12.8 },
                { "name": "Unexpected Value", "targetKey": "unexpectedValue", "ruleType": "Equal", "expected": "UNEXPECTED" },
                { "name": "Fallback Value", "targetKey": "fallbackValue", "ruleType": "Equal", "expected": "FALLBACK" }
              ]
            }
            """);

        var runner = new TestRunner();
        var result = await runner.RunAsync(
            new TestRunRequest("test run", recipePath, string.Empty, outputDirectory, string.Empty),
            CancellationToken.None);

        AssertEqual("Failed", result.Status, "Expected original spec failure to keep overall status Failed.");
        AssertEqual("ReadVoltage,FallbackPath", string.Join(",", result.Steps.Select(item => item.StepName)), "Expected false branch to execute from previous step status.");
        AssertEqual("Failed", result.Steps[0].FinalStatus, "Expected first step to fail its spec range.");
        AssertEqual("Passed", result.Steps[1].FinalStatus, "Expected false branch step to pass.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestRetryBehaviorWorksInsideConditionSequenceBranchAsync()
    {
        var outputDirectory = CreateOutputDirectory("flow-branch-retry");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "flow-branch-retry.recipe.json",
            """
            {
              "name": "Branch Retry Recipe",
              "scripts": [
                { "name": "ReadGate", "command": "READ_GATE", "measurementKey": "gateValue", "unit": "", "simulatedResponse": "YES" },
                { "name": "RetryStep", "command": "RETRY_STEP", "measurementKey": "retryValue", "unit": "", "retryCount": 1, "simulatedResponse": "RECOVERED" }
              ],
              "flow": {
                "name": "MainFlow",
                "nodes": [
                  { "type": "step", "step": "ReadGate" },
                  {
                    "type": "condition",
                    "name": "RetryBranch",
                    "condition": { "type": "dataExists", "key": "gateValue" },
                    "whenTrue": [
                      {
                        "type": "sequence",
                        "name": "RetrySequence",
                        "nodes": [
                          { "type": "step", "step": "RetryStep" }
                        ]
                      }
                    ],
                    "whenFalse": []
                  }
                ]
              },
              "rules": [
                { "name": "Gate Value", "targetKey": "gateValue", "ruleType": "Equal", "expected": "YES" },
                { "name": "Retry Value", "targetKey": "retryValue", "ruleType": "Equal", "expected": "RECOVERED" }
              ]
            }
            """);

        var runner = CreateTestRunner(
            new DelegateDeviceFactory(() =>
                new CallbackDevice(
                    "BranchRetryDevice",
                    (request, attemptNumber, _) =>
                    {
                        if (string.Equals(request.Command, "RETRY_STEP", StringComparison.OrdinalIgnoreCase) &&
                            attemptNumber == 1)
                        {
                            throw new InvalidOperationException("Branch retry step failed on first attempt.");
                        }

                        return Task.FromResult(
                            new DeviceCommandResponse
                            {
                                Command = request.Command,
                                Response = request.SimulatedResponse,
                                Success = true,
                                Message = "Branch retry command completed."
                            });
                    })));

        var result = await runner.RunAsync(
            new TestRunRequest("test run", recipePath, string.Empty, outputDirectory, string.Empty),
            CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected retry inside condition sequence branch to recover and pass.");
        AssertEqual("2", result.Steps.Count.ToString(), "Expected gate step and retry step to execute.");
        AssertEqual("2", result.Steps[1].AttemptCount.ToString(), "Expected retry step inside branch to finish on the second attempt.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestStructuredEventsCaptureContainerAndBranchLifecycleAsync()
    {
        var outputDirectory = CreateOutputDirectory("flow-structured-events");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "flow-structured-events.recipe.json",
            """
            {
              "name": "Flow Structured Events Recipe",
              "scripts": [
                { "name": "ReadGate", "command": "READ_GATE", "measurementKey": "gateValue", "unit": "", "simulatedResponse": "OPEN" },
                { "name": "NestedStep", "command": "NESTED_STEP", "measurementKey": "nestedValue", "unit": "", "simulatedResponse": "NESTED" }
              ],
              "flow": {
                "name": "MainFlow",
                "nodes": [
                  {
                    "type": "sequence",
                    "name": "OuterSequence",
                    "nodes": [
                      { "type": "step", "step": "ReadGate" },
                      {
                        "type": "condition",
                        "name": "GateBranch",
                        "condition": { "type": "dataEquals", "key": "gateValue", "value": "OPEN" },
                        "whenTrue": [
                          {
                            "type": "sequence",
                            "name": "NestedSequence",
                            "nodes": [
                              { "type": "step", "step": "NestedStep" }
                            ]
                          }
                        ],
                        "whenFalse": []
                      }
                    ]
                  }
                ]
              },
              "rules": [
                { "name": "Gate Value", "targetKey": "gateValue", "ruleType": "Equal", "expected": "OPEN" },
                { "name": "Nested Value", "targetKey": "nestedValue", "ruleType": "Equal", "expected": "NESTED" }
              ]
            }
            """);

        var runner = new TestRunner();
        var result = await runner.RunAsync(
            new TestRunRequest("test simulate", recipePath, string.Empty, outputDirectory, string.Empty),
            CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected structured-event flow recipe to pass.");

        var entries = ReadStructuredLogEntries(result.StructuredLogPath);
        AssertTrue(
            entries.Any(item =>
                item.EntryType == StructuredLogEntryType.ContainerStarted &&
                string.Equals(item.ItemName, "MainFlow", StringComparison.Ordinal)),
            "Expected root container start event.");
        AssertTrue(
            entries.Any(item =>
                item.EntryType == StructuredLogEntryType.ContainerCompleted &&
                string.Equals(item.ItemName, "NestedSequence", StringComparison.Ordinal)),
            "Expected nested container completed event.");
        AssertTrue(
            entries.Any(item =>
                item.EntryType == StructuredLogEntryType.BranchEvaluated &&
                string.Equals(item.ItemName, "GateBranch", StringComparison.Ordinal)),
            "Expected branch evaluated event.");
        AssertTrue(
            entries.Any(item =>
                item.EntryType == StructuredLogEntryType.BranchSelected &&
                string.Equals(item.ItemName, "GateBranch", StringComparison.Ordinal) &&
                item.Data.TryGetValue("selectedBranch", out var selectedBranch) &&
                string.Equals(selectedBranch?.ToString(), "True", StringComparison.Ordinal)),
            "Expected branch selected event with true branch.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestRepeatUntilStopsWhenPreviousStepStatusBecomesPassedAsync()
    {
        var outputDirectory = CreateOutputDirectory("flow-repeat-status");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "flow-repeat-status.recipe.json",
            """
            {
              "name": "Repeat Until Status Recipe",
              "scripts": [
                { "name": "PollReady", "command": "POLL_READY", "measurementKey": "readyStatus", "unit": "", "simulatedResponse": "READY" }
              ],
              "flow": {
                "name": "MainFlow",
                "nodes": [
                  {
                    "type": "repeatUntil",
                    "name": "WaitUntilPassed",
                    "maxIterations": 3,
                    "failOnMaxIterations": true,
                    "until": { "type": "previousStepStatus", "status": "Passed" },
                    "nodes": [
                      { "type": "step", "step": "PollReady" }
                    ]
                  }
                ]
              },
              "rules": [
                { "name": "Ready Status", "targetKey": "readyStatus", "ruleType": "Equal", "expected": "READY" }
              ]
            }
            """);

        var runner = CreateTestRunner(
            new DelegateDeviceFactory(() =>
                new CallbackDevice(
                    "RepeatStatusDevice",
                    (request, attemptNumber, _) =>
                        Task.FromResult(
                            new DeviceCommandResponse
                            {
                                Command = request.Command,
                                Response = attemptNumber == 1 ? "NOT_READY" : "READY",
                                Success = true,
                                Message = "Poll complete."
                            }))));

        var result = await runner.RunAsync(
            new TestRunRequest("test run", recipePath, string.Empty, outputDirectory, string.Empty),
            CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected repeatUntil to pass once previous step status becomes Passed.");
        AssertEqual("2", result.Steps.Count.ToString(), "Expected two loop iterations.");
        AssertTrue(!result.Steps[0].CountsTowardFinalStatus, "Expected the first non-terminal iteration to be marked transient.");
        AssertTrue(result.Steps[1].CountsTowardFinalStatus, "Expected the final iteration to count toward final status.");
        AssertEqual("Failed", result.Steps[0].FinalStatus, "Expected first iteration to fail the spec.");
        AssertEqual("Passed", result.Steps[1].FinalStatus, "Expected second iteration to pass the spec.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestRepeatUntilStopsWhenDataExistsAsync()
    {
        var outputDirectory = CreateOutputDirectory("flow-repeat-data-exists");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "flow-repeat-data-exists.recipe.json",
            """
            {
              "name": "Repeat Until Data Exists Recipe",
              "scripts": [
                { "name": "CaptureValue", "command": "CAPTURE_VALUE", "measurementKey": "capturedValue", "unit": "", "simulatedResponse": "FOUND" }
              ],
              "flow": {
                "name": "MainFlow",
                "nodes": [
                  {
                    "type": "repeatUntil",
                    "name": "WaitForData",
                    "maxIterations": 2,
                    "failOnMaxIterations": true,
                    "until": { "type": "dataExists", "key": "capturedValue" },
                    "nodes": [
                      { "type": "step", "step": "CaptureValue" }
                    ]
                  }
                ]
              },
              "rules": [
                { "name": "Captured Value", "targetKey": "capturedValue", "ruleType": "Equal", "expected": "FOUND" }
              ]
            }
            """);

        var runner = new TestRunner();
        var result = await runner.RunAsync(
            new TestRunRequest("test simulate", recipePath, string.Empty, outputDirectory, string.Empty),
            CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected repeatUntil to stop as soon as the data key exists.");
        AssertEqual("1", result.Steps.Count.ToString(), "Expected only one iteration when dataExists is already satisfied after the first pass.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestRepeatUntilReportsDeterministicFailureAtMaxIterationsAsync()
    {
        var outputDirectory = CreateOutputDirectory("flow-repeat-max");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "flow-repeat-max.recipe.json",
            """
            {
              "name": "Repeat Until Max Recipe",
              "scripts": [
                { "name": "PollReady", "command": "POLL_READY", "measurementKey": "readyStatus", "unit": "", "simulatedResponse": "READY" }
              ],
              "flow": {
                "name": "MainFlow",
                "nodes": [
                  {
                    "type": "repeatUntil",
                    "name": "WaitUntilReady",
                    "maxIterations": 2,
                    "failOnMaxIterations": true,
                    "until": { "type": "dataEquals", "key": "readyStatus", "value": "READY" },
                    "nodes": [
                      { "type": "step", "step": "PollReady" }
                    ]
                  }
                ]
              },
              "rules": [
                { "name": "Ready Status", "targetKey": "readyStatus", "ruleType": "Equal", "expected": "READY" }
              ]
            }
            """);

        var runner = CreateTestRunner(
            new DelegateDeviceFactory(() =>
                new CallbackDevice(
                    "RepeatMaxDevice",
                    (request, _, _) =>
                        Task.FromResult(
                            new DeviceCommandResponse
                            {
                                Command = request.Command,
                                Response = "NOT_READY",
                                Success = true,
                                Message = "Poll incomplete."
                            }))));

        var result = await runner.RunAsync(
            new TestRunRequest("test run", recipePath, string.Empty, outputDirectory, string.Empty),
            CancellationToken.None);

        AssertEqual("Error", result.Status, "Expected repeatUntil max-iterations breach to produce an Error result.");
        AssertContains(result.Errors, "maxIterations=2", "Expected deterministic max-iterations failure message.");

        var entries = ReadStructuredLogEntries(result.StructuredLogPath);
        AssertTrue(
            entries.Any(item =>
                item.EntryType == StructuredLogEntryType.LoopMaxIterationsReached &&
                string.Equals(item.ItemName, "WaitUntilReady", StringComparison.Ordinal)),
            "Expected structured log to record loop max-iterations event.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestRepeatUntilWorksInsideSequenceAsync()
    {
        var outputDirectory = CreateOutputDirectory("flow-repeat-sequence");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "flow-repeat-sequence.recipe.json",
            """
            {
              "name": "Repeat In Sequence Recipe",
              "scripts": [
                { "name": "StartStep", "command": "START_STEP", "measurementKey": "startValue", "unit": "", "simulatedResponse": "START" },
                { "name": "PollReady", "command": "POLL_READY", "measurementKey": "readyStatus", "unit": "", "simulatedResponse": "READY" },
                { "name": "FinishStep", "command": "FINISH_STEP", "measurementKey": "finishValue", "unit": "", "simulatedResponse": "DONE" }
              ],
              "flow": {
                "name": "MainFlow",
                "nodes": [
                  { "type": "step", "step": "StartStep" },
                  {
                    "type": "repeatUntil",
                    "name": "WaitUntilReady",
                    "maxIterations": 3,
                    "failOnMaxIterations": true,
                    "until": { "type": "dataEquals", "key": "readyStatus", "value": "READY" },
                    "nodes": [
                      { "type": "step", "step": "PollReady" }
                    ]
                  },
                  { "type": "step", "step": "FinishStep" }
                ]
              },
              "rules": [
                { "name": "Start Value", "targetKey": "startValue", "ruleType": "Equal", "expected": "START" },
                { "name": "Ready Status", "targetKey": "readyStatus", "ruleType": "Equal", "expected": "READY" },
                { "name": "Finish Value", "targetKey": "finishValue", "ruleType": "Equal", "expected": "DONE" }
              ]
            }
            """);

        var runner = CreateTestRunner(
            new DelegateDeviceFactory(() =>
                new CallbackDevice(
                    "RepeatSequenceDevice",
                    (request, attemptNumber, _) =>
                    {
                        var response = request.Command switch
                        {
                            "POLL_READY" when attemptNumber == 1 => "WAIT",
                            "POLL_READY" => "READY",
                            _ => request.SimulatedResponse
                        };

                        return Task.FromResult(
                            new DeviceCommandResponse
                            {
                                Command = request.Command,
                                Response = response,
                                Success = true,
                                Message = "Sequence loop command completed."
                            });
                    })));

        var result = await runner.RunAsync(
            new TestRunRequest("test run", recipePath, string.Empty, outputDirectory, string.Empty),
            CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected repeatUntil inside a sequence to let following steps run after the stop condition is met.");
        AssertEqual("StartStep,PollReady,PollReady,FinishStep", string.Join(",", result.Steps.Select(item => item.StepName)), "Expected sequence order to include repeated iterations and the following step.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestRepeatUntilWritesStructuredLoopEventsAsync()
    {
        var outputDirectory = CreateOutputDirectory("flow-repeat-events");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "flow-repeat-events.recipe.json",
            """
            {
              "name": "Repeat Loop Events Recipe",
              "scripts": [
                { "name": "CaptureValue", "command": "CAPTURE_VALUE", "measurementKey": "capturedValue", "unit": "", "simulatedResponse": "FOUND" }
              ],
              "flow": {
                "name": "MainFlow",
                "nodes": [
                  {
                    "type": "repeatUntil",
                    "name": "WaitForValue",
                    "maxIterations": 2,
                    "failOnMaxIterations": true,
                    "until": { "type": "dataEquals", "key": "capturedValue", "value": "FOUND" },
                    "nodes": [
                      { "type": "step", "step": "CaptureValue" }
                    ]
                  }
                ]
              },
              "rules": [
                { "name": "Captured Value", "targetKey": "capturedValue", "ruleType": "Equal", "expected": "FOUND" }
              ]
            }
            """);

        var runner = new TestRunner();
        var result = await runner.RunAsync(
            new TestRunRequest("test simulate", recipePath, string.Empty, outputDirectory, string.Empty),
            CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected loop-events recipe to pass.");

        var entries = ReadStructuredLogEntries(result.StructuredLogPath);
        AssertTrue(
            entries.Any(item => item.EntryType == StructuredLogEntryType.LoopStarted && string.Equals(item.ItemName, "WaitForValue", StringComparison.Ordinal)),
            "Expected loop started event.");
        AssertTrue(
            entries.Any(item => item.EntryType == StructuredLogEntryType.LoopIterationStarted && string.Equals(item.ItemName, "WaitForValue", StringComparison.Ordinal)),
            "Expected loop iteration started event.");
        AssertTrue(
            entries.Any(item => item.EntryType == StructuredLogEntryType.LoopIterationCompleted && string.Equals(item.ItemName, "WaitForValue", StringComparison.Ordinal)),
            "Expected loop iteration completed event.");
        AssertTrue(
            entries.Any(item => item.EntryType == StructuredLogEntryType.LoopConditionEvaluated && string.Equals(item.ItemName, "WaitForValue", StringComparison.Ordinal)),
            "Expected loop condition evaluated event.");
        AssertTrue(
            entries.Any(item => item.EntryType == StructuredLogEntryType.LoopCompleted && string.Equals(item.ItemName, "WaitForValue", StringComparison.Ordinal)),
            "Expected loop completed event.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestRepeatUntilBreaksOnFailedIterationWhenPolicyConfiguredAsync()
    {
        var outputDirectory = CreateOutputDirectory("flow-repeat-policy-failure");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "flow-repeat-policy-failure.recipe.json",
            """
            {
              "name": "Repeat Until Policy Failure Recipe",
              "scripts": [
                { "name": "PollReady", "command": "POLL_READY", "measurementKey": "readyStatus", "unit": "", "simulatedResponse": "READY" }
              ],
              "flow": {
                "name": "MainFlow",
                "nodes": [
                  {
                    "type": "repeatUntil",
                    "name": "WaitUntilReady",
                    "outcomePolicy": "breakOnStepFailure",
                    "maxIterations": 3,
                    "failOnMaxIterations": true,
                    "until": { "type": "dataEquals", "key": "readyStatus", "value": "READY" },
                    "nodes": [
                      { "type": "step", "step": "PollReady" }
                    ]
                  }
                ]
              },
              "rules": [
                { "name": "Ready Status", "targetKey": "readyStatus", "ruleType": "Equal", "expected": "READY" }
              ]
            }
            """);

        var runner = CreateTestRunner(
            new DelegateDeviceFactory(() =>
                new CallbackDevice(
                    "RepeatPolicyFailureDevice",
                    (request, _, _) =>
                        Task.FromResult(
                            new DeviceCommandResponse
                            {
                                Command = request.Command,
                                Response = "WAIT",
                                Success = true,
                                Message = "Policy failure poll complete."
                            }))));

        var result = await runner.RunAsync(
            new TestRunRequest("test run", recipePath, string.Empty, outputDirectory, string.Empty),
            CancellationToken.None);

        AssertEqual("Failed", result.Status, "Expected breakOnStepFailure to stop the loop with Failed status.");
        AssertEqual("1", result.Steps.Count.ToString(), "Expected failure policy to stop after the first failed iteration.");
        AssertTrue(result.Errors.Count == 0, "Expected failure-policy termination to avoid max-iterations error messages.");
        AssertEqual("StepFailureBreak", result.FlowResultTree!.Children[0].StopReason, "Expected flow tree stop reason for failure policy.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestRepeatUntilBreaksOnPassedIterationWhenPolicyConfiguredAsync()
    {
        var outputDirectory = CreateOutputDirectory("flow-repeat-policy-success");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "flow-repeat-policy-success.recipe.json",
            """
            {
              "name": "Repeat Until Policy Success Recipe",
              "scripts": [
                { "name": "PollReady", "command": "POLL_READY", "measurementKey": "readyStatus", "unit": "", "simulatedResponse": "READY" }
              ],
              "flow": {
                "name": "MainFlow",
                "nodes": [
                  {
                    "type": "repeatUntil",
                    "name": "WaitUntilReady",
                    "outcomePolicy": "breakOnStepSuccess",
                    "maxIterations": 3,
                    "failOnMaxIterations": true,
                    "until": { "type": "dataEquals", "key": "readyStatus", "value": "DONE" },
                    "nodes": [
                      { "type": "step", "step": "PollReady" }
                    ]
                  }
                ]
              },
              "rules": [
                { "name": "Ready Status", "targetKey": "readyStatus", "ruleType": "Equal", "expected": "READY" }
              ]
            }
            """);

        var runner = new TestRunner();
        var result = await runner.RunAsync(
            new TestRunRequest("test simulate", recipePath, string.Empty, outputDirectory, string.Empty),
            CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected breakOnStepSuccess to stop the loop with Passed status.");
        AssertEqual("1", result.Steps.Count.ToString(), "Expected success policy to stop after the first passed iteration.");
        AssertEqual("StepSuccessBreak", result.FlowResultTree!.Children[0].StopReason, "Expected flow tree stop reason for success policy.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestRepeatUntilDefaultBehaviorRemainsBackwardCompatibleWithoutOutcomePolicyAsync()
    {
        var outputDirectory = CreateOutputDirectory("flow-repeat-policy-default");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "flow-repeat-policy-default.recipe.json",
            """
            {
              "name": "Repeat Until Policy Default Recipe",
              "scripts": [
                { "name": "PollReady", "command": "POLL_READY", "measurementKey": "readyStatus", "unit": "", "simulatedResponse": "READY" }
              ],
              "flow": {
                "name": "MainFlow",
                "nodes": [
                  {
                    "type": "repeatUntil",
                    "name": "WaitUntilDone",
                    "maxIterations": 2,
                    "failOnMaxIterations": true,
                    "until": { "type": "dataEquals", "key": "readyStatus", "value": "DONE" },
                    "nodes": [
                      { "type": "step", "step": "PollReady" }
                    ]
                  }
                ]
              },
              "rules": [
                { "name": "Ready Status", "targetKey": "readyStatus", "ruleType": "Equal", "expected": "READY" }
              ]
            }
            """);

        var runner = new TestRunner();
        var result = await runner.RunAsync(
            new TestRunRequest("test simulate", recipePath, string.Empty, outputDirectory, string.Empty),
            CancellationToken.None);

        AssertEqual("Error", result.Status, "Expected repeatUntil without outcomePolicy to keep existing condition/maxIterations behavior.");
        AssertEqual("2", result.Steps.Count.ToString(), "Expected backward-compatible loop to continue until maxIterations.");
        AssertEqual("MaxIterationsReached", result.FlowResultTree!.Children[0].StopReason, "Expected max-iterations stop reason when no outcomePolicy is specified.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestRepeatUntilPolicyTerminationTakesPrecedenceOverMaxIterationsAsync()
    {
        var outputDirectory = CreateOutputDirectory("flow-repeat-policy-precedence");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "flow-repeat-policy-precedence.recipe.json",
            """
            {
              "name": "Repeat Until Policy Precedence Recipe",
              "scripts": [
                { "name": "PollReady", "command": "POLL_READY", "measurementKey": "readyStatus", "unit": "", "simulatedResponse": "READY" }
              ],
              "flow": {
                "name": "MainFlow",
                "nodes": [
                  {
                    "type": "repeatUntil",
                    "name": "WaitUntilReady",
                    "outcomePolicy": "breakOnStepSuccess",
                    "maxIterations": 1,
                    "failOnMaxIterations": true,
                    "until": { "type": "dataEquals", "key": "readyStatus", "value": "DONE" },
                    "nodes": [
                      { "type": "step", "step": "PollReady" }
                    ]
                  }
                ]
              },
              "rules": [
                { "name": "Ready Status", "targetKey": "readyStatus", "ruleType": "Equal", "expected": "READY" }
              ]
            }
            """);

        var runner = new TestRunner();
        var result = await runner.RunAsync(
            new TestRunRequest("test simulate", recipePath, string.Empty, outputDirectory, string.Empty),
            CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected outcomePolicy to terminate before max-iterations failure is applied.");
        AssertTrue(result.Errors.Count == 0, "Expected policy termination to suppress max-iterations errors.");
        AssertEqual("StepSuccessBreak", result.FlowResultTree!.Children[0].StopReason, "Expected policy stop reason to take precedence over max iterations.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestSequenceBreaksOnFailedChildWhenPolicyConfiguredAsync()
    {
        var outputDirectory = CreateOutputDirectory("flow-sequence-policy-failure");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "flow-sequence-policy-failure.recipe.json",
            """
            {
              "name": "Sequence Policy Failure Recipe",
              "scripts": [
                { "name": "PollReady", "command": "POLL_READY", "measurementKey": "readyStatus", "unit": "", "simulatedResponse": "READY" },
                { "name": "AfterReady", "command": "AFTER_READY", "measurementKey": "afterStatus", "unit": "", "simulatedResponse": "DONE" }
              ],
              "flow": {
                "name": "MainFlow",
                "outcomePolicy": "breakOnStepFailure",
                "nodes": [
                  { "type": "step", "step": "PollReady" },
                  { "type": "step", "step": "AfterReady" }
                ]
              },
              "rules": [
                { "name": "Ready Status", "targetKey": "readyStatus", "ruleType": "Equal", "expected": "DONE" },
                { "name": "After Status", "targetKey": "afterStatus", "ruleType": "Equal", "expected": "DONE" }
              ]
            }
            """);

        var runner = new TestRunner();
        var result = await runner.RunAsync(
            new TestRunRequest("test simulate", recipePath, string.Empty, outputDirectory, string.Empty),
            CancellationToken.None);

        AssertEqual("Failed", result.Status, "Expected breakOnStepFailure to stop the sequence after the first failed child.");
        AssertEqual("1", result.Steps.Count.ToString(), "Expected failure policy to stop remaining sequence children.");
        AssertEqual("PollReady", result.Steps[0].StepName, "Expected first child step to be recorded.");
        AssertEqual("StepFailureBreak", result.FlowResultTree!.StopReason, "Expected root sequence stop reason.");
        AssertEqual("breakOnStepFailure", result.FlowResultTree.OutcomePolicy, "Expected canonical outcome policy to be recorded.");
        AssertEqual("PollReady", result.FlowResultTree.TriggeredByNodeName, "Expected triggering child node name.");
        AssertEqual("1", result.FlowResultTree.Children.Count.ToString(), "Expected result tree to contain only executed children.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestSequenceBreaksOnPassedChildWhenPolicyConfiguredAsync()
    {
        var outputDirectory = CreateOutputDirectory("flow-sequence-policy-success");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "flow-sequence-policy-success.recipe.json",
            """
            {
              "name": "Sequence Policy Success Recipe",
              "scripts": [
                { "name": "PollReady", "command": "POLL_READY", "measurementKey": "readyStatus", "unit": "", "simulatedResponse": "READY" },
                { "name": "AfterReady", "command": "AFTER_READY", "measurementKey": "afterStatus", "unit": "", "simulatedResponse": "DONE" }
              ],
              "flow": {
                "name": "MainFlow",
                "outcomePolicy": "breakOnStepSuccess",
                "nodes": [
                  { "type": "step", "step": "PollReady" },
                  { "type": "step", "step": "AfterReady" }
                ]
              },
              "rules": [
                { "name": "Ready Status", "targetKey": "readyStatus", "ruleType": "Equal", "expected": "READY" },
                { "name": "After Status", "targetKey": "afterStatus", "ruleType": "Equal", "expected": "DONE" }
              ]
            }
            """);

        var runner = new TestRunner();
        var result = await runner.RunAsync(
            new TestRunRequest("test simulate", recipePath, string.Empty, outputDirectory, string.Empty),
            CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected breakOnStepSuccess to stop the sequence after the first passed child.");
        AssertEqual("1", result.Steps.Count.ToString(), "Expected success policy to stop remaining sequence children.");
        AssertEqual("StepSuccessBreak", result.FlowResultTree!.StopReason, "Expected root sequence stop reason.");
        AssertEqual("breakOnStepSuccess", result.FlowResultTree.OutcomePolicy, "Expected canonical outcome policy to be recorded.");
        AssertEqual("PollReady", result.FlowResultTree.TriggeredByNodeName, "Expected triggering child node name.");
        AssertEqual("1", result.FlowResultTree.Children.Count.ToString(), "Expected result tree to contain only executed children.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestSequenceDefaultBehaviorRemainsBackwardCompatibleWithoutOutcomePolicyAsync()
    {
        var outputDirectory = CreateOutputDirectory("flow-sequence-policy-default");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "flow-sequence-policy-default.recipe.json",
            """
            {
              "name": "Sequence Policy Default Recipe",
              "scripts": [
                { "name": "PollReady", "command": "POLL_READY", "measurementKey": "readyStatus", "unit": "", "simulatedResponse": "READY" },
                { "name": "AfterReady", "command": "AFTER_READY", "measurementKey": "afterStatus", "unit": "", "simulatedResponse": "DONE" }
              ],
              "flow": {
                "name": "MainFlow",
                "nodes": [
                  { "type": "step", "step": "PollReady" },
                  { "type": "step", "step": "AfterReady" }
                ]
              },
              "rules": [
                { "name": "Ready Status", "targetKey": "readyStatus", "ruleType": "Equal", "expected": "READY" },
                { "name": "After Status", "targetKey": "afterStatus", "ruleType": "Equal", "expected": "DONE" }
              ]
            }
            """);

        var runner = new TestRunner();
        var result = await runner.RunAsync(
            new TestRunRequest("test simulate", recipePath, string.Empty, outputDirectory, string.Empty),
            CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected sequence without outcomePolicy to keep existing behavior.");
        AssertEqual("2", result.Steps.Count.ToString(), "Expected all sequence children to execute when no policy is configured.");
        AssertTrue(string.IsNullOrWhiteSpace(result.FlowResultTree!.StopReason), "Expected no sequence stop reason without policy.");
        AssertEqual("2", result.FlowResultTree.Children.Count.ToString(), "Expected result tree to retain both executed children.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestSequenceOutcomePolicyDoesNotOverrideContinueOnFailureErrorHandlingAsync()
    {
        var outputDirectory = CreateOutputDirectory("flow-sequence-policy-continue");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "flow-sequence-policy-continue.recipe.json",
            """
            {
              "name": "Sequence Policy Continue Recipe",
              "scripts": [
                { "name": "PowerOn", "command": "POWER_ON", "measurementKey": "power.status", "unit": "", "continueOnFailure": true, "simulatedResponse": "READY" },
                { "name": "ReadVoltage", "command": "READ_VOLTAGE", "measurementKey": "battery.voltage", "unit": "V", "simulatedResponse": "12.3" }
              ],
              "flow": {
                "name": "MainFlow",
                "outcomePolicy": "breakOnStepFailure",
                "nodes": [
                  { "type": "step", "step": "PowerOn" },
                  { "type": "step", "step": "ReadVoltage" }
                ]
              },
              "rules": [
                { "name": "Power Status", "targetKey": "power.status", "ruleType": "Equal", "expected": "READY" },
                { "name": "Battery Voltage", "targetKey": "battery.voltage", "ruleType": "Range", "min": 11.5, "max": 12.8 }
              ]
            }
            """);

        var runner = CreateTestRunner(
            new DelegateDeviceFactory(() =>
                new CallbackDevice(
                    "SequencePolicyContinueDevice",
                    (request, _, _) =>
                    {
                        if (string.Equals(request.Command, "POWER_ON", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new InvalidOperationException("Power relay fault.");
                        }

                        return Task.FromResult(
                            new DeviceCommandResponse
                            {
                                Command = request.Command,
                                Response = request.SimulatedResponse,
                                Success = true,
                                Message = "Sequence policy continue step completed."
                            });
                    })));

        var result = await runner.RunAsync(
            new TestRunRequest("test run", recipePath, string.Empty, outputDirectory, string.Empty),
            CancellationToken.None);

        AssertEqual("Error", result.Status, "Expected continueOnFailure execution error to keep overall status Error.");
        AssertEqual("2", result.Steps.Count.ToString(), "Expected sequence to continue after an Error status child.");
        AssertEqual("Error", result.Steps[0].FinalStatus, "Expected first child to record execution error.");
        AssertEqual("Passed", result.Steps[1].FinalStatus, "Expected second child to execute after the error.");
        AssertTrue(string.IsNullOrWhiteSpace(result.FlowResultTree!.StopReason), "Expected breakOnStepFailure not to trigger on Error status.");
        AssertEqual("2", result.FlowResultTree.Children.Count.ToString(), "Expected result tree to include both executed children.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestFlowResultTreeCapturesSequencePolicyStopReasonAndExecutedChildrenAsync()
    {
        var outputDirectory = CreateOutputDirectory("flow-sequence-policy-tree");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "flow-sequence-policy-tree.recipe.json",
            """
            {
              "name": "Sequence Policy Tree Recipe",
              "scripts": [
                { "name": "GateOpen", "command": "GATE_OPEN", "measurementKey": "gate.status", "unit": "", "simulatedResponse": "OPEN" },
                { "name": "AfterGate", "command": "AFTER_GATE", "measurementKey": "after.status", "unit": "", "simulatedResponse": "AFTER" },
                { "name": "Finalize", "command": "FINALIZE", "measurementKey": "final.status", "unit": "", "simulatedResponse": "DONE" }
              ],
              "flow": {
                "name": "MainFlow",
                "nodes": [
                  {
                    "type": "sequence",
                    "name": "GateSequence",
                    "outcomePolicy": "breakOnStepSuccess",
                    "nodes": [
                      { "type": "step", "step": "GateOpen" },
                      { "type": "step", "step": "AfterGate" }
                    ]
                  },
                  { "type": "step", "step": "Finalize" }
                ]
              },
              "rules": [
                { "name": "Gate Status", "targetKey": "gate.status", "ruleType": "Equal", "expected": "OPEN" },
                { "name": "After Status", "targetKey": "after.status", "ruleType": "Equal", "expected": "AFTER" },
                { "name": "Final Status", "targetKey": "final.status", "ruleType": "Equal", "expected": "DONE" }
              ]
            }
            """);

        var runner = new TestRunner();
        var result = await runner.RunAsync(
            new TestRunRequest("test simulate", recipePath, string.Empty, outputDirectory, string.Empty),
            CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected nested sequence policy to stop only the nested container.");
        AssertEqual("2", result.Steps.Count.ToString(), "Expected nested sequence to stop early while outer flow continues.");
        AssertEqual("GateOpen", result.Steps[0].StepName, "Expected nested sequence first child to execute.");
        AssertEqual("Finalize", result.Steps[1].StepName, "Expected outer sequence to continue after nested container break.");

        var nestedSequence = result.FlowResultTree!.Children[0];
        AssertEqual("Sequence", nestedSequence.NodeKind, "Expected first root child to be nested sequence.");
        AssertEqual("GateSequence", nestedSequence.NodeName, "Expected nested sequence node name.");
        AssertEqual("StepSuccessBreak", nestedSequence.StopReason, "Expected nested sequence stop reason.");
        AssertEqual("GateOpen", nestedSequence.TriggeredByNodeName, "Expected nested sequence triggering child name.");
        AssertEqual("1", nestedSequence.Children.Count.ToString(), "Expected nested sequence to contain only executed child nodes.");
        AssertEqual("Finalize", result.FlowResultTree.Children[1].NodeName, "Expected root flow to include subsequent outer child.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestFlowResultTreeCapturesNestedSequenceShapeAsync()
    {
        var outputDirectory = CreateOutputDirectory("flow-result-tree-sequence");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "flow-result-tree-sequence.recipe.json",
            """
            {
              "name": "Flow Result Tree Sequence Recipe",
              "scripts": [
                { "name": "ReadGate", "command": "READ_GATE", "measurementKey": "gateValue", "unit": "", "simulatedResponse": "OPEN" },
                { "name": "RunNested", "command": "RUN_NESTED", "measurementKey": "nestedValue", "unit": "", "simulatedResponse": "NESTED" }
              ],
              "flow": {
                "name": "MainFlow",
                "nodes": [
                  { "type": "step", "step": "ReadGate" },
                  {
                    "type": "sequence",
                    "name": "NestedSequence",
                    "nodes": [
                      { "type": "step", "step": "RunNested" }
                    ]
                  }
                ]
              },
              "rules": [
                { "name": "Gate Value", "targetKey": "gateValue", "ruleType": "Equal", "expected": "OPEN" },
                { "name": "Nested Value", "targetKey": "nestedValue", "ruleType": "Equal", "expected": "NESTED" }
              ]
            }
            """);

        var runner = new TestRunner();
        var result = await runner.RunAsync(
            new TestRunRequest("test simulate", recipePath, string.Empty, outputDirectory, string.Empty),
            CancellationToken.None);

        var root = result.FlowResultTree;
        AssertTrue(root is not null, "Expected flow result tree to be present.");
        AssertEqual("Sequence", root!.NodeKind, "Expected root flow result node kind.");
        AssertEqual("MainFlow", root.NodeName, "Expected root flow result node name.");
        AssertEqual("2", root.Children.Count.ToString(), "Expected root to contain two child nodes.");
        AssertEqual("Step", root.Children[0].NodeKind, "Expected first child to be a step node.");
        AssertEqual("ReadGate", root.Children[0].NodeName, "Expected first child step name.");
        AssertEqual("Sequence", root.Children[1].NodeKind, "Expected second child to be a nested sequence.");
        AssertEqual("NestedSequence", root.Children[1].NodeName, "Expected nested sequence name.");
        AssertEqual("1", root.Children[1].Children.Count.ToString(), "Expected nested sequence to contain one child step.");
        AssertEqual("RunNested", root.Children[1].Children[0].NodeName, "Expected nested step node name.");
        AssertTrue(root.CompletedAtUtc >= root.StartedAtUtc, "Expected root flow timestamps to be ordered.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestFlowResultTreeCapturesSelectedConditionBranchAsync()
    {
        var outputDirectory = CreateOutputDirectory("flow-result-tree-condition");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "flow-result-tree-condition.recipe.json",
            """
            {
              "name": "Flow Result Tree Condition Recipe",
              "scripts": [
                { "name": "ReadGate", "command": "READ_GATE", "measurementKey": "gateValue", "unit": "", "simulatedResponse": "OPEN" },
                { "name": "RunPathA", "command": "RUN_A", "measurementKey": "pathValueA", "unit": "", "simulatedResponse": "A" },
                { "name": "RunPathB", "command": "RUN_B", "measurementKey": "pathValueB", "unit": "", "simulatedResponse": "B" }
              ],
              "flow": {
                "name": "MainFlow",
                "nodes": [
                  { "type": "step", "step": "ReadGate" },
                  {
                    "type": "condition",
                    "name": "GateBranch",
                    "condition": { "type": "dataEquals", "key": "gateValue", "value": "OPEN" },
                    "whenTrue": [
                      { "type": "step", "step": "RunPathA" }
                    ],
                    "whenFalse": [
                      { "type": "step", "step": "RunPathB" }
                    ]
                  }
                ]
              },
              "rules": [
                { "name": "Gate Value", "targetKey": "gateValue", "ruleType": "Equal", "expected": "OPEN" },
                { "name": "Path Value A", "targetKey": "pathValueA", "ruleType": "Equal", "expected": "A" },
                { "name": "Path Value B", "targetKey": "pathValueB", "ruleType": "Equal", "expected": "B" }
              ]
            }
            """);

        var runner = new TestRunner();
        var result = await runner.RunAsync(
            new TestRunRequest("test simulate", recipePath, string.Empty, outputDirectory, string.Empty),
            CancellationToken.None);

        var branchNode = result.FlowResultTree!.Children[1];
        AssertEqual("Condition", branchNode.NodeKind, "Expected condition node kind.");
        AssertEqual("GateBranch", branchNode.NodeName, "Expected condition node name.");
        AssertEqual("dataEquals", branchNode.ConditionType, "Expected condition type to be recorded.");
        AssertEqual("True", branchNode.SelectedBranch, "Expected selected branch to be recorded.");
        AssertEqual("1", branchNode.Children.Count.ToString(), "Expected selected branch to contain one child result.");
        AssertEqual("RunPathA", branchNode.Children[0].NodeName, "Expected true-branch child node.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestFlowResultTreeCapturesRepeatIterationsAsync()
    {
        var outputDirectory = CreateOutputDirectory("flow-result-tree-repeat");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "flow-result-tree-repeat.recipe.json",
            """
            {
              "name": "Flow Result Tree Repeat Recipe",
              "scripts": [
                { "name": "PollReady", "command": "POLL_READY", "measurementKey": "readyStatus", "unit": "", "simulatedResponse": "READY" }
              ],
              "flow": {
                "name": "MainFlow",
                "nodes": [
                  {
                    "type": "repeatUntil",
                    "name": "WaitUntilReady",
                    "maxIterations": 3,
                    "failOnMaxIterations": true,
                    "until": { "type": "dataEquals", "key": "readyStatus", "value": "READY" },
                    "nodes": [
                      { "type": "step", "step": "PollReady" }
                    ]
                  }
                ]
              },
              "rules": [
                { "name": "Ready Status", "targetKey": "readyStatus", "ruleType": "Equal", "expected": "READY" }
              ]
            }
            """);

        var runner = CreateTestRunner(
            new DelegateDeviceFactory(() =>
                new CallbackDevice(
                    "RepeatTreeDevice",
                    (request, attemptNumber, _) =>
                        Task.FromResult(
                            new DeviceCommandResponse
                            {
                                Command = request.Command,
                                Response = attemptNumber == 1 ? "WAIT" : "READY",
                                Success = true,
                                Message = "Repeat tree poll complete."
                            }))));

        var result = await runner.RunAsync(
            new TestRunRequest("test run", recipePath, string.Empty, outputDirectory, string.Empty),
            CancellationToken.None);

        var repeatNode = result.FlowResultTree!.Children[0];
        AssertEqual("RepeatUntil", repeatNode.NodeKind, "Expected repeat node kind.");
        AssertEqual("WaitUntilReady", repeatNode.NodeName, "Expected repeat node name.");
        AssertEqual("3", repeatNode.MaxIterations.ToString(), "Expected max iterations metadata.");
        AssertEqual("2", repeatNode.CompletedIterations.ToString(), "Expected completed iterations metadata.");
        AssertEqual("ConditionSatisfied", repeatNode.StopReason, "Expected repeat stop reason.");
        AssertEqual("2", repeatNode.Iterations.Count.ToString(), "Expected per-iteration results.");
        AssertTrue(!repeatNode.Iterations[0].CountsTowardFinalStatus, "Expected first repeat iteration to be intermediate.");
        AssertTrue(repeatNode.Iterations[1].CountsTowardFinalStatus, "Expected final repeat iteration to count toward final status.");
        AssertEqual("Failed", repeatNode.Iterations[0].Children[0].Status, "Expected first repeat iteration step status.");
        AssertEqual("Passed", repeatNode.Iterations[1].Children[0].Status, "Expected final repeat iteration step status.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestFlowResultTreeCapturesRepeatMaxIterationsStopReasonAsync()
    {
        var outputDirectory = CreateOutputDirectory("flow-result-tree-repeat-max");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "flow-result-tree-repeat-max.recipe.json",
            """
            {
              "name": "Flow Result Tree Repeat Max Recipe",
              "scripts": [
                { "name": "PollReady", "command": "POLL_READY", "measurementKey": "readyStatus", "unit": "", "simulatedResponse": "READY" }
              ],
              "flow": {
                "name": "MainFlow",
                "nodes": [
                  {
                    "type": "repeatUntil",
                    "name": "WaitUntilReady",
                    "maxIterations": 2,
                    "failOnMaxIterations": true,
                    "until": { "type": "dataEquals", "key": "readyStatus", "value": "READY" },
                    "nodes": [
                      { "type": "step", "step": "PollReady" }
                    ]
                  }
                ]
              },
              "rules": [
                { "name": "Ready Status", "targetKey": "readyStatus", "ruleType": "Equal", "expected": "READY" }
              ]
            }
            """);

        var runner = CreateTestRunner(
            new DelegateDeviceFactory(() =>
                new CallbackDevice(
                    "RepeatTreeMaxDevice",
                    (request, _, _) =>
                        Task.FromResult(
                            new DeviceCommandResponse
                            {
                                Command = request.Command,
                                Response = "WAIT",
                                Success = true,
                                Message = "Repeat tree max poll incomplete."
                            }))));

        var result = await runner.RunAsync(
            new TestRunRequest("test run", recipePath, string.Empty, outputDirectory, string.Empty),
            CancellationToken.None);

        var repeatNode = result.FlowResultTree!.Children[0];
        AssertEqual("Error", repeatNode.Status, "Expected repeat node error status.");
        AssertEqual("MaxIterationsReached", repeatNode.StopReason, "Expected max-iterations stop reason.");
        AssertEqual("2", repeatNode.CompletedIterations.ToString(), "Expected completed iteration count.");
        AssertEqual("2", repeatNode.Iterations.Count.ToString(), "Expected iteration results even on failure.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestResultJsonEmitsFlowResultTreeAndPreservesFlatSummariesAsync()
    {
        var outputDirectory = CreateOutputDirectory("flow-result-tree-json");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "flow-result-tree-json.recipe.json",
            """
            {
              "name": "Flow Result Tree Json Recipe",
              "scripts": [
                { "name": "PollReady", "command": "POLL_READY", "measurementKey": "readyStatus", "unit": "", "simulatedResponse": "READY" }
              ],
              "flow": {
                "name": "MainFlow",
                "nodes": [
                  {
                    "type": "repeatUntil",
                    "name": "WaitUntilReady",
                    "maxIterations": 2,
                    "failOnMaxIterations": true,
                    "until": { "type": "dataEquals", "key": "readyStatus", "value": "READY" },
                    "nodes": [
                      { "type": "step", "step": "PollReady" }
                    ]
                  }
                ]
              },
              "rules": [
                { "name": "Ready Status", "targetKey": "readyStatus", "ruleType": "Equal", "expected": "READY" }
              ]
            }
            """);

        var runner = new TestRunner();
        var result = await runner.RunAsync(
            new TestRunRequest("test simulate", recipePath, string.Empty, outputDirectory, string.Empty),
            CancellationToken.None);

        var json = File.ReadAllText(result.ResultJsonPath);
        var persistedResult = JsonSerializer.Deserialize<TestResult>(json, CreateJsonOptions());

        AssertTrue(persistedResult is not null, "Expected persisted result json to deserialize.");
        AssertTrue(persistedResult!.FlowResultTree is not null, "Expected persisted result json to contain FlowResultTree.");
        AssertEqual(result.Steps.Count.ToString(), persistedResult.Steps.Count.ToString(), "Expected flat step summaries to remain present.");
        AssertEqual(result.Scripts.Count.ToString(), persistedResult.Scripts.Count.ToString(), "Expected flat script summaries to remain present.");
        AssertEqual("RepeatUntil", persistedResult.FlowResultTree!.Children[0].NodeKind, "Expected persisted flow tree node kind.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestRecipeValidationPassesAsync()
    {
        var repositoryRoot = FindRepositoryRoot();
        var recipePath = Path.Combine(repositoryRoot, "samples", "recipes", "phase2.recipe.json");
        var specPath = Path.Combine(repositoryRoot, "samples", "specs", "phase2.spec.json");
        var outputDirectory = CreateOutputDirectory("recipe-validate");
        var service = new RecipeValidationService();

        var result = await service.ValidateAsync(recipePath, specPath, outputDirectory, null, null, CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected recipe validation to pass.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestSpecValidationPassesAsync()
    {
        var repositoryRoot = FindRepositoryRoot();
        var specPath = Path.Combine(repositoryRoot, "samples", "specs", "phase2.spec.json");
        var outputDirectory = CreateOutputDirectory("spec-validate");
        var service = new SpecValidationService();

        var result = await service.ValidateAsync(specPath, outputDirectory, null, null, CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected spec validation to pass.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestMultiMeasurementRecipeValidationPassesAsync()
    {
        var repositoryRoot = FindRepositoryRoot();
        var recipePath = Path.Combine(repositoryRoot, "samples", "recipes", "multi-measurement.recipe.json");
        var specPath = Path.Combine(repositoryRoot, "samples", "specs", "multi-measurement.spec.json");
        var outputDirectory = CreateOutputDirectory("multi-recipe-validate");
        var service = new RecipeValidationService();

        var result = await service.ValidateAsync(recipePath, specPath, outputDirectory, null, null, CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected multi-measurement recipe validation to pass.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestMultiMeasurementSpecValidationPassesAsync()
    {
        var repositoryRoot = FindRepositoryRoot();
        var specPath = Path.Combine(repositoryRoot, "samples", "specs", "multi-measurement.spec.json");
        var outputDirectory = CreateOutputDirectory("multi-spec-validate");
        var service = new SpecValidationService();

        var result = await service.ValidateAsync(specPath, outputDirectory, null, null, CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected multi-measurement spec validation to pass.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestAllSpecTypesRecipeValidationPassesAsync()
    {
        var repositoryRoot = FindRepositoryRoot();
        var recipePath = Path.Combine(repositoryRoot, "samples", "recipes", "all-spec-types.recipe.json");
        var specPath = Path.Combine(repositoryRoot, "samples", "specs", "all-spec-types.spec.json");
        var outputDirectory = CreateOutputDirectory("all-types-recipe-validate");
        var service = new RecipeValidationService();

        var result = await service.ValidateAsync(recipePath, specPath, outputDirectory, null, null, CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected all spec types recipe validation to pass.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestAllSpecTypesSpecValidationPassesAsync()
    {
        var repositoryRoot = FindRepositoryRoot();
        var specPath = Path.Combine(repositoryRoot, "samples", "specs", "all-spec-types.spec.json");
        var outputDirectory = CreateOutputDirectory("all-types-spec-validate");
        var service = new SpecValidationService();

        var result = await service.ValidateAsync(specPath, outputDirectory, null, null, CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected all spec types spec validation to pass.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestDuplicateMeasurementFullKeysFailValidationAsync()
    {
        var outputDirectory = CreateOutputDirectory("duplicate-fullkey");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "duplicate-fullkey.recipe.json",
            """
            {
              "name": "Duplicate FullKey Recipe",
              "scripts": [
                {
                  "name": "BatterySnapshot",
                  "command": "READ_BATTERY_SNAPSHOT",
                  "prefix": "battery",
                  "measurements": [
                    {
                      "key": "voltage",
                      "unit": "V"
                    },
                    {
                      "key": "voltage",
                      "unit": "V"
                    }
                  ],
                  "simulatedResponse": "{\"voltage\":12.3}"
                }
              ],
              "rules": [
                {
                  "name": "Battery Voltage",
                  "targetKey": "battery.voltage",
                  "ruleType": "Range",
                  "min": 11.5,
                  "max": 12.8
                }
              ]
            }
            """);

        var service = new RecipeValidationService();
        var result = await service.ValidateAsync(recipePath, string.Empty, outputDirectory, null, null, CancellationToken.None);

        AssertEqual("Failed", result.Status, "Expected duplicate fullKey recipe validation to fail.");
        AssertContains(
            result.Errors,
            "Duplicate measurement fullKey 'battery.voltage'",
            "Expected duplicate fullKey validation error.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestMissingSpecTargetKeyFailsValidationAsync()
    {
        var outputDirectory = CreateOutputDirectory("missing-targetkey");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "missing-targetkey.recipe.json",
            """
            {
              "name": "Missing TargetKey Recipe",
              "scripts": [
                {
                  "name": "ReadBatteryVoltage",
                  "command": "READ_VOLTAGE",
                  "prefix": "battery",
                  "measurementKey": "voltage",
                  "unit": "V",
                  "simulatedResponse": "12.3"
                }
              ],
              "rules": [
                {
                  "name": "Battery Current",
                  "targetKey": "battery.current",
                  "ruleType": "Range",
                  "min": 0.0,
                  "max": 2.0
                }
              ]
            }
            """);

        var service = new RecipeValidationService();
        var result = await service.ValidateAsync(recipePath, string.Empty, outputDirectory, null, null, CancellationToken.None);

        AssertEqual("Failed", result.Status, "Expected missing targetKey recipe validation to fail.");
        AssertContains(
            result.Errors,
            "Script 'ReadBatteryVoltage' measurement fullKey 'battery.voltage' does not have an exact matching spec rule targetKey.",
            "Expected missing exact targetKey mapping error.");
        AssertContains(
            result.Errors,
            "Spec rule 'Battery Current' targetKey 'battery.current' does not match any declared measurement fullKey.",
            "Expected missing spec targetKey fullKey error.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestDataCollectionSupportsConcurrentFullKeyAccessAsync()
    {
        var collection = new DataCollection();
        var tasks = Enumerable.Range(0, 200)
            .Select(index => Task.Run(() =>
            {
                var fullKey = $"ch{index}.voltage";
                var value = index.ToString();
                var item = new MeasurementItem
                {
                    Key = "voltage",
                    Prefix = $"ch{index}",
                    FullKey = fullKey,
                    Value = value,
                    ValueType = MeasurementValueType.Number,
                    Unit = "V",
                    RawText = value
                };

                collection.Set(item);

                AssertTrue(collection.TryGetValue(fullKey, out var storedValue), $"Expected value for {fullKey}.");
                AssertEqual(value, storedValue ?? string.Empty, $"Expected stored value for {fullKey}.");
                AssertTrue(collection.TryGetItem(fullKey, out var storedItem), $"Expected measurement item for {fullKey}.");
                AssertEqual(fullKey, storedItem?.FullKey ?? string.Empty, $"Expected stored item fullKey for {fullKey}.");
            }))
            .ToArray();

        await Task.WhenAll(tasks);

        AssertEqual("200", collection.Values.Count.ToString(), "Expected concurrent value writes to be preserved.");
        AssertEqual("200", collection.Items.Count.ToString(), "Expected concurrent item writes to be preserved.");
    }

    private static async Task TestSessionLogUsesReadableTimestampElapsedAndItemNameAsync()
    {
        var repositoryRoot = FindRepositoryRoot();
        var recipePath = Path.Combine(repositoryRoot, "samples", "recipes", "all-spec-types.recipe.json");
        var specPath = Path.Combine(repositoryRoot, "samples", "specs", "all-spec-types.spec.json");
        var outputDirectory = CreateOutputDirectory("session-log-format");
        var runner = new TestRunner();

        var result = await runner.RunAsync(
            new TestRunRequest("test run", recipePath, specPath, outputDirectory, string.Empty),
            CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected sample run to pass before checking session.log.");

        var logLines = File.ReadAllLines(result.SessionLogPath);
        AssertTrue(logLines.Length > 0, "Expected session.log to contain at least one line.");
        AssertTrue(
            logLines.Any(line => line.StartsWith("========================= Session Header", StringComparison.Ordinal)),
            "Expected session.log header to include a session header section.");
        AssertTrue(
            logLines.Any(line => line.Contains("Application Release Time", StringComparison.Ordinal)),
            "Expected session.log header to include application release time.");
        AssertTrue(
            logLines.Any(line => line.StartsWith("========================= Runtime Logs", StringComparison.Ordinal)),
            "Expected session.log to include a runtime logs section.");
        AssertTrue(
            logLines.Any(line => line.StartsWith("======================= Test Summary", StringComparison.Ordinal)),
            "Expected session.log to include test summary block.");
        AssertTrue(
            logLines.Any(line => line.Contains("Product Serial Number", StringComparison.Ordinal)),
            "Expected session.log to include Product Serial Number field.");

        var firstTimedLine = logLines.First(line => line.Contains(" | +", StringComparison.Ordinal));
        var segments = firstTimedLine.Split(" | ", 5, StringSplitOptions.None);
        AssertEqual("5", segments.Length.ToString(), "Expected session log line to contain five formatted segments.");
        AssertTrue(
            DateTime.TryParseExact(
                segments[0],
                "yyyy-MM-dd HH:mm:ss.fff",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out _),
            "Expected readable local timestamp with milliseconds.");
        AssertTrue(
            segments[1].StartsWith("+", StringComparison.Ordinal) &&
            TimeSpan.TryParseExact(
                segments[1][1..],
                "hh\\:mm\\:ss\\.fff",
                System.Globalization.CultureInfo.InvariantCulture,
                out _),
            "Expected elapsed runtime segment with millisecond precision.");
        AssertTrue(
            logLines.Any(line => line.Contains(" | item=ReadBatterySnapshot | ", StringComparison.Ordinal)),
            "Expected session.log to clearly show the active step name.");
    }

    private static Task TestSessionArtifactTemplatesResolvePathsAndCreateDirectoriesAsync()
    {
        var repositoryRoot = FindRepositoryRoot();
        var outputDirectory = CreateOutputDirectory("artifact-templates");
        var factory = new SessionFactory();
        var context = factory.Create(
            "test run",
            outputDirectory,
            new SessionArtifactOptions
            {
                OutputDirectoryTemplate = "runs\\{SN}\\{yyyyMMdd_HHmmss}",
                ResultJsonTemplate = "json\\{Recipe}_{SN}_{SessionId}.json",
                ResultCsvTemplate = "csv\\{Recipe}_{SN}_{SessionId}.csv",
                SessionLogTemplate = "logs\\{SN}_{yyyyMMdd_HHmmss}.log",
                Variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["LoginUser"] = "MTE"
                }
            },
            new RunInputModel
            {
                SerialNumber = "SN-001",
                Station = "STATION-01",
                Mode = "RUN",
                Values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["LoginUser"] = "MTE"
                }
            },
            recipePath: Path.Combine(repositoryRoot, "samples", "recipes", "demo.recipe.json"));

        context.Log("Immediate log write check.", "artifact-template-test");

        AssertTrue(
            context.OutputDirectory.Contains(Path.Combine("runs", "SN-001"), StringComparison.OrdinalIgnoreCase),
            "Expected output directory template to use the SN variable.");
        AssertTrue(File.Exists(context.ArtifactPaths.SessionLogPath), "Expected session.log file to be created immediately.");
        AssertTrue(File.Exists(context.ArtifactPaths.StructuredLogPath), "Expected structured log file to be created immediately.");
        AssertTrue(Directory.Exists(Path.GetDirectoryName(context.ArtifactPaths.ResultJsonPath)!), "Expected JSON output directory to be created.");
        AssertTrue(Directory.Exists(Path.GetDirectoryName(context.ArtifactPaths.ResultCsvPath)!), "Expected CSV output directory to be created.");
        AssertTrue(
            context.ArtifactPaths.ResultJsonPath.Contains("demo_SN-001", StringComparison.OrdinalIgnoreCase),
            "Expected JSON path template to include recipe and SN.");
        AssertTrue(
            File.ReadAllLines(context.ArtifactPaths.SessionLogPath).Any(line => line.Contains("Immediate log write check.", StringComparison.Ordinal)),
            "Expected session.log to persist lines before the final artifact write.");
        AssertTrue(
            File.ReadAllLines(context.ArtifactPaths.StructuredLogPath).Any(line => line.Contains("SessionStarted", StringComparison.Ordinal)),
            "Expected structured log to persist initial session events.");

        return Task.CompletedTask;
    }

    private static Task TestRepeatedRunsCreateUniqueArtifactPathsAsync()
    {
        var outputDirectory = CreateOutputDirectory("artifact-reuse");
        var factory = new SessionFactory();

        var firstContext = factory.Create("test run", outputDirectory);
        File.WriteAllText(firstContext.ArtifactPaths.ResultJsonPath, "{}");
        File.WriteAllText(firstContext.ArtifactPaths.ResultCsvPath, "col1,col2");
        firstContext.Log("First run log.", "first-run");

        var secondContext = factory.Create("test run", outputDirectory);

        AssertTrue(
            !string.Equals(firstContext.ArtifactPaths.SessionLogPath, secondContext.ArtifactPaths.SessionLogPath, StringComparison.OrdinalIgnoreCase),
            "Expected repeated runs to use a unique session.log path.");
        AssertTrue(
            !string.Equals(firstContext.ArtifactPaths.ResultJsonPath, secondContext.ArtifactPaths.ResultJsonPath, StringComparison.OrdinalIgnoreCase),
            "Expected repeated runs to use a unique result.json path when the previous file already exists.");
        AssertTrue(
            !string.Equals(firstContext.ArtifactPaths.ResultCsvPath, secondContext.ArtifactPaths.ResultCsvPath, StringComparison.OrdinalIgnoreCase),
            "Expected repeated runs to use a unique result.csv path when the previous file already exists.");
        AssertTrue(File.Exists(secondContext.ArtifactPaths.SessionLogPath), "Expected second session.log file to be created immediately.");
        AssertTrue(
            !string.Equals(firstContext.ArtifactPaths.StructuredLogPath, secondContext.ArtifactPaths.StructuredLogPath, StringComparison.OrdinalIgnoreCase),
            "Expected repeated runs to use a unique structured log path.");

        return Task.CompletedTask;
    }

    private static async Task TestStructuredLogUsesVersionedGlobalSequenceAsync()
    {
        var repositoryRoot = FindRepositoryRoot();
        var recipePath = Path.Combine(repositoryRoot, "samples", "recipes", "all-spec-types.recipe.json");
        var specPath = Path.Combine(repositoryRoot, "samples", "specs", "all-spec-types.spec.json");
        var outputDirectory = CreateOutputDirectory("structured-log-sequence");
        var runner = new TestRunner();

        var result = await runner.RunAsync(
            new TestRunRequest("test run", recipePath, specPath, outputDirectory, string.Empty),
            CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected sample run to pass before checking structured log.");
        var entries = ReadStructuredLogEntries(result.StructuredLogPath);
        AssertTrue(entries.Count > 0, "Expected structured log entries.");
        AssertTrue(
            entries.All(item => string.Equals(item.SchemaVersion, StructuredLogEntry.CurrentSchemaVersion, StringComparison.Ordinal)),
            "Expected every structured log entry to use the current schema version.");

        for (var index = 0; index < entries.Count; index++)
        {
            AssertEqual((index + 1).ToString(), entries[index].Sequence.ToString(), "Expected structured log sequence to be session-global and strictly increasing.");
        }

        AssertTrue(entries.Any(item => item.EntryType == StructuredLogEntryType.SessionStarted), "Expected SessionStarted entry.");
        AssertTrue(entries.Any(item => item.EntryType == StructuredLogEntryType.MeasurementCollected), "Expected MeasurementCollected entry.");
        AssertTrue(entries.Any(item => item.EntryType == StructuredLogEntryType.SpecEvaluated), "Expected SpecEvaluated entry.");
        AssertTrue(entries.Any(item => item.EntryType == StructuredLogEntryType.SessionCompleted), "Expected SessionCompleted entry.");
    }

    private static async Task TestSessionInfoIsCanonicalMetadataSourceAsync()
    {
        var repositoryRoot = FindRepositoryRoot();
        var recipePath = Path.Combine(repositoryRoot, "samples", "recipes", "demo.recipe.json");
        var outputDirectory = CreateOutputDirectory("sessioninfo-canonical");
        var runner = new TestRunner();

        var result = await runner.RunAsync(
            new TestRunRequest(
                "test run",
                recipePath,
                string.Empty,
                outputDirectory,
                string.Empty,
                null,
                new RunInputModel
                {
                    SerialNumber = "SN-CANON-01",
                    Station = "ST-CANON",
                    Mode = "RUN",
                    Values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["LoginUser"] = "MTE"
                    }
                }),
            CancellationToken.None);

        AssertEqual(result.SessionId, result.SessionInfo.SessionId, "Expected SessionInfo.SessionId to mirror the session.");
        AssertEqual(result.RunInput.SerialNumber, result.SessionInfo.SerialNumber, "Expected SessionInfo.SerialNumber to be canonical.");
        AssertEqual(result.RunInput.Station, result.SessionInfo.Station, "Expected SessionInfo.Station to be canonical.");
        AssertEqual(result.Status, result.SessionInfo.FinalStatus, "Expected SessionInfo.FinalStatus to be canonical.");
        AssertEqual(result.StructuredLogPath, result.SessionInfo.Artifacts.StructuredLogPath, "Expected SessionInfo artifact manifest to include structured log path.");
        AssertTrue(
            result.SessionInfo.Inputs.TryGetValue("SN", out var serialNumber) && string.Equals(serialNumber, "SN-CANON-01", StringComparison.Ordinal),
            "Expected SessionInfo inputs to contain SN.");
    }

    private static Task TestVariableResolverUsesStepGlobalPrecedenceAsync()
    {
        var resolver = new VariableResolver();
        var variableContext = new VariableContext
        {
            GlobalVariables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Target"] = "GLOBAL"
            },
            StepVariables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Target"] = "STEP"
            }
        };

        AssertEqual(
            "RUN_STEP",
            resolver.ResolveTemplate("RUN_${Target}", variableContext, "Command", "ResolverStep"),
            "Expected step scope to have highest precedence.");

        variableContext.StepVariables.Clear();
        AssertEqual(
            "RUN_GLOBAL",
            resolver.ResolveTemplate("RUN_${Target}", variableContext, "Command", "ResolverStep"),
            "Expected global scope to be used when step scope is missing.");

        return Task.CompletedTask;
    }

    private static Task TestArtifactSummaryBuilderNormalizesRunArtifactsAsync()
    {
        var builder = new ArtifactSummaryBuilder();
        var result = new TestResult
        {
            SessionId = "SESSION-001",
            CommandName = "test run",
            RecipeName = "Summary Recipe",
            DeviceName = "FakeDevice",
            Status = "Failed",
            ResultJsonPath = "result.json",
            ResultCsvPath = "result.csv",
            SessionLogPath = "session.log",
            StructuredLogPath = "session.events.jsonl",
            RunInput = new RunInputModel
            {
                SerialNumber = "SN-001",
                Station = "ST-01",
                Mode = "RUN"
            },
            StartedAtUtc = DateTimeOffset.UtcNow.AddSeconds(-5),
            CompletedAtUtc = DateTimeOffset.UtcNow,
            Steps = new List<StepResult>
            {
                new()
                {
                    StepName = "ReadVoltage",
                    FinalStatus = "Passed",
                    Measurements = new List<MeasurementItem>
                    {
                        new()
                        {
                            Key = "voltage",
                            FullKey = "battery.voltage",
                            Value = "12.3",
                            ValueType = MeasurementValueType.Number,
                            Unit = "V"
                        }
                    },
                    SpecResults = new List<SpecEvaluationResult>
                    {
                        new()
                        {
                            RuleName = "Voltage Range",
                            TargetKey = "battery.voltage",
                            RuleType = "Range",
                            PassFail = "Passed"
                        }
                    }
                },
                new()
                {
                    StepName = "ReadCurrent",
                    FinalStatus = "Failed",
                    Measurements = new List<MeasurementItem>
                    {
                        new()
                        {
                            Key = "current",
                            FullKey = "battery.current",
                            Value = "3.5",
                            ValueType = MeasurementValueType.Number,
                            Unit = "A"
                        }
                    },
                    SpecResults = new List<SpecEvaluationResult>
                    {
                        new()
                        {
                            RuleName = "Current Range",
                            TargetKey = "battery.current",
                            RuleType = "Range",
                            PassFail = "Failed"
                        }
                    }
                }
            },
            Errors = new List<string>
            {
                "Spec failure recorded."
            }
        };

        var summary = builder.Build(result, @"E:\summary\result.json");

        AssertEqual("SESSION-001", summary.SessionId, "Expected session id to be copied into normalized summary.");
        AssertEqual("2", summary.StepCount.ToString(), "Expected step count to be normalized.");
        AssertEqual("1", summary.PassedStepCount.ToString(), "Expected passed step count to be normalized.");
        AssertEqual("1", summary.FailedStepCount.ToString(), "Expected failed step count to be normalized.");
        AssertEqual("2", summary.MeasurementCount.ToString(), "Expected measurement count to be normalized.");
        AssertEqual("2", summary.SpecCount.ToString(), "Expected spec count to be normalized.");
        AssertEqual("1", summary.FailedSpecCount.ToString(), "Expected failed spec count to be normalized.");
        AssertTrue(
            summary.FailedStepNames.Contains("ReadCurrent", StringComparer.Ordinal),
            "Expected failed step names to include the failing step.");
        AssertTrue(
            summary.FailedTargetKeys.Contains("battery.current", StringComparer.Ordinal),
            "Expected failed target keys to include the failing target.");

        return Task.CompletedTask;
    }

    private static async Task TestRuleBasedRunAnalyzerClassifiesVariableResolutionFailuresAsync()
    {
        var analyzer = new RuleBasedRunAnalyzer();
        var result = await analyzer.AnalyzeAsync(
            new AiRunAnalysisRequest
            {
                ResultJsonPath = @"E:\summary\result.json",
                ArtifactSummary = new RunArtifactSummary
                {
                    SourcePath = @"E:\summary\result.json",
                    SessionId = "SESSION-002",
                    CommandName = "test run",
                    RecipeName = "Analyzer Recipe",
                    RunStatus = "Error",
                    StepCount = 1,
                    MeasurementCount = 0,
                    SpecCount = 0,
                    ErrorCount = 1,
                    ErrorStepCount = 1,
                    ErrorStepNames = new List<string> { "ReadVoltage" },
                    VariableResolutionFailedCount = 1,
                    HasVariableResolutionFailures = true,
                    FirstFailureMessage = "Variable 'dut.sn' required by field 'Command' in step 'ReadVoltage' was not found in DutContext."
                }
            },
            CancellationToken.None);

        AssertEqual("RuleBasedRunAnalyzer", result.AnalyzerName, "Expected rule-based analyzer name.");
        AssertEqual("Configuration", result.PrimaryCategory, "Expected variable resolution failures to classify as configuration issues.");
        AssertTrue(
            result.PrimaryCause.Contains("Variable resolution failed", StringComparison.Ordinal),
            "Expected variable resolution primary cause.");
        AssertTrue(
            result.RecommendedActions.Any(item =>
                item.Contains("session.events.jsonl", StringComparison.Ordinal)),
            "Expected recommendation to inspect structured events for missing variables.");
    }

    private static async Task TestRuleBasedRunAnalyzerClassifiesUnhandledExceptionsAsync()
    {
        var analyzer = new RuleBasedRunAnalyzer();
        var result = await analyzer.AnalyzeAsync(
            new AiRunAnalysisRequest
            {
                ResultJsonPath = @"E:\summary\result.json",
                ArtifactSummary = new RunArtifactSummary
                {
                    SourcePath = @"E:\summary\result.json",
                    SessionId = "SESSION-EX-001",
                    CommandName = "test run",
                    RecipeName = "Exception Recipe",
                    RunStatus = "Error",
                    StepCount = 1,
                    ErrorCount = 1,
                    ExceptionCount = 1,
                    HasUnhandledException = true,
                    FirstExceptionMessage = "Unhandled exception: device timeout."
                }
            },
            CancellationToken.None);

        AssertEqual("Runtime", result.PrimaryCategory, "Expected exception classification category.");
        AssertTrue(
            result.PrimaryCause.Contains("unhandled exception", StringComparison.OrdinalIgnoreCase),
            "Expected exception primary cause.");
        AssertTrue(
            result.RecommendedActions.Any(item =>
                item.Contains("session.log", StringComparison.Ordinal)),
            "Expected recommendation to inspect session.log.");
    }

    private static async Task TestRuleBasedRunAnalyzerClassifiesStepFailuresAsync()
    {
        var analyzer = new RuleBasedRunAnalyzer();
        var result = await analyzer.AnalyzeAsync(
            new AiRunAnalysisRequest
            {
                ResultJsonPath = @"E:\summary\result.json",
                ArtifactSummary = new RunArtifactSummary
                {
                    SourcePath = @"E:\summary\result.json",
                    SessionId = "SESSION-STEP-001",
                    CommandName = "test run",
                    RecipeName = "Step Failure Recipe",
                    RunStatus = "Failed",
                    StepCount = 2,
                    FailedStepNames = new List<string> { "MeasureResistance" },
                    FirstFailureMessage = "Step 'MeasureResistance' completed with status 'Failed'."
                }
            },
            CancellationToken.None);

        AssertEqual("Execution", result.PrimaryCategory, "Expected failed step category.");
        AssertTrue(
            result.PrimaryCause.Contains("steps", StringComparison.OrdinalIgnoreCase),
            "Expected failed step cause.");
        AssertTrue(
            result.RecommendedActions.Any(item =>
                item.Contains("failed step names", StringComparison.OrdinalIgnoreCase)),
            "Expected failed step recommendation.");
    }

    private static async Task TestRuleBasedRunAnalyzerClassifiesSuccessAsync()
    {
        var analyzer = new RuleBasedRunAnalyzer();
        var result = await analyzer.AnalyzeAsync(
            new AiRunAnalysisRequest
            {
                ResultJsonPath = @"E:\summary\result.json",
                ArtifactSummary = new RunArtifactSummary
                {
                    SourcePath = @"E:\summary\result.json",
                    SessionId = "SESSION-SUCCESS-001",
                    CommandName = "test run",
                    RecipeName = "Success Recipe",
                    RunStatus = "Passed",
                    StepCount = 2,
                    MeasurementCount = 3,
                    SpecCount = 3
                }
            },
            CancellationToken.None);

        AssertEqual("Success", result.PrimaryCategory, "Expected success category.");
        AssertTrue(
            result.PrimaryCause.Contains("completed successfully", StringComparison.OrdinalIgnoreCase),
            "Expected success cause.");
        AssertTrue(
            result.RecommendedActions.Count > 0,
            "Expected success recommendations to be populated.");
    }

    private static async Task TestRuleBasedRunAnalyzerUsesMixedFailurePrecedenceAsync()
    {
        var analyzer = new RuleBasedRunAnalyzer();
        var result = await analyzer.AnalyzeAsync(
            new AiRunAnalysisRequest
            {
                ResultJsonPath = @"E:\summary\result.json",
                ArtifactSummary = new RunArtifactSummary
                {
                    SourcePath = @"E:\summary\result.json",
                    SessionId = "SESSION-MIXED-001",
                    CommandName = "test run",
                    RecipeName = "Mixed Recipe",
                    RunStatus = "Error",
                    FailedSpecCount = 1,
                    FailedTargetKeys = new List<string> { "battery.voltage" },
                    FailedStepNames = new List<string> { "ReadVoltage" },
                    VariableResolutionFailedCount = 1,
                    HasVariableResolutionFailures = true,
                    FirstFailureMessage = "Variable 'dut.sn' was not found."
                }
            },
            CancellationToken.None);

        AssertEqual("Configuration", result.PrimaryCategory, "Expected variable resolution failure to outrank step/spec failures.");
        AssertTrue(
            result.PrimaryCause.Contains("Variable resolution failed", StringComparison.Ordinal),
            "Expected precedence to select variable resolution cause.");
    }

    private static async Task TestRuleBasedRunAnalyzerEmitsEvidenceForMatchedRulesAsync()
    {
        var analyzer = new RuleBasedRunAnalyzer();
        var result = await analyzer.AnalyzeAsync(
            new AiRunAnalysisRequest
            {
                ResultJsonPath = @"E:\summary\result.json",
                ArtifactSummary = new RunArtifactSummary
                {
                    SourcePath = @"E:\summary\result.json",
                    SessionId = "SESSION-EVIDENCE-001",
                    CommandName = "test run",
                    RecipeName = "Evidence Recipe",
                    RunStatus = "Error",
                    VariableResolutionFailedCount = 2,
                    HasVariableResolutionFailures = true,
                    FirstFailureMessage = "Variable 'SN' was not found."
                }
            },
            CancellationToken.None);

        AssertTrue(
            result.Evidence.Any(item =>
                string.Equals(item.Source, "RunArtifactSummary.VariableResolutionFailedCount", StringComparison.Ordinal) &&
                string.Equals(item.Value, "2", StringComparison.Ordinal)),
            "Expected variable resolution evidence count.");
        AssertTrue(
            result.Evidence.Any(item =>
                string.Equals(item.Source, "RunArtifactSummary.FirstFailureMessage", StringComparison.Ordinal) &&
                item.Value.Contains("Variable 'SN'", StringComparison.Ordinal)),
            "Expected first failure message evidence.");
        AssertTrue(
            result.MatchedRules.Contains("VariableResolutionFailureRule", StringComparer.Ordinal),
            "Expected matched rules to include VariableResolutionFailureRule.");
    }

    private static async Task TestRuleBasedRunAnalyzerKeepsPrecedenceAndEvidenceConsistentAsync()
    {
        var analyzer = new RuleBasedRunAnalyzer();
        var result = await analyzer.AnalyzeAsync(
            new AiRunAnalysisRequest
            {
                ResultJsonPath = @"E:\summary\result.json",
                ArtifactSummary = new RunArtifactSummary
                {
                    SourcePath = @"E:\summary\result.json",
                    SessionId = "SESSION-EVIDENCE-002",
                    CommandName = "test run",
                    RecipeName = "Mixed Evidence Recipe",
                    RunStatus = "Error",
                    FailedStepNames = new List<string> { "ReadVoltage" },
                    FailedSpecCount = 1,
                    FailedTargetKeys = new List<string> { "battery.voltage" },
                    VariableResolutionFailedCount = 1,
                    HasVariableResolutionFailures = true,
                    FirstFailureMessage = "Variable 'dut.sn' was not found."
                }
            },
            CancellationToken.None);

        AssertEqual("Configuration", result.PrimaryCategory, "Expected precedence to remain configuration-first.");
        AssertTrue(
            result.MatchedRules.Contains("StepFailureRule", StringComparer.Ordinal),
            "Expected secondary matched rule to remain visible.");
        AssertTrue(
            result.Evidence.Any(item =>
                string.Equals(item.Source, "RunArtifactSummary.FailedStepNames", StringComparison.Ordinal) &&
                item.Value.Contains("ReadVoltage", StringComparison.Ordinal)),
            "Expected evidence to include failed step support even when configuration wins precedence.");
    }

    private static Task TestAiAnalysisBundleBuilderPopulatesMetadataAndContentAsync()
    {
        var summary = new RunArtifactSummary
        {
            SourcePath = @"E:\summary\result.json",
            SessionId = "SESSION-BUNDLE-001",
            CommandName = "test run",
            RecipeName = "Bundle Recipe",
            RunStatus = "Error",
            VariableResolutionFailedCount = 1,
            HasVariableResolutionFailures = true,
            FirstFailureMessage = "Variable 'dut.sn' was not found."
        };
        var analysis = new AiRunAnalysisResult
        {
            AnalyzerName = "RuleBasedRunAnalyzer",
            PrimaryCategory = "Configuration",
            PrimaryCause = "Variable resolution failed before the run could complete.",
            Summary = "Run status=Error, steps=1, failedSpecs=0, errors=1, variableFailures=1, exceptions=0, warnings=0.",
            Evidence = new List<AiEvidenceItem>
            {
                new()
                {
                    Type = "Metric",
                    Message = "Variable resolution failure count",
                    Source = "RunArtifactSummary.VariableResolutionFailedCount",
                    Value = "1"
                }
            },
            MatchedRules = new List<string>
            {
                "VariableResolutionFailureRule"
            }
        };

        var builder = new AiAnalysisBundleBuilder();
        var bundle = builder.Build(
            summary,
            analysis,
            @"E:\summary\result.json",
            @"E:\summary\session.events.jsonl",
            @"E:\summary\analysis.json");

        AssertEqual("ats.ai-analysis-bundle.v1", bundle.SchemaVersion, "Expected bundle schema version.");
        AssertTrue(bundle.GeneratedAtUtc > DateTimeOffset.MinValue, "Expected bundle generated timestamp.");
        AssertEqual("RuleBasedRunAnalyzer", bundle.AnalyzerName, "Expected analyzer name to be promoted to bundle metadata.");
        AssertEqual(Path.GetFullPath(@"E:\summary\result.json"), bundle.ResultJsonPath, "Expected result json path metadata.");
        AssertEqual(Path.GetFullPath(@"E:\summary\session.events.jsonl"), bundle.EventsJsonlPath, "Expected events jsonl path metadata.");
        AssertEqual(Path.GetFullPath(@"E:\summary\analysis.json"), bundle.AnalysisJsonPath, "Expected analysis json path metadata.");
        AssertEqual("SESSION-BUNDLE-001", bundle.Summary.SessionId, "Expected summary to be embedded.");
        AssertEqual("Configuration", bundle.Analysis.PrimaryCategory, "Expected analysis to be embedded.");
        AssertTrue(bundle.Analysis.Evidence.Count > 0, "Expected bundle analysis to include evidence.");

        return Task.CompletedTask;
    }

    private static async Task TestAiAnalysisBundleWriterWritesBundleJsonAsync()
    {
        var outputDirectory = CreateOutputDirectory("ai-bundle-writer");
        var outputPath = Path.Combine(outputDirectory, "bundle", "analysis-bundle.json");
        var bundle = new AiAnalysisBundleBuilder().Build(
            new RunArtifactSummary
            {
                SourcePath = Path.Combine(outputDirectory, "result.json"),
                SessionId = "SESSION-BUNDLE-WRITER-001",
                CommandName = "test run",
                RecipeName = "Bundle Writer Recipe",
                RunStatus = "Passed",
                StepCount = 1
            },
            new AiRunAnalysisResult
            {
                AnalyzerName = "RuleBasedRunAnalyzer",
                PrimaryCategory = "Success",
                PrimaryCause = "The run completed successfully without detected failures.",
                Summary = "Run status=Passed, steps=1, failedSpecs=0, errors=0, variableFailures=0, exceptions=0, warnings=0.",
                Evidence = new List<AiEvidenceItem>
                {
                    new()
                    {
                        Type = "Status",
                        Message = "Run status",
                        Source = "RunArtifactSummary.RunStatus",
                        Value = "Passed"
                    }
                },
                MatchedRules = new List<string>
                {
                    "SuccessRule"
                }
            },
            Path.Combine(outputDirectory, "result.json"));

        var writer = new AiAnalysisBundleWriter();
        var fullPath = await writer.WriteAsync(bundle, outputPath, CancellationToken.None);

        AssertEqual(Path.GetFullPath(outputPath), fullPath, "Expected bundle writer to return full output path.");
        AssertTrue(File.Exists(fullPath), "Expected bundle json file to be written.");

        var json = File.ReadAllText(fullPath);
        var writtenBundle = JsonSerializer.Deserialize<AiAnalysisBundle>(json, CreateJsonOptions());

        AssertTrue(writtenBundle is not null, "Expected written bundle to deserialize.");
        AssertEqual("SESSION-BUNDLE-WRITER-001", writtenBundle!.Summary.SessionId, "Expected bundle summary to round-trip.");
        AssertEqual("Success", writtenBundle.Analysis.PrimaryCategory, "Expected bundle analysis to round-trip.");
        AssertTrue(writtenBundle.Analysis.Evidence.Count > 0, "Expected bundle analysis evidence to round-trip.");
    }

    private static async Task TestAiAnalysisHtmlRendererGeneratesHtmlAsync()
    {
        var outputDirectory = CreateOutputDirectory("ai-html-renderer");
        var outputPath = Path.Combine(outputDirectory, "viewer", "analysis-viewer.html");
        var bundle = CreateSampleAnalysisBundle(Path.Combine(outputDirectory, "result.json"));
        var writer = new AiAnalysisHtmlWriter();

        var fullPath = await writer.WriteAsync(bundle, outputPath, CancellationToken.None);

        AssertEqual(Path.GetFullPath(outputPath), fullPath, "Expected html writer to return full output path.");
        AssertTrue(File.Exists(fullPath), "Expected html viewer file to be written.");

        var html = File.ReadAllText(fullPath);
        AssertTrue(html.Contains("<!DOCTYPE html>", StringComparison.Ordinal), "Expected html doctype.");
        AssertTrue(html.Contains("ATS Analysis Viewer", StringComparison.Ordinal), "Expected html title.");
        AssertTrue(html.Contains("analysis-viewer", StringComparison.OrdinalIgnoreCase) || html.Contains("Analysis Bundle Viewer", StringComparison.Ordinal), "Expected viewer heading.");
    }

    private static Task TestAiAnalysisHtmlRendererIncludesRequiredSectionsAsync()
    {
        var bundle = CreateSampleAnalysisBundle(@"E:\summary\result.json");
        var renderer = new AiAnalysisHtmlRenderer();
        var html = renderer.Render(bundle);

        AssertTrue(html.Contains("Header / Artifact Metadata", StringComparison.Ordinal), "Expected metadata section.");
        AssertTrue(html.Contains("Primary Category", StringComparison.Ordinal), "Expected primary category section.");
        AssertTrue(html.Contains("Primary Cause", StringComparison.Ordinal), "Expected primary cause section.");
        AssertTrue(html.Contains("Confidence", StringComparison.Ordinal), "Expected confidence section.");
        AssertTrue(html.Contains("Observations", StringComparison.Ordinal), "Expected observations section.");
        AssertTrue(html.Contains("Recommended Actions", StringComparison.Ordinal), "Expected recommended actions section.");
        AssertTrue(html.Contains("Evidence", StringComparison.Ordinal), "Expected evidence section.");
        AssertTrue(html.Contains("Normalized Summary Facts", StringComparison.Ordinal), "Expected normalized summary facts section.");

        return Task.CompletedTask;
    }

    private static Task TestAiAnalysisHtmlRendererIncludesInteractiveSectionsAsync()
    {
        var bundle = CreateSampleAnalysisBundle(@"E:\summary\result.json");
        var renderer = new AiAnalysisHtmlRenderer();
        var html = renderer.Render(bundle);

        AssertTrue(html.Contains("Interactive Controls", StringComparison.Ordinal), "Expected interactive controls section.");
        AssertTrue(html.Contains("<details class=\"section-panel\"", StringComparison.Ordinal), "Expected collapsible details sections.");
        AssertTrue(html.Contains("data-group-category=\"observations\"", StringComparison.Ordinal), "Expected observations details section metadata.");
        AssertTrue(html.Contains("data-group-category=\"evidence\"", StringComparison.Ordinal), "Expected evidence details section metadata.");
        AssertTrue(html.Contains("data-group-category=\"matched-rules\"", StringComparison.Ordinal), "Expected matched rules details section metadata.");
        AssertTrue(html.Contains("data-group-category=\"summary-facts\"", StringComparison.Ordinal), "Expected summary facts details section metadata.");
        AssertTrue(html.Contains("ATS Interactive Offline Analysis Viewer", StringComparison.Ordinal), "Expected interactive viewer heading.");

        return Task.CompletedTask;
    }

    private static Task TestAiAnalysisHtmlRendererRendersSearchAndFilterMarkupAsync()
    {
        var bundle = CreateSampleAnalysisBundle(@"E:\summary\result.json");
        var renderer = new AiAnalysisHtmlRenderer();
        var html = renderer.Render(bundle);

        AssertTrue(html.Contains("id=\"viewer-search\"", StringComparison.Ordinal), "Expected search input markup.");
        AssertTrue(html.Contains("data-viewer-severity-filter", StringComparison.Ordinal), "Expected severity filter markup.");
        AssertTrue(html.Contains("data-viewer-category-filter", StringComparison.Ordinal), "Expected category filter markup.");
        AssertTrue(html.Contains("data-search-item", StringComparison.Ordinal), "Expected searchable item markup.");
        AssertTrue(html.Contains("data-viewer-match-count", StringComparison.Ordinal), "Expected match-count markup.");
        AssertTrue(html.Contains("applyViewerFilters", StringComparison.Ordinal), "Expected inline viewer filter script.");
        AssertTrue(html.Contains("<noscript>", StringComparison.Ordinal), "Expected no-script readability note.");

        return Task.CompletedTask;
    }

    private static Task TestAiAnalysisBundleComparisonBuilderHandlesSameBundleComparisonAsync()
    {
        var bundle = CreateSampleAnalysisBundle(@"E:\comparison\left-result.json");
        var comparison = new AiAnalysisBundleComparisonBuilder().Build(bundle, bundle);

        AssertTrue(!comparison.HasDifferences, "Expected identical bundles to produce no differences.");
        AssertTrue(!comparison.PrimaryCategory.Changed, "Expected primary category to remain unchanged.");
        AssertEqual("0", comparison.SummaryCountChanges.Count.ToString(), "Expected no summary count changes.");
        AssertEqual("0", comparison.AddedMatchedRules.Count.ToString(), "Expected no added matched rules.");
        AssertEqual("0", comparison.RemovedEvidence.Count.ToString(), "Expected no removed evidence.");

        return Task.CompletedTask;
    }

    private static Task TestAiAnalysisBundleComparisonBuilderDetectsCategoryChangesAsync()
    {
        var leftBundle = CreateSampleAnalysisBundle(@"E:\comparison\left-result.json");
        var rightBundle = CreateComparisonVariantBundle(@"E:\comparison\right-result.json");
        var comparison = new AiAnalysisBundleComparisonBuilder().Build(leftBundle, rightBundle);

        AssertTrue(comparison.HasDifferences, "Expected differing bundles to report differences.");
        AssertTrue(comparison.PrimaryCategory.Changed, "Expected primary category change.");
        AssertEqual("Configuration", comparison.PrimaryCategory.LeftValue, "Expected left primary category.");
        AssertEqual("Runtime", comparison.PrimaryCategory.RightValue, "Expected right primary category.");

        return Task.CompletedTask;
    }

    private static Task TestAiAnalysisBundleComparisonBuilderDetectsSummaryCountChangesAsync()
    {
        var leftBundle = CreateSampleAnalysisBundle(@"E:\comparison\left-result.json");
        var rightBundle = CreateComparisonVariantBundle(@"E:\comparison\right-result.json");
        var comparison = new AiAnalysisBundleComparisonBuilder().Build(leftBundle, rightBundle);

        AssertTrue(
            comparison.SummaryCountChanges.Any(item =>
                string.Equals(item.Label, "Failed Step Count", StringComparison.Ordinal) &&
                item.LeftValue == 1 &&
                item.RightValue == 2 &&
                item.Delta == 1),
            "Expected failed step count change.");
        AssertTrue(
            comparison.SummaryCountChanges.Any(item =>
                string.Equals(item.Label, "Exception Count", StringComparison.Ordinal) &&
                item.LeftValue == 0 &&
                item.RightValue == 1),
            "Expected exception count change.");

        return Task.CompletedTask;
    }

    private static Task TestAiAnalysisBundleComparisonBuilderDetectsMatchedRuleChangesAsync()
    {
        var leftBundle = CreateSampleAnalysisBundle(@"E:\comparison\left-result.json");
        var rightBundle = CreateComparisonVariantBundle(@"E:\comparison\right-result.json");
        var comparison = new AiAnalysisBundleComparisonBuilder().Build(leftBundle, rightBundle);

        AssertTrue(
            comparison.AddedMatchedRules.Any(item => string.Equals(item, "UnhandledExceptionRule", StringComparison.Ordinal)),
            "Expected added matched rule.");
        AssertTrue(
            comparison.AddedMatchedRules.Any(item => string.Equals(item, "StepFailureRule", StringComparison.Ordinal)),
            "Expected second added matched rule.");
        AssertTrue(
            comparison.RemovedMatchedRules.Any(item => string.Equals(item, "VariableResolutionFailureRule", StringComparison.Ordinal)),
            "Expected removed matched rule.");

        return Task.CompletedTask;
    }

    private static Task TestAiAnalysisComparisonHtmlRendererIncludesRequiredSectionsAsync()
    {
        var leftBundle = CreateSampleAnalysisBundle(@"E:\comparison\left-result.json");
        var rightBundle = CreateComparisonVariantBundle(@"E:\comparison\right-result.json");
        var comparison = new AiAnalysisBundleComparisonBuilder().Build(leftBundle, rightBundle);
        var renderer = new AiAnalysisComparisonHtmlRenderer();
        var html = renderer.Render(comparison);

        AssertTrue(html.Contains("Analysis Bundle Comparison", StringComparison.Ordinal), "Expected comparison viewer title.");
        AssertTrue(html.Contains("Left Bundle", StringComparison.Ordinal), "Expected left bundle summary card.");
        AssertTrue(html.Contains("Right Bundle", StringComparison.Ordinal), "Expected right bundle summary card.");
        AssertTrue(html.Contains("Analysis Field Changes", StringComparison.Ordinal), "Expected analysis field change section.");
        AssertTrue(html.Contains("Summary Count Changes", StringComparison.Ordinal), "Expected summary count change section.");
        AssertTrue(html.Contains("Matched Rule Changes", StringComparison.Ordinal), "Expected matched rule change section.");
        AssertTrue(html.Contains("Evidence Changes", StringComparison.Ordinal), "Expected evidence change section.");
        AssertTrue(html.Contains("Recommended Action Changes", StringComparison.Ordinal), "Expected recommended action change section.");

        return Task.CompletedTask;
    }

    private static Task TestAiRegressionCheckerReturnsNoRegressionForSuccessToSuccessAsync()
    {
        var baselineBundle = CreateSuccessAnalysisBundle(@"E:\regression\baseline-result.json");
        var candidateBundle = CreateSuccessAnalysisBundle(@"E:\regression\candidate-result.json");
        var result = new AiRegressionChecker().Check(baselineBundle, candidateBundle);

        AssertEqual(AiRegressionStatus.NoRegression.ToString(), result.Status.ToString(), "Expected success-to-success to remain non-regressed.");
        AssertEqual("0", result.Findings.Count.ToString(), "Expected no regression findings.");

        return Task.CompletedTask;
    }

    private static Task TestAiRegressionCheckerDetectsSuccessToStepFailureRegressionAsync()
    {
        var baselineBundle = CreateSuccessAnalysisBundle(@"E:\regression\baseline-result.json");
        var candidateBundle = CreateStepFailureAnalysisBundle(@"E:\regression\candidate-result.json");
        var result = new AiRegressionChecker().Check(baselineBundle, candidateBundle);

        AssertEqual(AiRegressionStatus.RegressionDetected.ToString(), result.Status.ToString(), "Expected success-to-step-failure regression.");
        AssertTrue(
            result.Findings.Any(item => string.Equals(item.Code, "PrimaryCategoryWorsened", StringComparison.Ordinal)),
            "Expected primary category regression.");
        AssertTrue(
            result.Findings.Any(item => string.Equals(item.Code, "NewFailedStepNamesDetected", StringComparison.Ordinal)),
            "Expected new failed step regression.");
        AssertTrue(
            result.Findings.Any(item => string.Equals(item.Code, "MoreSevereMatchedRulesAppeared", StringComparison.Ordinal)),
            "Expected more-severe matched rule regression.");

        return Task.CompletedTask;
    }

    private static Task TestAiRegressionCheckerDetectsSuccessToVariableResolutionFailureRegressionAsync()
    {
        var baselineBundle = CreateSuccessAnalysisBundle(@"E:\regression\baseline-result.json");
        var candidateBundle = CreateSampleAnalysisBundle(@"E:\regression\candidate-result.json");
        var result = new AiRegressionChecker().Check(baselineBundle, candidateBundle);

        AssertEqual(AiRegressionStatus.RegressionDetected.ToString(), result.Status.ToString(), "Expected success-to-variable-resolution-failure regression.");
        AssertTrue(
            result.Findings.Any(item => string.Equals(item.Code, "VariableResolutionFailuresIncreased", StringComparison.Ordinal)),
            "Expected variable resolution failure regression.");
        AssertTrue(
            result.Findings.Any(item => string.Equals(item.Code, "PrimaryCategoryWorsened", StringComparison.Ordinal)),
            "Expected primary category regression.");

        return Task.CompletedTask;
    }

    private static Task TestAiRegressionCheckerDetectsSummaryCountRegressionAsync()
    {
        var baselineBundle = CreateSuccessAnalysisBundle(@"E:\regression\baseline-result.json");
        var candidateBundle = CreateSpecFailureAnalysisBundle(@"E:\regression\candidate-result.json");
        var result = new AiRegressionChecker().Check(baselineBundle, candidateBundle);

        AssertEqual(AiRegressionStatus.RegressionDetected.ToString(), result.Status.ToString(), "Expected summary count regression.");
        AssertTrue(
            result.Findings.Any(item => string.Equals(item.Code, "FailedSpecCountIncreased", StringComparison.Ordinal)),
            "Expected failed spec count regression.");

        return Task.CompletedTask;
    }

    private static Task TestAiRegressionHtmlRendererIncludesRequiredSectionsAsync()
    {
        var baselineBundle = CreateSuccessAnalysisBundle(@"E:\regression\baseline-result.json");
        var candidateBundle = CreateStepFailureAnalysisBundle(@"E:\regression\candidate-result.json");
        var result = new AiRegressionChecker().Check(baselineBundle, candidateBundle) with
        {
            BaselineBundlePath = @"E:\regression\baseline-bundle.json",
            CandidateBundlePath = @"E:\regression\candidate-bundle.json"
        };
        var renderer = new AiRegressionHtmlRenderer();
        var html = renderer.Render(result, baselineBundle, candidateBundle);

        AssertTrue(html.Contains("Regression Check Report", StringComparison.Ordinal), "Expected regression report title.");
        AssertTrue(html.Contains("Regression Status", StringComparison.Ordinal), "Expected regression status section.");
        AssertTrue(html.Contains("Findings", StringComparison.Ordinal), "Expected findings section.");
        AssertTrue(html.Contains("Compared Facts", StringComparison.Ordinal), "Expected compared facts section.");
        AssertTrue(html.Contains("Baseline Run", StringComparison.Ordinal), "Expected baseline run card.");
        AssertTrue(html.Contains("Candidate Run", StringComparison.Ordinal), "Expected candidate run card.");

        return Task.CompletedTask;
    }

    private static async Task TestFakeBundleAnalysisProviderGeneratesDeterministicResponseAsync()
    {
        var bundle = new AiAnalysisBundleBuilder().Build(
            new RunArtifactSummary
            {
                SourcePath = @"E:\summary\result.json",
                SessionId = "SESSION-PROVIDER-001",
                CommandName = "test run",
                RecipeName = "Provider Recipe",
                RunStatus = "Error"
            },
            new AiRunAnalysisResult
            {
                AnalyzerName = "RuleBasedRunAnalyzer",
                PrimaryCategory = "Configuration",
                PrimaryCause = "Variable resolution failed before the run could complete.",
                Summary = "Run status=Error, steps=1, failedSpecs=0, errors=1, variableFailures=1, exceptions=0, warnings=0.",
                Evidence = new List<AiEvidenceItem>
                {
                    new()
                    {
                        Type = "Metric",
                        Message = "Variable resolution failure count",
                        Source = "RunArtifactSummary.VariableResolutionFailedCount",
                        Value = "1"
                    }
                },
                MatchedRules = new List<string>
                {
                    "VariableResolutionFailureRule"
                },
                RecommendedActions = new List<string>
                {
                    "Verify the required value exists in Step, DUT, or Global inputs before rerunning."
                }
            },
            @"E:\summary\result.json");

        var provider = new FakeBundleAnalysisProvider();
        var response = await provider.AnalyzeAsync(
            new AiProviderRequest
            {
                Bundle = bundle
            },
            CancellationToken.None);

        AssertEqual("fake", response.ProviderName, "Expected fake provider name.");
        AssertEqual("ats.ai-analysis-bundle.v1", response.BundleSchemaVersion, "Expected bundle schema version to be echoed.");
        AssertEqual("Configuration", response.PrimaryCategory, "Expected provider response category.");
        AssertTrue(
            response.Summary.Contains("matchedRules=1", StringComparison.Ordinal),
            "Expected provider summary to include matched rule count.");
        AssertTrue(
            response.Highlights.Any(item =>
                item.Contains("SESSION-PROVIDER-001", StringComparison.Ordinal)),
            "Expected provider highlights to mention session id.");
    }

    private static async Task TestFakeBundleAnalysisProviderStaysConsistentWithBundleAnalysisAsync()
    {
        var bundle = new AiAnalysisBundleBuilder().Build(
            new RunArtifactSummary
            {
                SourcePath = @"E:\summary\result.json",
                SessionId = "SESSION-PROVIDER-002",
                CommandName = "test run",
                RecipeName = "Provider Consistency Recipe",
                RunStatus = "Passed"
            },
            new AiRunAnalysisResult
            {
                AnalyzerName = "RuleBasedRunAnalyzer",
                PrimaryCategory = "Success",
                PrimaryCause = "The run completed successfully without detected failures.",
                Summary = "Run status=Passed, steps=2, failedSpecs=0, errors=0, variableFailures=0, exceptions=0, warnings=0.",
                Evidence = new List<AiEvidenceItem>
                {
                    new()
                    {
                        Type = "Status",
                        Message = "Run status",
                        Source = "RunArtifactSummary.RunStatus",
                        Value = "Passed"
                    }
                },
                MatchedRules = new List<string>
                {
                    "SuccessRule"
                },
                RecommendedActions = new List<string>
                {
                    "Archive the run artifacts if this session should be retained as a known-good baseline."
                }
            },
            @"E:\summary\result.json");

        var provider = new FakeBundleAnalysisProvider();
        var response = await provider.AnalyzeAsync(
            new AiProviderRequest
            {
                Bundle = bundle
            },
            CancellationToken.None);

        AssertEqual(bundle.Analysis.PrimaryCategory, response.PrimaryCategory, "Expected provider category to match bundle analysis.");
        AssertEqual(bundle.Analysis.PrimaryCause, response.PrimaryCause, "Expected provider cause to match bundle analysis.");
        AssertEqual(bundle.Analysis.RecommendedActions[0], response.RecommendedActions[0], "Expected provider action to reuse analysis recommendations.");
        AssertTrue(
            response.Highlights.Any(item =>
                item.Contains("SuccessRule", StringComparison.Ordinal)),
            "Expected provider highlights to reflect matched rules from the bundle.");
    }

    private static async Task TestRunAnalysisServiceAnalyzesResultJsonArtifactsAsync()
    {
        var repositoryRoot = FindRepositoryRoot();
        var recipePath = Path.Combine(repositoryRoot, "samples", "recipes", "demo.recipe.json");
        var outputDirectory = CreateOutputDirectory("ai-analyze");
        var simulationService = new TestSimulationService();
        var simulationResult = await simulationService.RunAsync(
            recipePath,
            outputDirectory,
            null,
            new RunInputModel
            {
                SerialNumber = "SN-AI-001",
                Station = "ST-AI"
            },
            CancellationToken.None);

        var service = new RunAnalysisService();
        var analysis = await service.AnalyzeAsync(simulationResult.ResultJsonPath, CancellationToken.None);

        AssertEqual("RuleBasedRunAnalyzer", analysis.AnalyzerName, "Expected rule-based analyzer to be used by default.");
        AssertEqual("Success", analysis.PrimaryCategory, "Expected passed result artifact to classify as success.");
        AssertTrue(
            analysis.Summary.Contains("Run status=Passed", StringComparison.Ordinal),
            "Expected analysis summary to mention the passed run status.");
        AssertTrue(
            analysis.Observations.Any(item =>
                string.Equals(item.Title, "Run Passed", StringComparison.Ordinal)),
            "Expected passed run observation.");
        AssertTrue(
            analysis.Evidence.Count > 0,
            "Expected analysis result to contain structured evidence.");
    }

    private static async Task TestRunAnalysisServiceAnalyzesResultAndEventsArtifactsAsync()
    {
        var outputDirectory = CreateOutputDirectory("ai-analyze-events");
        var resultPath = WriteTestResultJson(
            outputDirectory,
            "result.json",
            new TestResult
            {
                SessionId = "SESSION-EVENTS-001",
                CommandName = "test run",
                RecipeName = "Event Aware Recipe",
                DeviceName = "FakeDevice",
                Status = "Passed",
                ResultJsonPath = Path.Combine(outputDirectory, "result.json"),
                ResultCsvPath = Path.Combine(outputDirectory, "result.csv"),
                SessionLogPath = Path.Combine(outputDirectory, "session.log"),
                StructuredLogPath = Path.Combine(outputDirectory, "session.events.jsonl"),
                RunInput = new RunInputModel
                {
                    SerialNumber = "SN-EVT-001",
                    Station = "ST-EVT",
                    Mode = "RUN"
                },
                StartedAtUtc = DateTimeOffset.UtcNow.AddSeconds(-2),
                CompletedAtUtc = DateTimeOffset.UtcNow,
                Steps = new List<StepResult>
                {
                    new()
                    {
                        StepName = "ReadVoltage",
                        FinalStatus = "Passed",
                        Measurements = new List<MeasurementItem>
                        {
                            new()
                            {
                                Key = "voltage",
                                FullKey = "voltage",
                                Value = "12.3",
                                ValueType = MeasurementValueType.Number,
                                Unit = "V"
                            }
                        },
                        SpecResults = new List<SpecEvaluationResult>
                        {
                            new()
                            {
                                RuleName = "voltage",
                                TargetKey = "voltage",
                                RuleType = "Range",
                                PassFail = "Passed"
                            }
                        }
                    }
                }
            });
        var eventsPath = WriteStructuredLogJsonLines(
            outputDirectory,
            "session.events.jsonl",
            new[]
            {
                CreateStructuredLogEntry(1, StructuredLogEntryType.VariableResolved, "INFO", "Variable 'SN' resolved.", "ReadVoltage"),
                CreateStructuredLogEntry(2, StructuredLogEntryType.Message, "WARNING", "Review low margin on voltage.", "ReadVoltage")
            });

        var service = new RunAnalysisService();
        var analysis = await service.AnalyzeAsync(resultPath, eventsPath, CancellationToken.None);

        AssertTrue(
            analysis.Summary.Contains("warnings=1", StringComparison.Ordinal),
            "Expected event-aware summary to include warning count.");
        AssertTrue(
            analysis.Summary.Contains("variableFailures=0", StringComparison.Ordinal),
            "Expected event-aware summary to include variable failure count.");
        AssertTrue(
            analysis.Observations.Any(item =>
                string.Equals(item.Title, "Structured Warning Events", StringComparison.Ordinal)),
            "Expected warning-level event observation.");
        AssertEqual("Success", analysis.PrimaryCategory, "Expected warning-only event set to keep success classification.");
    }

    private static async Task TestRunAnalysisServiceFlagsVariableResolutionFailuresFromEventsAsync()
    {
        var outputDirectory = CreateOutputDirectory("ai-analyze-var-failure");
        var resultPath = WriteTestResultJson(
            outputDirectory,
            "result.json",
            new TestResult
            {
                SessionId = "SESSION-VAR-FAIL-001",
                CommandName = "test run",
                RecipeName = "Variable Failure Recipe",
                DeviceName = "FakeDevice",
                Status = "Error",
                ResultJsonPath = Path.Combine(outputDirectory, "result.json"),
                ResultCsvPath = Path.Combine(outputDirectory, "result.csv"),
                SessionLogPath = Path.Combine(outputDirectory, "session.log"),
                StructuredLogPath = Path.Combine(outputDirectory, "session.events.jsonl"),
                StartedAtUtc = DateTimeOffset.UtcNow.AddSeconds(-2),
                CompletedAtUtc = DateTimeOffset.UtcNow,
                Steps = new List<StepResult>
                {
                    new()
                    {
                        StepName = "ReadDut",
                        FinalStatus = "Error"
                    }
                },
                Errors = new List<string>
                {
                    "Variable resolution failed."
                }
            });
        var eventsPath = WriteStructuredLogJsonLines(
            outputDirectory,
            "session.events.jsonl",
            new[]
            {
                CreateStructuredLogEntry(
                    1,
                    StructuredLogEntryType.VariableResolutionFailed,
                    "ERROR",
                    "Variable 'dut.sn' required by field 'Command' in step 'ReadDut' was not found in DutContext.",
                    "ReadDut",
                    status: "Failed"),
                CreateStructuredLogEntry(
                    2,
                    StructuredLogEntryType.StepCompleted,
                    "ERROR",
                    "Step 'ReadDut' completed with status 'Error'.",
                    "ReadDut",
                    status: "Error")
            });

        var service = new RunAnalysisService();
        var analysis = await service.AnalyzeAsync(resultPath, eventsPath, CancellationToken.None);

        AssertTrue(
            analysis.Summary.Contains("variableFailures=1", StringComparison.Ordinal),
            "Expected summary to include variable resolution failure count.");
        AssertEqual("Configuration", analysis.PrimaryCategory, "Expected variable resolution event path to classify as configuration.");
        AssertTrue(
            analysis.RecommendedActions.Any(item =>
                item.Contains("verify the required value exists in Step, DUT, or Global inputs", StringComparison.OrdinalIgnoreCase)),
            "Expected variable resolution recommendation.");
    }

    private static async Task TestRunAnalysisServiceFlagsUnhandledExceptionsFromEventsAsync()
    {
        var outputDirectory = CreateOutputDirectory("ai-analyze-exception");
        var resultPath = WriteTestResultJson(
            outputDirectory,
            "result.json",
            new TestResult
            {
                SessionId = "SESSION-EXCEPTION-001",
                CommandName = "test run",
                RecipeName = "Exception Recipe",
                DeviceName = "FakeDevice",
                Status = "Error",
                ResultJsonPath = Path.Combine(outputDirectory, "result.json"),
                ResultCsvPath = Path.Combine(outputDirectory, "result.csv"),
                SessionLogPath = Path.Combine(outputDirectory, "session.log"),
                StructuredLogPath = Path.Combine(outputDirectory, "session.events.jsonl"),
                StartedAtUtc = DateTimeOffset.UtcNow.AddSeconds(-2),
                CompletedAtUtc = DateTimeOffset.UtcNow,
                Steps = new List<StepResult>(),
                Errors = new List<string>
                {
                    "Unhandled exception during run."
                }
            });
        var eventsPath = WriteStructuredLogJsonLines(
            outputDirectory,
            "session.events.jsonl",
            new[]
            {
                CreateStructuredLogEntry(
                    1,
                    StructuredLogEntryType.Error,
                    "ERROR",
                    "Unhandled exception: FakeDevice connection dropped.",
                    "test run",
                    data: new Dictionary<string, object?>
                    {
                        ["exceptionType"] = "InvalidOperationException"
                    })
            });

        var service = new RunAnalysisService();
        var analysis = await service.AnalyzeAsync(resultPath, eventsPath, CancellationToken.None);

        AssertTrue(
            analysis.Summary.Contains("exceptions=1", StringComparison.Ordinal),
            "Expected summary to include exception count.");
        AssertEqual("Runtime", analysis.PrimaryCategory, "Expected exception event path to classify as runtime.");
        AssertTrue(
            analysis.RecommendedActions.Any(item =>
                item.Contains("Inspect session.log", StringComparison.Ordinal)),
            "Expected exception recommendation.");
    }

    private static Task TestArtifactSummaryBuilderMergesFailedStepNamesFromEventsAsync()
    {
        var outputDirectory = CreateOutputDirectory("ai-failed-step-events");
        var eventsPath = WriteStructuredLogJsonLines(
            outputDirectory,
            "session.events.jsonl",
            new[]
            {
                CreateStructuredLogEntry(
                    1,
                    StructuredLogEntryType.StepCompleted,
                    "ERROR",
                    "Step 'MeasureResistance' completed with status 'Failed'.",
                    "MeasureResistance",
                    status: "Failed")
            });

        var builder = new ArtifactSummaryBuilder();
        var summary = builder.Build(
            new TestResult
            {
                SessionId = "SESSION-FAILED-STEP-001",
                CommandName = "test run",
                RecipeName = "Failed Step Recipe",
                DeviceName = "FakeDevice",
                Status = "Failed",
                ResultJsonPath = Path.Combine(outputDirectory, "result.json"),
                ResultCsvPath = Path.Combine(outputDirectory, "result.csv"),
                SessionLogPath = Path.Combine(outputDirectory, "session.log"),
                StructuredLogPath = Path.Combine(outputDirectory, "session.events.jsonl"),
                StartedAtUtc = DateTimeOffset.UtcNow.AddSeconds(-2),
                CompletedAtUtc = DateTimeOffset.UtcNow
            },
            Path.Combine(outputDirectory, "result.json"),
            eventsPath);

        AssertTrue(
            summary.FailedStepNames.Contains("MeasureResistance", StringComparer.Ordinal),
            "Expected failed step names to merge in event-derived failed steps.");
        AssertEqual(
            "Step 'MeasureResistance' completed with status 'Failed'.",
            summary.FirstFailureMessage,
            "Expected first failure message to come from events.");

        return Task.CompletedTask;
    }

    private static async Task TestCliAiAnalyzeWritesOutputJsonArtifactAsync()
    {
        var repositoryRoot = FindRepositoryRoot();
        var recipePath = Path.Combine(repositoryRoot, "samples", "recipes", "demo.recipe.json");
        var outputDirectory = CreateOutputDirectory("ai-output-json");
        var simulationService = new TestSimulationService();
        var simulationResult = await simulationService.RunAsync(recipePath, outputDirectory, null, null, CancellationToken.None);
        var analysisOutputPath = Path.Combine(outputDirectory, "analysis", "analysis.json");
        var bundleOutputPath = Path.Combine(outputDirectory, "analysis", "analysis-bundle.json");

        var exitCode = await CliProgram.RunAsync(
            new[]
            {
                "ai",
                "analyze",
                "--result-json",
                simulationResult.ResultJsonPath,
                "--output-json",
                analysisOutputPath
            });

        AssertEqual("0", exitCode.ToString(), "Expected ai analyze CLI command to succeed.");
        AssertTrue(File.Exists(analysisOutputPath), "Expected analysis output json file to be written.");
        AssertTrue(!File.Exists(bundleOutputPath), "Expected bundle output to remain absent when not requested.");

        var json = File.ReadAllText(analysisOutputPath);
        var analysis = JsonSerializer.Deserialize<AiRunAnalysisResult>(json, CreateJsonOptions());

        AssertTrue(analysis is not null, "Expected analysis output json to deserialize.");
        AssertEqual("RuleBasedRunAnalyzer", analysis!.AnalyzerName, "Expected exported analysis to contain analyzer name.");
        AssertTrue(analysis.Evidence.Count > 0, "Expected exported analysis to contain evidence.");
    }

    private static async Task TestCliAiAnalyzeWritesBundleJsonArtifactAsync()
    {
        var repositoryRoot = FindRepositoryRoot();
        var recipePath = Path.Combine(repositoryRoot, "samples", "recipes", "demo.recipe.json");
        var outputDirectory = CreateOutputDirectory("ai-output-bundle-json");
        var simulationService = new TestSimulationService();
        var simulationResult = await simulationService.RunAsync(recipePath, outputDirectory, null, null, CancellationToken.None);
        var analysisOutputPath = Path.Combine(outputDirectory, "analysis", "analysis.json");
        var bundleOutputPath = Path.Combine(outputDirectory, "analysis", "analysis-bundle.json");

        var exitCode = await CliProgram.RunAsync(
            new[]
            {
                "ai",
                "analyze",
                "--result-json",
                simulationResult.ResultJsonPath,
                "--events-jsonl",
                simulationResult.StructuredLogPath,
                "--output-json",
                analysisOutputPath,
                "--output-bundle-json",
                bundleOutputPath
            });

        AssertEqual("0", exitCode.ToString(), "Expected ai analyze bundle CLI command to succeed.");
        AssertTrue(File.Exists(bundleOutputPath), "Expected bundle output json file to be written.");

        var json = File.ReadAllText(bundleOutputPath);
        var bundle = JsonSerializer.Deserialize<AiAnalysisBundle>(json, CreateJsonOptions());

        AssertTrue(bundle is not null, "Expected bundle output json to deserialize.");
        AssertEqual("ats.ai-analysis-bundle.v1", bundle!.SchemaVersion, "Expected bundle schema version.");
        AssertTrue(bundle.GeneratedAtUtc > DateTimeOffset.MinValue, "Expected bundle generated timestamp.");
        AssertEqual("RuleBasedRunAnalyzer", bundle.AnalyzerName, "Expected bundle analyzer metadata.");
        AssertEqual(Path.GetFullPath(simulationResult.ResultJsonPath), bundle.ResultJsonPath, "Expected bundle result path metadata.");
        AssertEqual(Path.GetFullPath(simulationResult.StructuredLogPath), bundle.EventsJsonlPath, "Expected bundle events path metadata.");
        AssertEqual(Path.GetFullPath(analysisOutputPath), bundle.AnalysisJsonPath, "Expected bundle analysis path metadata.");
        AssertTrue(bundle.Summary.StepCount > 0, "Expected bundle to include normalized summary.");
        AssertTrue(bundle.Analysis.Evidence.Count > 0, "Expected bundle to include analysis evidence.");
    }

    private static async Task TestCliAiRenderWritesHtmlArtifactAsync()
    {
        var outputDirectory = CreateOutputDirectory("ai-render-html");
        var bundleOutputPath = Path.Combine(outputDirectory, "analysis-bundle.json");
        var htmlOutputPath = Path.Combine(outputDirectory, "viewer", "analysis-viewer.html");
        var bundle = CreateSampleAnalysisBundle(Path.Combine(outputDirectory, "result.json"));
        var bundleWriter = new AiAnalysisBundleWriter();
        await bundleWriter.WriteAsync(bundle, bundleOutputPath, CancellationToken.None);

        var exitCode = await CliProgram.RunAsync(
            new[]
            {
                "ai",
                "render",
                "--bundle-json",
                bundleOutputPath,
                "--output-html",
                htmlOutputPath
            });

        AssertEqual("0", exitCode.ToString(), "Expected ai render CLI command to succeed.");
        AssertTrue(File.Exists(htmlOutputPath), "Expected html viewer artifact to be written.");

        var html = File.ReadAllText(htmlOutputPath);
        AssertTrue(html.Contains("Analysis Bundle Viewer", StringComparison.Ordinal), "Expected viewer title in html output.");
        AssertTrue(html.Contains("Normalized Summary Facts", StringComparison.Ordinal), "Expected normalized summary section in html output.");
        AssertTrue(html.Contains("Variable resolution failed before the run could complete.", StringComparison.Ordinal), "Expected primary cause in html output.");
    }

    private static async Task TestCliAiCompareWritesHtmlArtifactAsync()
    {
        var outputDirectory = CreateOutputDirectory("ai-compare-html");
        var leftBundleOutputPath = Path.Combine(outputDirectory, "left", "analysis-bundle.json");
        var rightBundleOutputPath = Path.Combine(outputDirectory, "right", "analysis-bundle.json");
        var htmlOutputPath = Path.Combine(outputDirectory, "compare", "analysis-compare.html");
        var leftBundle = CreateSampleAnalysisBundle(Path.Combine(outputDirectory, "left", "result.json"));
        var rightBundle = CreateComparisonVariantBundle(Path.Combine(outputDirectory, "right", "result.json"));
        var bundleWriter = new AiAnalysisBundleWriter();
        await bundleWriter.WriteAsync(leftBundle, leftBundleOutputPath, CancellationToken.None);
        await bundleWriter.WriteAsync(rightBundle, rightBundleOutputPath, CancellationToken.None);

        var exitCode = await CliProgram.RunAsync(
            new[]
            {
                "ai",
                "compare",
                "--left-bundle",
                leftBundleOutputPath,
                "--right-bundle",
                rightBundleOutputPath,
                "--output-html",
                htmlOutputPath
            });

        AssertEqual("0", exitCode.ToString(), "Expected ai compare CLI command to succeed.");
        AssertTrue(File.Exists(htmlOutputPath), "Expected comparison html artifact to be written.");

        var html = File.ReadAllText(htmlOutputPath);
        AssertTrue(html.Contains("Analysis Bundle Comparison", StringComparison.Ordinal), "Expected comparison viewer title.");
        AssertTrue(html.Contains("Configuration", StringComparison.Ordinal), "Expected left-side category in html output.");
        AssertTrue(html.Contains("Runtime", StringComparison.Ordinal), "Expected right-side category in html output.");
        AssertTrue(html.Contains("Summary Count Changes", StringComparison.Ordinal), "Expected summary count change section in html output.");
    }

    private static async Task TestCliAiRegressWritesJsonAndHtmlArtifactsAsync()
    {
        var outputDirectory = CreateOutputDirectory("ai-regress");
        var baselineBundleOutputPath = Path.Combine(outputDirectory, "baseline", "analysis-bundle.json");
        var candidateBundleOutputPath = Path.Combine(outputDirectory, "candidate", "analysis-bundle.json");
        var regressionJsonOutputPath = Path.Combine(outputDirectory, "regression", "regression.json");
        var regressionHtmlOutputPath = Path.Combine(outputDirectory, "regression", "regression.html");
        var bundleWriter = new AiAnalysisBundleWriter();
        await bundleWriter.WriteAsync(
            CreateSuccessAnalysisBundle(Path.Combine(outputDirectory, "baseline", "result.json")),
            baselineBundleOutputPath,
            CancellationToken.None);
        await bundleWriter.WriteAsync(
            CreateStepFailureAnalysisBundle(Path.Combine(outputDirectory, "candidate", "result.json")),
            candidateBundleOutputPath,
            CancellationToken.None);

        var exitCode = await CliProgram.RunAsync(
            new[]
            {
                "ai",
                "regress",
                "--baseline-bundle",
                baselineBundleOutputPath,
                "--candidate-bundle",
                candidateBundleOutputPath,
                "--output-json",
                regressionJsonOutputPath,
                "--output-html",
                regressionHtmlOutputPath
            });

        AssertEqual("0", exitCode.ToString(), "Expected ai regress CLI command to succeed.");
        AssertTrue(File.Exists(regressionJsonOutputPath), "Expected regression json artifact.");
        AssertTrue(File.Exists(regressionHtmlOutputPath), "Expected regression html artifact.");

        var json = File.ReadAllText(regressionJsonOutputPath);
        var result = JsonSerializer.Deserialize<AiRegressionCheckResult>(json, CreateJsonOptions());
        AssertTrue(result is not null, "Expected regression json output to deserialize.");
        AssertEqual(AiRegressionStatus.RegressionDetected.ToString(), result!.Status.ToString(), "Expected regression status in json output.");
        AssertTrue(result.Findings.Count > 0, "Expected regression findings in json output.");

        var html = File.ReadAllText(regressionHtmlOutputPath);
        AssertTrue(html.Contains("Regression Check Report", StringComparison.Ordinal), "Expected regression html title.");
        AssertTrue(html.Contains("Findings", StringComparison.Ordinal), "Expected findings section in regression html.");
    }

    private static async Task TestCliAiAnalyzeSupportsFakeProviderPathAsync()
    {
        var repositoryRoot = FindRepositoryRoot();
        var recipePath = Path.Combine(repositoryRoot, "samples", "recipes", "demo.recipe.json");
        var outputDirectory = CreateOutputDirectory("ai-provider-fake");
        var simulationService = new TestSimulationService();
        var simulationResult = await simulationService.RunAsync(recipePath, outputDirectory, null, null, CancellationToken.None);

        var output = await CaptureStandardOutputAsync(async () =>
        {
            var exitCode = await CliProgram.RunAsync(
                new[]
                {
                    "ai",
                    "analyze",
                    "--result-json",
                    simulationResult.ResultJsonPath,
                    "--provider",
                    "fake"
                });

            AssertEqual("0", exitCode.ToString(), "Expected ai analyze provider CLI command to succeed.");
        });

        AssertTrue(
            output.Contains("Provider: fake", StringComparison.Ordinal),
            "Expected provider section in CLI output.");
        AssertTrue(
            output.Contains("Provider Summary:", StringComparison.Ordinal),
            "Expected provider summary in CLI output.");
    }

    private static async Task TestCliAiAnalyzeRemainsBackwardCompatibleWithoutProviderAsync()
    {
        var repositoryRoot = FindRepositoryRoot();
        var recipePath = Path.Combine(repositoryRoot, "samples", "recipes", "demo.recipe.json");
        var outputDirectory = CreateOutputDirectory("ai-provider-backcompat");
        var simulationService = new TestSimulationService();
        var simulationResult = await simulationService.RunAsync(recipePath, outputDirectory, null, null, CancellationToken.None);

        var output = await CaptureStandardOutputAsync(async () =>
        {
            var exitCode = await CliProgram.RunAsync(
                new[]
                {
                    "ai",
                    "analyze",
                    "--result-json",
                    simulationResult.ResultJsonPath
                });

            AssertEqual("0", exitCode.ToString(), "Expected ai analyze CLI command without provider to succeed.");
        });

        AssertTrue(
            !output.Contains("Provider:", StringComparison.Ordinal),
            "Expected provider section to remain absent when --provider is not specified.");
    }

    private static async Task TestCliAiAnalyzeRemainsBackwardCompatibleAfterAiRenderAdditionAsync()
    {
        var repositoryRoot = FindRepositoryRoot();
        var recipePath = Path.Combine(repositoryRoot, "samples", "recipes", "demo.recipe.json");
        var outputDirectory = CreateOutputDirectory("ai-render-backcompat");
        var simulationService = new TestSimulationService();
        var simulationResult = await simulationService.RunAsync(recipePath, outputDirectory, null, null, CancellationToken.None);
        var analysisOutputPath = Path.Combine(outputDirectory, "analysis", "analysis.json");
        var htmlOutputPath = Path.Combine(outputDirectory, "analysis", "analysis-viewer.html");

        var exitCode = await CliProgram.RunAsync(
            new[]
            {
                "ai",
                "analyze",
                "--result-json",
                simulationResult.ResultJsonPath,
                "--output-json",
                analysisOutputPath
            });

        AssertEqual("0", exitCode.ToString(), "Expected ai analyze CLI command to remain compatible after adding ai render.");
        AssertTrue(File.Exists(analysisOutputPath), "Expected analysis json artifact.");
        AssertTrue(!File.Exists(htmlOutputPath), "Expected ai analyze to avoid generating html when ai render was not requested.");
    }

    private static Task TestVariableResolverResolvesDutContextVariablesAsync()
    {
        var resolver = new VariableResolver();
        var variableContext = new VariableContext
        {
            GlobalVariables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["dut.sn"] = "GLOBAL-OVERRIDE"
            },
            StepVariables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["dut.sn"] = "STEP-OVERRIDE"
            },
            DutContext = new DutContext
            {
                Id = "DUT-01",
                Index = 2,
                SerialNumber = "SN-DUT-01",
                Station = "ST-01",
                Slot = "SLOT-A",
                IsSimulated = true
            }
        };

        AssertEqual(
            "READ_SN-DUT-01_ST-01_SLOT-A_2_true",
            resolver.ResolveTemplate(
                "READ_${dut.sn}_${dut.station}_${dut.slot}_${dut.index}_${dut.isSimulated}",
                variableContext,
                "Command",
                "ResolverStep"),
            "Expected canonical dut.* variables to resolve from DutContext.");

        try
        {
            resolver.ResolveTemplate(
                "READ_${dut.id}",
                new VariableContext
                {
                    GlobalVariables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["dut.id"] = "GLOBAL-ID"
                    },
                    StepVariables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["dut.id"] = "STEP-ID"
                    }
                },
                "Command",
                "ResolverStep");
            throw new InvalidOperationException("Expected dut.id to fail when DutContext does not provide it.");
        }
        catch (InvalidOperationException exception)
        {
            AssertTrue(
                exception.Message.Contains("DutContext", StringComparison.Ordinal),
                "Expected missing dut.* resolution to point at DutContext.");
        }

        return Task.CompletedTask;
    }

    private static Task TestVariableResolverThrowsWhenVariableIsMissingAsync()
    {
        var resolver = new VariableResolver();
        var variableContext = new VariableContext();

        try
        {
            resolver.ResolveTemplate("RUN_${MissingValue}", variableContext, "Command", "ResolverStep");
            throw new InvalidOperationException("Expected missing variable resolution to throw.");
        }
        catch (InvalidOperationException exception)
        {
            AssertTrue(
                exception.Message.Contains("MissingValue", StringComparison.Ordinal),
                "Expected error message to include missing variable name.");
            AssertTrue(
                exception.Message.Contains("Step > Dut > Global", StringComparison.Ordinal),
                "Expected error message to include the fixed search order.");
        }

        return Task.CompletedTask;
    }

    private static async Task TestRunnerResolvesVariableTemplatesAsync()
    {
        var outputDirectory = CreateOutputDirectory("variable-resolution");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "variable-resolution.recipe.json",
            """
            {
              "name": "Variable Resolution Recipe",
              "variables": {
                "SharedCommand": "GLOBAL",
                "VoltagePath": "payload.globalVoltage",
                "CurrentPath": "payload.globalCurrent",
                "VoltageValue": "12.3",
                "CurrentValue": "1.4",
                "VoltageUnit": "V",
                "VoltageDescription": "GLOBAL-DESC"
              },
              "scripts": [
                {
                  "name": "ReadSnapshot",
                  "prefix": "battery",
                  "command": "READ_${SN}_${SharedCommand}",
                  "variables": {
                    "SharedCommand": "STEP",
                    "VoltagePath": "payload.stepVoltage",
                    "VoltageDescription": "STEP-DESC"
                  },
                  "measurements": [
                    {
                      "key": "voltage",
                      "sourcePath": "${VoltagePath}",
                      "unit": "${VoltageUnit}",
                      "description": "${VoltageDescription}"
                    },
                    {
                      "key": "current",
                      "sourcePath": "${CurrentPath}",
                      "unit": "A",
                      "description": "Current for ${SN}"
                    }
                  ],
                  "simulatedResponse": "{\"payload\":{\"stepVoltage\":\"${VoltageValue}\",\"globalCurrent\":\"${CurrentValue}\",\"serial\":\"${SN}\"}}"
                }
              ],
              "rules": [
                {
                  "name": "Battery Voltage Range",
                  "targetKey": "battery.voltage",
                  "ruleType": "Range",
                  "min": 11.5,
                  "max": 12.8
                },
                {
                  "name": "Battery Current Range",
                  "targetKey": "battery.current",
                  "ruleType": "Range",
                  "min": 1.0,
                  "max": 2.0
                }
              ]
            }
            """);

        var runner = new TestRunner();
        var result = await runner.RunAsync(
            new TestRunRequest(
                "test run",
                recipePath,
                string.Empty,
                outputDirectory,
                string.Empty,
                null,
                new RunInputModel
                {
                    SerialNumber = "SN-VAR-001",
                    Station = "ST-VAR",
                    Mode = "RUN"
                }),
            CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected variable-based recipe to pass.");
        AssertEqual("1", result.Steps.Count.ToString(), "Expected one resolved step.");
        AssertEqual("READ_SN-VAR-001_STEP", result.Steps[0].Command, "Expected resolved command to use step override and global SN.");
        AssertTrue(
            result.Steps[0].MeasurementSet.RawPayload.Contains("SN-VAR-001", StringComparison.Ordinal),
            "Expected simulated response to resolve SN into the raw payload.");

        var voltage = result.Steps[0].Measurements.Single(item => item.FullKey == "battery.voltage");
        var current = result.Steps[0].Measurements.Single(item => item.FullKey == "battery.current");
        AssertEqual("12.3", voltage.Value, "Expected voltage to resolve through step-scoped sourcePath.");
        AssertEqual("STEP-DESC", voltage.Description, "Expected step-scoped description override.");
        AssertEqual("1.4", current.Value, "Expected current to resolve through global sourcePath.");
        AssertEqual("Current for SN-VAR-001", current.Description, "Expected global SN to resolve in measurement description.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestStructuredLogCapturesVariableResolutionEventsAsync()
    {
        var outputDirectory = CreateOutputDirectory("variable-structured-log");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "variable-structured-log.recipe.json",
            """
            {
              "name": "Variable Structured Log Recipe",
              "variables": {
                "CommandSuffix": "GLOBAL",
                "VoltagePath": "payload.globalVoltage",
                "VoltageValue": "12.1"
              },
              "scripts": [
                {
                  "name": "ReadSnapshot",
                  "prefix": "battery",
                  "command": "SNAP_${CommandSuffix}_${dut.sn}_${dut.isSimulated}",
                  "variables": {
                    "CommandSuffix": "STEP",
                    "VoltagePath": "payload.stepVoltage"
                  },
                  "measurements": [
                    {
                      "key": "voltage",
                      "sourcePath": "${VoltagePath}",
                      "unit": "V"
                    }
                  ],
                  "simulatedResponse": "{\"payload\":{\"stepVoltage\":\"${VoltageValue}\"}}"
                }
              ],
              "rules": [
                {
                  "name": "Battery Voltage Range",
                  "targetKey": "battery.voltage",
                  "ruleType": "Range",
                  "min": 11.5,
                  "max": 12.8
                }
              ]
            }
            """);

        var runner = new TestRunner();
        var result = await runner.RunAsync(
            new TestRunRequest(
                "test run",
                recipePath,
                string.Empty,
                outputDirectory,
                string.Empty,
                null,
                new RunInputModel
                {
                    SerialNumber = "SN-TRACE-01",
                    Mode = "RUN"
                }),
            CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected structured log variable sample to pass.");
        var entries = ReadStructuredLogEntries(result.StructuredLogPath);
        var variableEvents = entries
            .Where(item => item.EntryType == StructuredLogEntryType.VariableResolved)
            .ToList();

        AssertTrue(variableEvents.Count >= 4, "Expected variable resolution events for command, sourcePath, and simulated response.");
        AssertTrue(
            variableEvents.Any(item =>
                item.Data.TryGetValue("fieldName", out var fieldName) &&
                string.Equals(fieldName?.ToString(), "Command", StringComparison.Ordinal) &&
                item.Data.TryGetValue("variableName", out var variableName) &&
                string.Equals(variableName?.ToString(), "CommandSuffix", StringComparison.Ordinal) &&
                item.Data.TryGetValue("scope", out var scope) &&
                string.Equals(scope?.ToString(), "Step", StringComparison.Ordinal)),
            "Expected command variable to resolve from step scope.");
        AssertTrue(
            variableEvents.Any(item =>
                item.Data.TryGetValue("fieldName", out var fieldName) &&
                string.Equals(fieldName?.ToString(), "Command", StringComparison.Ordinal) &&
                item.Data.TryGetValue("requestedName", out var requestedName) &&
                string.Equals(requestedName?.ToString(), "dut.sn", StringComparison.Ordinal) &&
                item.Data.TryGetValue("resolvedName", out var resolvedName) &&
                string.Equals(resolvedName?.ToString(), "dut.sn", StringComparison.Ordinal) &&
                item.Data.TryGetValue("scope", out var scope) &&
                string.Equals(scope?.ToString(), "Dut", StringComparison.Ordinal) &&
                item.Data.TryGetValue("source", out var source) &&
                string.Equals(source?.ToString(), "DutContext.SerialNumber", StringComparison.Ordinal) &&
                item.Data.TryGetValue("value", out var value) &&
                string.Equals(value?.ToString(), "SN-TRACE-01", StringComparison.Ordinal)),
            "Expected dut.sn to resolve with canonical structured log data.");
        AssertTrue(
            variableEvents.Any(item =>
                item.Data.TryGetValue("fieldName", out var fieldName) &&
                string.Equals(fieldName?.ToString(), "Measurements[voltage].SourcePath", StringComparison.Ordinal) &&
                item.Data.TryGetValue("variableName", out var variableName) &&
                string.Equals(variableName?.ToString(), "VoltagePath", StringComparison.Ordinal) &&
                item.Data.TryGetValue("scope", out var scope) &&
                string.Equals(scope?.ToString(), "Step", StringComparison.Ordinal)),
            "Expected measurement sourcePath to resolve from step scope.");
    }

    private static async Task TestRecipeValidationAllowsRuntimeProvidedDutVariablesAsync()
    {
        var outputDirectory = CreateOutputDirectory("variable-validate-dut");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "variable-validate-dut.recipe.json",
            """
            {
              "name": "DUT Variable Validation Recipe",
              "scripts": [
                {
                  "name": "ReadDut",
                  "command": "READ_${dut.sn}",
                  "measurements": [
                    {
                      "key": "voltage",
                      "sourcePath": "payload.voltage",
                      "unit": "V",
                      "description": "Station ${dut.station}"
                    }
                  ],
                  "simulatedResponse": "{\"payload\":{\"voltage\":\"12.3\"}}"
                }
              ],
              "rules": [
                {
                  "name": "Voltage Range",
                  "targetKey": "voltage",
                  "ruleType": "Range",
                  "min": 11.5,
                  "max": 12.8
                }
              ]
            }
            """);

        var service = new RecipeValidationService();
        var result = await service.ValidateAsync(recipePath, string.Empty, outputDirectory, null, null, CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected supported dut.* placeholders to pass validation.");
        AssertEqual("0", result.Errors.Count.ToString(), "Expected no validation errors for supported dut.* placeholders.");
        AssertTrue(
            result.Warnings.Any(item => item.Contains("dut.sn", StringComparison.Ordinal)),
            "Expected validation warnings to mark dut.sn as runtime-provided.");
        AssertTrue(
            result.Warnings.Any(item => item.Contains("dut.station", StringComparison.Ordinal)),
            "Expected validation warnings to mark dut.station as runtime-provided.");
    }

    private static async Task TestRecipeValidationFailsForMalformedVariableTemplateSyntaxAsync()
    {
        var outputDirectory = CreateOutputDirectory("variable-validate");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "variable-validate.recipe.json",
            """
            {
              "name": "Malformed Variable Recipe",
              "scripts": [
                {
                  "name": "BrokenStep",
                  "command": "READ_${SN",
                  "measurementKey": "voltage",
                  "unit": "V",
                  "simulatedResponse": "12.3"
                }
              ],
              "rules": [
                {
                  "name": "Voltage Range",
                  "targetKey": "voltage",
                  "ruleType": "Range",
                  "min": 11.5,
                  "max": 12.8
                }
              ]
            }
            """);

        var service = new RecipeValidationService();
        var result = await service.ValidateAsync(recipePath, string.Empty, outputDirectory, null, null, CancellationToken.None);

        AssertEqual("Failed", result.Status, "Expected malformed variable syntax to fail validation.");
        AssertTrue(
            result.Errors.Any(item => item.Contains("unterminated variable placeholder", StringComparison.Ordinal)),
            "Expected validation errors to mention the unterminated variable placeholder.");
    }

    private static async Task TestRuntimeMissingVariableProducesStructuredFailureEventAsync()
    {
        var outputDirectory = CreateOutputDirectory("variable-missing-runtime");
        var recipePath = WriteJsonFile(
            outputDirectory,
            "variable-missing-runtime.recipe.json",
            """
            {
              "name": "Missing Variable Runtime Recipe",
              "scripts": [
                {
                  "name": "BrokenStep",
                  "command": "READ_${MissingCommand}",
                  "measurementKey": "voltage",
                  "unit": "V",
                  "simulatedResponse": "12.3"
                }
              ],
              "rules": [
                {
                  "name": "Voltage Range",
                  "targetKey": "voltage",
                  "ruleType": "Range",
                  "min": 11.5,
                  "max": 12.8
                }
              ]
            }
            """);

        var runner = new TestRunner();
        var result = await runner.RunAsync(
            new TestRunRequest("test run", recipePath, string.Empty, outputDirectory, string.Empty),
            CancellationToken.None);

        AssertEqual("Error", result.Status, "Expected missing runtime variable to fail the step.");
        AssertTrue(
            result.Errors.Any(item => item.Contains("MissingCommand", StringComparison.Ordinal)),
            "Expected runtime error to mention the missing variable.");

        var entries = ReadStructuredLogEntries(result.StructuredLogPath);
        AssertTrue(
            entries.Any(item =>
                item.EntryType == StructuredLogEntryType.VariableResolutionFailed &&
                item.Data.TryGetValue("variableName", out var variableName) &&
                string.Equals(variableName?.ToString(), "MissingCommand", StringComparison.Ordinal)),
            "Expected structured log to record the variable resolution failure.");
    }

    private static async Task TestInvalidRunPreservesArtifactsAsync()
    {
        var outputDirectory = CreateOutputDirectory("invalid-run");
        var recipePath = Path.Combine(outputDirectory, "invalid.recipe.json");

        File.WriteAllText(
            recipePath,
            """
            {
              "name": "Invalid Recipe",
              "scripts": [
                {
                  "name": "BrokenScript",
                  "command": "READ_VOLTAGE",
                  "measurementKey": "voltage",
                  "unit": "V",
                  "specKey": "missing_spec",
                  "simulatedResponse": "12.3"
                }
              ]
            }
            """);

        var runner = new TestRunner();
        var result = await runner.RunAsync(
            new TestRunRequest(
                "test run",
                recipePath,
                string.Empty,
                outputDirectory,
                string.Empty,
                null,
                new RunInputModel
                {
                    SerialNumber = "SN12345678",
                    Station = "LINE-1",
                    Mode = "RUN",
                    Values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["Operator"] = "MTE"
                    }
                }),
            CancellationToken.None);

        AssertEqual("Invalid", result.Status, "Expected invalid recipe status.");
        AssertTrue(result.Errors.Count > 0, "Expected invalid run to include errors.");
        AssertEqual("SN12345678", result.RunInput.SerialNumber, "Expected run input SN to be preserved.");
        AssertEqual("LINE-1", result.SessionInfo.Station, "Expected session info station to be preserved.");
        AssertArtifactsExist(outputDirectory);
    }

    private static ScriptResult Evaluate(
        SpecEngine engine,
        string rawValue,
        string ruleType,
        string expected = "",
        decimal? min = null,
        decimal? max = null)
    {
        return engine.Evaluate(
            new ScriptExecutionResult
            {
                ScriptName = "SampleScript",
                Command = "SAMPLE",
                SpecKey = "sample_spec",
                MeasurementSet = new MeasurementSet
                {
                    Source = "SampleScript",
                    Command = "SAMPLE",
                    CollectedAt = DateTimeOffset.UtcNow,
                    RawPayload = rawValue,
                    Items = new List<MeasurementItem>
                    {
                        new()
                        {
                            Key = "sample",
                            Prefix = string.Empty,
                            FullKey = "sample",
                            Value = rawValue,
                            ValueType = decimal.TryParse(rawValue, out _) ? MeasurementValueType.Number : MeasurementValueType.Text,
                            Unit = string.Empty,
                            RawText = rawValue
                        }
                    }
                }
            },
            new SpecDefinition
            {
                Key = "sample_spec",
                Operator = ruleType,
                Expected = expected,
                Minimum = min,
                Maximum = max
            });
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Repository root could not be found.");
    }

    private static string CreateOutputDirectory(string prefix)
    {
        var repositoryRoot = FindRepositoryRoot();
        var outputDirectory = Path.Combine(repositoryRoot, ".test-output", $"{prefix}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(outputDirectory);
        return outputDirectory;
    }

    private static string WriteJsonFile(string directory, string fileName, string json)
    {
        var path = Path.Combine(directory, fileName);
        File.WriteAllText(path, json);
        return path;
    }

    private static string WriteTestResultJson(string directory, string fileName, TestResult result)
    {
        var path = Path.Combine(directory, fileName);
        File.WriteAllText(path, JsonSerializer.Serialize(result, CreateJsonOptions()));
        return path;
    }

    private static string WriteStructuredLogJsonLines(
        string directory,
        string fileName,
        IEnumerable<StructuredLogEntry> entries)
    {
        var path = Path.Combine(directory, fileName);
        var lines = entries
            .Select(item => JsonSerializer.Serialize(item, CreateJsonLinesOptions()))
            .ToArray();
        File.WriteAllLines(path, lines);
        return path;
    }

    private static StructuredLogEntry CreateStructuredLogEntry(
        long sequence,
        StructuredLogEntryType entryType,
        string level,
        string message,
        string stepName,
        string status = "",
        IReadOnlyDictionary<string, object?>? data = null)
    {
        return new StructuredLogEntry
        {
            Sequence = sequence,
            SessionId = "SESSION-EVENT",
            TimestampUtc = DateTimeOffset.UtcNow,
            ElapsedMs = sequence,
            Level = level,
            EntryType = entryType,
            ItemName = stepName,
            StepName = stepName,
            Status = status,
            Message = message,
            Data = data is null
                ? new Dictionary<string, object?>()
                : new Dictionary<string, object?>(data, StringComparer.OrdinalIgnoreCase)
        };
    }

    private static AiAnalysisBundle CreateSampleAnalysisBundle(string resultJsonPath)
    {
        return new AiAnalysisBundleBuilder().Build(
            new RunArtifactSummary
            {
                SourcePath = resultJsonPath,
                SessionId = "SESSION-HTML-001",
                CommandName = "test run",
                RecipeName = "HTML Viewer Recipe",
                DeviceName = "FakeDevice",
                RunStatus = "Error",
                SerialNumber = "SN-HTML-001",
                Station = "ST-HTML",
                Mode = "RUN",
                DurationSeconds = 1.25,
                StepCount = 2,
                FailedStepCount = 1,
                MeasurementCount = 3,
                SpecCount = 2,
                FailedSpecCount = 1,
                ErrorCount = 1,
                VariableResolutionFailedCount = 1,
                WarningCount = 1,
                FailedStepNames = new List<string> { "ReadDut" },
                FailedTargetKeys = new List<string> { "battery.voltage" },
                ErrorMessages = new List<string> { "Variable resolution failed." },
                FirstFailureMessage = "Variable 'dut.sn' required by field 'Command' in step 'ReadDut' was not found in DutContext.",
                HasVariableResolutionFailures = true,
                ResultJsonPath = resultJsonPath,
                StructuredLogPath = Path.ChangeExtension(resultJsonPath, ".events.jsonl")
            },
            new AiRunAnalysisResult
            {
                AnalyzerName = "RuleBasedRunAnalyzer",
                PrimaryCategory = "Configuration",
                PrimaryCause = "Variable resolution failed before the run could complete.",
                Confidence = 0.98,
                Summary = "Run status=Error, steps=2, failedSpecs=1, errors=1, variableFailures=1, exceptions=0, warnings=1.",
                Observations = new List<AiObservation>
                {
                    new()
                    {
                        Severity = "Error",
                        Title = "Variable Resolution Failures",
                        Detail = "Detected 1 variable resolution failures."
                    }
                },
                RecommendedActions = new List<string>
                {
                    "Verify the required value exists in Step, DUT, or Global inputs before rerunning."
                },
                Evidence = new List<AiEvidenceItem>
                {
                    new()
                    {
                        Type = "Metric",
                        Message = "Variable resolution failure count",
                        Source = "RunArtifactSummary.VariableResolutionFailedCount",
                        Value = "1"
                    }
                },
                MatchedRules = new List<string>
                {
                    "VariableResolutionFailureRule"
                }
            },
            resultJsonPath,
            Path.ChangeExtension(resultJsonPath, ".events.jsonl"),
            Path.ChangeExtension(resultJsonPath, ".analysis.json"));
    }

    private static AiAnalysisBundle CreateComparisonVariantBundle(string resultJsonPath)
    {
        var baseBundle = CreateSampleAnalysisBundle(resultJsonPath);
        return baseBundle with
        {
            ResultJsonPath = Path.GetFullPath(resultJsonPath),
            EventsJsonlPath = Path.GetFullPath(Path.ChangeExtension(resultJsonPath, ".events.jsonl")),
            AnalysisJsonPath = Path.GetFullPath(Path.ChangeExtension(resultJsonPath, ".analysis.json")),
            Summary = baseBundle.Summary with
            {
                SourcePath = resultJsonPath,
                ResultJsonPath = resultJsonPath,
                StructuredLogPath = Path.ChangeExtension(resultJsonPath, ".events.jsonl"),
                RunStatus = "Failed",
                FailedStepCount = 2,
                FailedSpecCount = 2,
                ErrorCount = 2,
                VariableResolutionFailedCount = 0,
                ExceptionCount = 1,
                WarningCount = 0,
                FailedStepNames = new List<string> { "PowerOn", "ReadVoltage" },
                ErrorMessages = new List<string> { "Unhandled exception interrupted the run." },
                FirstFailureMessage = "PowerOn step returned a failure status.",
                FirstExceptionMessage = "Unhandled exception while waiting for response.",
                HasVariableResolutionFailures = false,
                HasUnhandledException = true
            },
            Analysis = baseBundle.Analysis with
            {
                PrimaryCategory = "Runtime",
                PrimaryCause = "Unhandled exception interrupted the run.",
                Confidence = 0.93,
                Summary = "Run status=Failed, steps=2, failedSpecs=2, errors=2, variableFailures=0, exceptions=1, warnings=0.",
                Observations = new List<AiObservation>
                {
                    new()
                    {
                        Severity = "Error",
                        Title = "Unhandled Exception",
                        Detail = "Detected 1 unhandled exception."
                    }
                },
                RecommendedActions = new List<string>
                {
                    "Inspect the failing device or command path before rerunning.",
                    "Collect the session log for engineering review."
                },
                Evidence = new List<AiEvidenceItem>
                {
                    new()
                    {
                        Type = "Metric",
                        Message = "Unhandled exception count",
                        Source = "RunArtifactSummary.ExceptionCount",
                        Value = "1"
                    }
                },
                MatchedRules = new List<string>
                {
                    "UnhandledExceptionRule",
                    "StepFailureRule"
                }
            }
        };
    }

    private static AiAnalysisBundle CreateSuccessAnalysisBundle(string resultJsonPath)
    {
        return new AiAnalysisBundleBuilder().Build(
            new RunArtifactSummary
            {
                SourcePath = resultJsonPath,
                SessionId = "SESSION-SUCCESS-001",
                CommandName = "test run",
                RecipeName = "Success Recipe",
                DeviceName = "FakeDevice",
                RunStatus = "Passed",
                SerialNumber = "SN-SUCCESS-001",
                Station = "ST-SUCCESS",
                Mode = "RUN",
                DurationSeconds = 1.15,
                StepCount = 2,
                PassedStepCount = 2,
                FailedStepCount = 0,
                ErrorStepCount = 0,
                MeasurementCount = 3,
                SpecCount = 2,
                FailedSpecCount = 0,
                ErrorCount = 0,
                VariableResolvedCount = 5,
                VariableResolutionFailedCount = 0,
                ExceptionCount = 0,
                WarningCount = 0,
                ResultJsonPath = resultJsonPath,
                StructuredLogPath = Path.ChangeExtension(resultJsonPath, ".events.jsonl")
            },
            new AiRunAnalysisResult
            {
                AnalyzerName = "RuleBasedRunAnalyzer",
                PrimaryCategory = "Success",
                PrimaryCause = "The run completed successfully without detected failures.",
                Confidence = 0.99,
                Summary = "Run status=Passed, steps=2, failedSpecs=0, errors=0, variableFailures=0, exceptions=0, warnings=0.",
                Observations = new List<AiObservation>
                {
                    new()
                    {
                        Severity = "Info",
                        Title = "Run Passed",
                        Detail = "Run completed successfully with 3 measurements and 2 spec evaluations."
                    }
                },
                RecommendedActions = new List<string>
                {
                    "Archive the run artifacts if this session should be retained as a known-good baseline."
                },
                Evidence = new List<AiEvidenceItem>
                {
                    new()
                    {
                        Type = "Status",
                        Message = "Run status",
                        Source = "RunArtifactSummary.RunStatus",
                        Value = "Passed"
                    }
                },
                MatchedRules = new List<string>
                {
                    "SuccessRule"
                }
            },
            resultJsonPath,
            Path.ChangeExtension(resultJsonPath, ".events.jsonl"),
            Path.ChangeExtension(resultJsonPath, ".analysis.json"));
    }

    private static AiAnalysisBundle CreateStepFailureAnalysisBundle(string resultJsonPath)
    {
        return new AiAnalysisBundleBuilder().Build(
            new RunArtifactSummary
            {
                SourcePath = resultJsonPath,
                SessionId = "SESSION-STEP-FAIL-001",
                CommandName = "test run",
                RecipeName = "Step Failure Recipe",
                DeviceName = "FakeDevice",
                RunStatus = "Failed",
                SerialNumber = "SN-STEP-001",
                Station = "ST-STEP",
                Mode = "RUN",
                DurationSeconds = 1.45,
                StepCount = 2,
                PassedStepCount = 1,
                FailedStepCount = 1,
                ErrorStepCount = 0,
                MeasurementCount = 2,
                SpecCount = 1,
                FailedSpecCount = 0,
                ErrorCount = 1,
                VariableResolvedCount = 4,
                VariableResolutionFailedCount = 0,
                ExceptionCount = 0,
                WarningCount = 0,
                FailedStepNames = new List<string> { "ReadVoltage" },
                FirstFailureMessage = "ReadVoltage step returned a failure status.",
                ResultJsonPath = resultJsonPath,
                StructuredLogPath = Path.ChangeExtension(resultJsonPath, ".events.jsonl")
            },
            new AiRunAnalysisResult
            {
                AnalyzerName = "RuleBasedRunAnalyzer",
                PrimaryCategory = "Execution",
                PrimaryCause = "One or more steps did not complete successfully.",
                Confidence = 0.88,
                Summary = "Run status=Failed, steps=2, failedSpecs=0, errors=1, variableFailures=0, exceptions=0, warnings=0.",
                Observations = new List<AiObservation>
                {
                    new()
                    {
                        Severity = "Warning",
                        Title = "Failed Steps Detected",
                        Detail = "Failed steps: ReadVoltage."
                    }
                },
                RecommendedActions = new List<string>
                {
                    "Inspect the failed step names in result.json and compare them with session.log."
                },
                Evidence = new List<AiEvidenceItem>
                {
                    new()
                    {
                        Type = "List",
                        Message = "Failed step names",
                        Source = "RunArtifactSummary.FailedStepNames",
                        Value = "ReadVoltage"
                    }
                },
                MatchedRules = new List<string>
                {
                    "StepFailureRule"
                }
            },
            resultJsonPath,
            Path.ChangeExtension(resultJsonPath, ".events.jsonl"),
            Path.ChangeExtension(resultJsonPath, ".analysis.json"));
    }

    private static AiAnalysisBundle CreateSpecFailureAnalysisBundle(string resultJsonPath)
    {
        return new AiAnalysisBundleBuilder().Build(
            new RunArtifactSummary
            {
                SourcePath = resultJsonPath,
                SessionId = "SESSION-SPEC-FAIL-001",
                CommandName = "test run",
                RecipeName = "Spec Failure Recipe",
                DeviceName = "FakeDevice",
                RunStatus = "Failed",
                SerialNumber = "SN-SPEC-001",
                Station = "ST-SPEC",
                Mode = "RUN",
                DurationSeconds = 1.22,
                StepCount = 2,
                PassedStepCount = 2,
                FailedStepCount = 0,
                ErrorStepCount = 0,
                MeasurementCount = 3,
                SpecCount = 2,
                FailedSpecCount = 2,
                ErrorCount = 0,
                VariableResolvedCount = 5,
                VariableResolutionFailedCount = 0,
                ExceptionCount = 0,
                WarningCount = 0,
                FailedTargetKeys = new List<string> { "battery.voltage", "battery.current" },
                ResultJsonPath = resultJsonPath,
                StructuredLogPath = Path.ChangeExtension(resultJsonPath, ".events.jsonl")
            },
            new AiRunAnalysisResult
            {
                AnalyzerName = "RuleBasedRunAnalyzer",
                PrimaryCategory = "Spec",
                PrimaryCause = "Measurements were collected, but one or more spec rules failed.",
                Confidence = 0.84,
                Summary = "Run status=Failed, steps=2, failedSpecs=2, errors=0, variableFailures=0, exceptions=0, warnings=0.",
                Observations = new List<AiObservation>
                {
                    new()
                    {
                        Severity = "Warning",
                        Title = "Spec Failures Detected",
                        Detail = "Detected 2 failed spec evaluations."
                    }
                },
                RecommendedActions = new List<string>
                {
                    "Review failed target keys and compare measured values against the expected rule limits."
                },
                Evidence = new List<AiEvidenceItem>
                {
                    new()
                    {
                        Type = "Metric",
                        Message = "Failed spec count",
                        Source = "RunArtifactSummary.FailedSpecCount",
                        Value = "2"
                    }
                },
                MatchedRules = new List<string>
                {
                    "SpecFailureRule"
                }
            },
            resultJsonPath,
            Path.ChangeExtension(resultJsonPath, ".events.jsonl"),
            Path.ChangeExtension(resultJsonPath, ".analysis.json"));
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    private static async Task<string> CaptureStandardOutputAsync(Func<Task> action)
    {
        var originalOut = Console.Out;
        using var writer = new StringWriter();

        try
        {
            Console.SetOut(writer);
            await action();
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        return writer.ToString();
    }

    private static JsonSerializerOptions CreateJsonLinesOptions()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = false
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    private static TestRunner CreateTestRunner(IDeviceFactory deviceFactory)
    {
        return new TestRunner(
            new RecipeLoader(),
            new RecipeValidator(),
            new SpecLoader(),
            new SpecValidator(),
            new FlowEngine(new SpecEngine()),
            new SessionFactory(),
            new SessionArtifactWriter(),
            deviceFactory);
    }

    private static void AssertArtifactsExist(string outputDirectory)
    {
        AssertTrue(Directory.GetFiles(outputDirectory, "*.json", SearchOption.AllDirectories).Length > 0, "No JSON artifact was created.");
        AssertTrue(Directory.GetFiles(outputDirectory, "*.csv", SearchOption.AllDirectories).Length > 0, "No CSV artifact was created.");
        AssertTrue(Directory.GetFiles(outputDirectory, "*.log", SearchOption.AllDirectories).Length > 0, "No log artifact was created.");
        AssertTrue(Directory.GetFiles(outputDirectory, "*.jsonl", SearchOption.AllDirectories).Length > 0, "No structured log artifact was created.");
    }

    private static List<StructuredLogEntry> ReadStructuredLogEntries(string path)
    {
        var options = new JsonSerializerOptions
        {
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };

        return File.ReadAllLines(path)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonSerializer.Deserialize<StructuredLogEntry>(line, options) ?? throw new InvalidOperationException("Structured log entry could not be parsed."))
            .ToList();
    }

    private static void AssertEqual(string expected, string actual, string message)
    {
        if (!string.Equals(expected, actual, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"{message} Expected '{expected}', got '{actual}'.");
        }
    }

    private static void AssertTrue(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static void AssertContains(IEnumerable<string> values, string expectedValue, string message)
    {
        if (!values.Any(item => item.Contains(expectedValue, StringComparison.Ordinal)))
        {
            throw new InvalidOperationException(message);
        }
    }
}

internal sealed class DelegateDeviceFactory : IDeviceFactory
{
    private readonly Func<IDevice> _createDevice;

    public DelegateDeviceFactory(Func<IDevice> createDevice)
    {
        _createDevice = createDevice;
    }

    public IDevice CreateDevice()
    {
        return _createDevice();
    }
}

internal sealed class CallbackDevice : IDevice
{
    private readonly Func<DeviceCommandRequest, int, CancellationToken, Task<DeviceCommandResponse>> _executeAsync;
    private readonly Dictionary<string, int> _commandAttempts = new(StringComparer.OrdinalIgnoreCase);
    private bool _isConnected;

    public CallbackDevice(
        string name,
        Func<DeviceCommandRequest, int, CancellationToken, Task<DeviceCommandResponse>> executeAsync)
    {
        Name = name;
        _executeAsync = executeAsync;
    }

    public string Name { get; }

    public Task ConnectAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _isConnected = true;
        return Task.CompletedTask;
    }

    public Task DisconnectAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _isConnected = false;
        return Task.CompletedTask;
    }

    public Task<DeviceCommandResponse> ExecuteAsync(DeviceCommandRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_isConnected)
        {
            throw new InvalidOperationException($"{Name} is not connected.");
        }

        var attemptNumber = _commandAttempts.TryGetValue(request.Command, out var currentAttempt)
            ? currentAttempt + 1
            : 1;
        _commandAttempts[request.Command] = attemptNumber;

        return _executeAsync(request, attemptNumber, cancellationToken);
    }
}
