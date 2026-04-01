using ATS.Application.Execution;
using ATS.Application.Recipes;
using ATS.Application.Simulation;
using ATS.Application.Specs;
using ATS.Core.Models;

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
        await RunTestAsync("ScriptRunner executes selected script only", TestScriptRunnerExecutesSelectedScriptAsync, failures);
        await RunTestAsync("DeviceExecutor writes device artifacts", TestDeviceExecutorWritesArtifactsAsync, failures);
        await RunTestAsync("Recipe validation passes for Phase 2 sample", TestRecipeValidationPassesAsync, failures);
        await RunTestAsync("Spec validation passes for Phase 2 sample", TestSpecValidationPassesAsync, failures);
        await RunTestAsync("Recipe validation passes for multi-measurement sample", TestMultiMeasurementRecipeValidationPassesAsync, failures);
        await RunTestAsync("Spec validation passes for multi-measurement sample", TestMultiMeasurementSpecValidationPassesAsync, failures);
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

        var result = await service.RunAsync(recipePath, outputDirectory, CancellationToken.None);

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
            CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected selected script to pass.");
        AssertEqual("1", result.Steps.Count.ToString(), "Expected only one step result.");
        AssertEqual("ReadSerial", result.Steps[0].StepName, "Expected selected script name to match.");
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

    private static async Task TestRecipeValidationPassesAsync()
    {
        var repositoryRoot = FindRepositoryRoot();
        var recipePath = Path.Combine(repositoryRoot, "samples", "recipes", "phase2.recipe.json");
        var specPath = Path.Combine(repositoryRoot, "samples", "specs", "phase2.spec.json");
        var outputDirectory = CreateOutputDirectory("recipe-validate");
        var service = new RecipeValidationService();

        var result = await service.ValidateAsync(recipePath, specPath, outputDirectory, CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected recipe validation to pass.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestSpecValidationPassesAsync()
    {
        var repositoryRoot = FindRepositoryRoot();
        var specPath = Path.Combine(repositoryRoot, "samples", "specs", "phase2.spec.json");
        var outputDirectory = CreateOutputDirectory("spec-validate");
        var service = new SpecValidationService();

        var result = await service.ValidateAsync(specPath, outputDirectory, CancellationToken.None);

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

        var result = await service.ValidateAsync(recipePath, specPath, outputDirectory, CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected multi-measurement recipe validation to pass.");
        AssertArtifactsExist(outputDirectory);
    }

    private static async Task TestMultiMeasurementSpecValidationPassesAsync()
    {
        var repositoryRoot = FindRepositoryRoot();
        var specPath = Path.Combine(repositoryRoot, "samples", "specs", "multi-measurement.spec.json");
        var outputDirectory = CreateOutputDirectory("multi-spec-validate");
        var service = new SpecValidationService();

        var result = await service.ValidateAsync(specPath, outputDirectory, CancellationToken.None);

        AssertEqual("Passed", result.Status, "Expected multi-measurement spec validation to pass.");
        AssertArtifactsExist(outputDirectory);
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
            new TestRunRequest("test run", recipePath, string.Empty, outputDirectory, string.Empty),
            CancellationToken.None);

        AssertEqual("Invalid", result.Status, "Expected invalid recipe status.");
        AssertTrue(result.Errors.Count > 0, "Expected invalid run to include errors.");
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

    private static void AssertArtifactsExist(string outputDirectory)
    {
        AssertTrue(File.Exists(Path.Combine(outputDirectory, "result.json")), "result.json was not created.");
        AssertTrue(File.Exists(Path.Combine(outputDirectory, "result.csv")), "result.csv was not created.");
        AssertTrue(File.Exists(Path.Combine(outputDirectory, "session.log")), "session.log was not created.");
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
}
