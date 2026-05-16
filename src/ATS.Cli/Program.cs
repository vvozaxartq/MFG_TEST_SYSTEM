using ATS.Application.Execution;
using ATS.Application.Ai;
using ATS.Application.Recipes;
using ATS.Application.Specs;
using ATS.Core.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

return await CliProgram.RunAsync(args);

public static class CliProgram
{
    private const int ExitSuccess = 0;
    private const int ExitTestFailure = 1;
    private const int ExitInvalidArguments = 2;
    private const int ExitValidationFailure = 3;
    private const int ExitRuntimeError = 4;
    private static readonly JsonSerializerOptions AnalysisJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public static async Task<int> RunAsync(string[] args)
    {
        if (args.Length == 0 || (args.Length == 1 && IsHelpFlag(args[0])))
        {
            PrintRootUsage();
            return ExitSuccess;
        }

        if (!TryParseCommand(args, out var command, out var errorMessage))
        {
            Console.Error.WriteLine(errorMessage);
            PrintRootUsage();
            return ExitInvalidArguments;
        }

        if (command.ShowHelp)
        {
            PrintCommandUsage(command);
            return ExitSuccess;
        }

        try
        {
            return command.Category switch
            {
                "test" when command.Action is "simulate" or "run" => await RunTestCommandAsync(command),
                "script" when command.Action == "run" => await RunScriptCommandAsync(command),
                "device" when command.Action == "exec" => await RunDeviceCommandAsync(command),
                "ai" when command.Action == "analyze" => await RunAiAnalyzeCommandAsync(command),
                "ai" when command.Action == "render" => await RunAiRenderCommandAsync(command),
                "ai" when command.Action == "compare" => await RunAiCompareCommandAsync(command),
                "ai" when command.Action == "regress" => await RunAiRegressCommandAsync(command),
                "recipe" when command.Action == "validate" => await RunRecipeValidationAsync(command),
                "spec" when command.Action == "validate" => await RunSpecValidationAsync(command),
                _ => HandleUnsupportedCommand(command)
            };
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
            return ExitRuntimeError;
        }
    }

    private static async Task<int> RunTestCommandAsync(ParsedCommand command)
    {
        if (!TryGetRequiredOption(command, "recipe", out var recipePath, out var errorMessage))
        {
            Console.Error.WriteLine(errorMessage);
            PrintCommandUsage(command);
            return ExitInvalidArguments;
        }

        var specPath = GetOption(command, "spec");
        var outputDirectory = GetOption(command, "output", Directory.GetCurrentDirectory());
        if (!TryBuildRunConfiguration(command, out var artifactOptions, out var runInput, out errorMessage))
        {
            Console.Error.WriteLine(errorMessage);
            PrintCommandUsage(command);
            return ExitInvalidArguments;
        }
        var runner = new TestRunner();
        var result = await runner.RunAsync(
            new TestRunRequest(
                $"{command.Category} {command.Action}",
                recipePath,
                specPath,
                outputDirectory,
                string.Empty,
                artifactOptions,
                runInput),
            CancellationToken.None);

        PrintTestResult(result);
        return MapTestExitCode(result);
    }

    private static async Task<int> RunScriptCommandAsync(ParsedCommand command)
    {
        var recipeError = string.Empty;
        var scriptError = string.Empty;

        if (!TryGetRequiredOption(command, "recipe", out var recipePath, out recipeError) ||
            !TryGetRequiredOption(command, "script", out var scriptName, out scriptError))
        {
            Console.Error.WriteLine(string.IsNullOrWhiteSpace(recipeError) ? scriptError : recipeError);
            PrintCommandUsage(command);
            return ExitInvalidArguments;
        }

        var specPath = GetOption(command, "spec");
        var outputDirectory = GetOption(command, "output", Directory.GetCurrentDirectory());
        if (!TryBuildRunConfiguration(command, out var artifactOptions, out var runInput, out recipeError))
        {
            Console.Error.WriteLine(recipeError);
            PrintCommandUsage(command);
            return ExitInvalidArguments;
        }
        var runner = new ScriptRunner();
        var result = await runner.RunAsync(
            recipePath,
            specPath,
            scriptName,
            outputDirectory,
            artifactOptions,
            runInput,
            CancellationToken.None);

        PrintTestResult(result);
        return MapTestExitCode(result);
    }

    private static async Task<int> RunDeviceCommandAsync(ParsedCommand command)
    {
        if (!TryGetRequiredOption(command, "command", out var deviceCommand, out var errorMessage))
        {
            Console.Error.WriteLine(errorMessage);
            PrintCommandUsage(command);
            return ExitInvalidArguments;
        }

        var outputDirectory = GetOption(command, "output", Directory.GetCurrentDirectory());
        if (!TryBuildRunConfiguration(command, out var artifactOptions, out var runInput, out errorMessage))
        {
            Console.Error.WriteLine(errorMessage);
            PrintCommandUsage(command);
            return ExitInvalidArguments;
        }
        var executor = new DeviceExecutor();
        var result = await executor.ExecuteAsync(
            new DeviceExecutionRequest("device exec", deviceCommand, outputDirectory, artifactOptions, runInput),
            CancellationToken.None);

        Console.WriteLine($"Device: {result.DeviceName}");
        Console.WriteLine($"Session: {result.SessionId}");
        Console.WriteLine($"Command: {result.Command}");
        Console.WriteLine($"Response: {result.Response}");
        Console.WriteLine($"Status: {result.Status}");
        Console.WriteLine($"Message: {result.Message}");
        PrintArtifactPaths(result.ResultJsonPath, result.ResultCsvPath, result.SessionLogPath, result.StructuredLogPath);
        return string.Equals(result.Status, "Passed", StringComparison.OrdinalIgnoreCase)
            ? ExitSuccess
            : ExitRuntimeError;
    }

    private static async Task<int> RunRecipeValidationAsync(ParsedCommand command)
    {
        if (!TryGetRequiredOption(command, "recipe", out var recipePath, out var errorMessage))
        {
            Console.Error.WriteLine(errorMessage);
            PrintCommandUsage(command);
            return ExitInvalidArguments;
        }

        var specPath = GetOption(command, "spec");
        var outputDirectory = GetOption(command, "output", Directory.GetCurrentDirectory());
        if (!TryBuildRunConfiguration(command, out var artifactOptions, out var runInput, out errorMessage))
        {
            Console.Error.WriteLine(errorMessage);
            PrintCommandUsage(command);
            return ExitInvalidArguments;
        }
        var service = new RecipeValidationService();
        var result = await service.ValidateAsync(recipePath, specPath, outputDirectory, artifactOptions, runInput, CancellationToken.None);

        PrintValidationResult(result);
        return string.Equals(result.Status, "Passed", StringComparison.OrdinalIgnoreCase)
            ? ExitSuccess
            : ExitValidationFailure;
    }

    private static async Task<int> RunAiAnalyzeCommandAsync(ParsedCommand command)
    {
        if (!TryGetRequiredOption(command, "result-json", out var resultJsonPath, out var errorMessage))
        {
            Console.Error.WriteLine(errorMessage);
            PrintCommandUsage(command);
            return ExitInvalidArguments;
        }

        var eventsJsonlPath = GetOption(command, "events-jsonl");
        var outputJsonPath = GetOption(command, "output-json");
        var outputBundleJsonPath = GetOption(command, "output-bundle-json");
        var providerName = GetOption(command, "provider");
        var service = new RunAnalysisService();
        AiAnalysisBundle? bundle = null;
        AiRunAnalysisResult result;
        AiProviderResponse? providerResponse = null;
        var shouldBuildBundle = !string.IsNullOrWhiteSpace(outputBundleJsonPath) || !string.IsNullOrWhiteSpace(providerName);

        if (shouldBuildBundle)
        {
            bundle = await service.AnalyzeBundleAsync(resultJsonPath, eventsJsonlPath, outputJsonPath, CancellationToken.None);
            result = bundle.Analysis;
        }
        else
        {
            result = await service.AnalyzeAsync(resultJsonPath, eventsJsonlPath, CancellationToken.None);
        }

        if (!string.IsNullOrWhiteSpace(outputJsonPath))
        {
            var fullOutputJsonPath = Path.GetFullPath(outputJsonPath);
            var outputDirectory = Path.GetDirectoryName(fullOutputJsonPath);
            if (!string.IsNullOrWhiteSpace(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            File.WriteAllText(fullOutputJsonPath, JsonSerializer.Serialize(result, AnalysisJsonOptions));
        }

        if (bundle is not null)
        {
            if (!string.IsNullOrWhiteSpace(outputBundleJsonPath))
            {
                var writer = new AiAnalysisBundleWriter();
                await writer.WriteAsync(bundle, outputBundleJsonPath, CancellationToken.None);
            }

            if (!string.IsNullOrWhiteSpace(providerName))
            {
                if (!TryResolveBundleProvider(providerName, out var provider, out errorMessage))
                {
                    Console.Error.WriteLine(errorMessage);
                    PrintCommandUsage(command);
                    return ExitInvalidArguments;
                }

                providerResponse = await provider.AnalyzeAsync(
                    new AiProviderRequest
                    {
                        Bundle = bundle
                    },
                    CancellationToken.None);
            }
        }

        PrintAiAnalysisResult(resultJsonPath, eventsJsonlPath, result);

        if (providerResponse is not null)
        {
            PrintProviderResponse(providerResponse);
        }

        return ExitSuccess;
    }

    private static async Task<int> RunAiRenderCommandAsync(ParsedCommand command)
    {
        if (!TryGetRequiredOption(command, "bundle-json", out var bundleJsonPath, out var errorMessage) ||
            !TryGetRequiredOption(command, "output-html", out var outputHtmlPath, out errorMessage))
        {
            Console.Error.WriteLine(errorMessage);
            PrintCommandUsage(command);
            return ExitInvalidArguments;
        }

        var bundle = await ReadAnalysisBundleAsync(bundleJsonPath, CancellationToken.None);
        var fullBundlePath = Path.GetFullPath(bundleJsonPath);
        var writer = new AiAnalysisHtmlWriter();
        var fullOutputHtmlPath = await writer.WriteAsync(bundle, outputHtmlPath, CancellationToken.None);
        PrintAiRenderResult(fullBundlePath, fullOutputHtmlPath, bundle);
        return ExitSuccess;
    }

    private static async Task<int> RunAiCompareCommandAsync(ParsedCommand command)
    {
        var leftBundleError = string.Empty;
        var rightBundleError = string.Empty;
        var outputHtmlError = string.Empty;

        if (!TryGetRequiredOption(command, "left-bundle", out var leftBundlePath, out leftBundleError) ||
            !TryGetRequiredOption(command, "right-bundle", out var rightBundlePath, out rightBundleError) ||
            !TryGetRequiredOption(command, "output-html", out var outputHtmlPath, out outputHtmlError))
        {
            Console.Error.WriteLine(
                !string.IsNullOrWhiteSpace(leftBundleError)
                    ? leftBundleError
                    : !string.IsNullOrWhiteSpace(rightBundleError)
                        ? rightBundleError
                        : outputHtmlError);
            PrintCommandUsage(command);
            return ExitInvalidArguments;
        }

        var leftBundle = await ReadAnalysisBundleAsync(leftBundlePath, CancellationToken.None);
        var rightBundle = await ReadAnalysisBundleAsync(rightBundlePath, CancellationToken.None);
        var comparison = new AiAnalysisBundleComparisonBuilder().Build(leftBundle, rightBundle);
        var writer = new AiAnalysisComparisonHtmlWriter();
        var fullOutputHtmlPath = await writer.WriteAsync(comparison, outputHtmlPath, CancellationToken.None);
        PrintAiCompareResult(Path.GetFullPath(leftBundlePath), Path.GetFullPath(rightBundlePath), fullOutputHtmlPath, comparison);
        return ExitSuccess;
    }

    private static async Task<int> RunAiRegressCommandAsync(ParsedCommand command)
    {
        var baselineBundleError = string.Empty;
        var candidateBundleError = string.Empty;
        var outputJsonError = string.Empty;
        var outputHtmlError = string.Empty;

        if (!TryGetRequiredOption(command, "baseline-bundle", out var baselineBundlePath, out baselineBundleError) ||
            !TryGetRequiredOption(command, "candidate-bundle", out var candidateBundlePath, out candidateBundleError) ||
            !TryGetRequiredOption(command, "output-json", out var outputJsonPath, out outputJsonError) ||
            !TryGetRequiredOption(command, "output-html", out var outputHtmlPath, out outputHtmlError))
        {
            Console.Error.WriteLine(
                !string.IsNullOrWhiteSpace(baselineBundleError)
                    ? baselineBundleError
                    : !string.IsNullOrWhiteSpace(candidateBundleError)
                        ? candidateBundleError
                        : !string.IsNullOrWhiteSpace(outputJsonError)
                            ? outputJsonError
                            : outputHtmlError);
            PrintCommandUsage(command);
            return ExitInvalidArguments;
        }

        var baselineBundle = await ReadAnalysisBundleAsync(baselineBundlePath, CancellationToken.None);
        var candidateBundle = await ReadAnalysisBundleAsync(candidateBundlePath, CancellationToken.None);
        var checker = new AiRegressionChecker();
        var regressionResult = checker.Check(baselineBundle, candidateBundle) with
        {
            BaselineBundlePath = Path.GetFullPath(baselineBundlePath),
            CandidateBundlePath = Path.GetFullPath(candidateBundlePath)
        };

        var fullOutputJsonPath = Path.GetFullPath(outputJsonPath);
        var outputJsonDirectory = Path.GetDirectoryName(fullOutputJsonPath);
        if (!string.IsNullOrWhiteSpace(outputJsonDirectory))
        {
            Directory.CreateDirectory(outputJsonDirectory);
        }

        await File.WriteAllTextAsync(
            fullOutputJsonPath,
            JsonSerializer.Serialize(regressionResult, AnalysisJsonOptions),
            CancellationToken.None);

        var htmlWriter = new AiRegressionHtmlWriter();
        var fullOutputHtmlPath = await htmlWriter.WriteAsync(
            regressionResult,
            baselineBundle,
            candidateBundle,
            outputHtmlPath,
            CancellationToken.None);

        PrintAiRegressionResult(
            Path.GetFullPath(baselineBundlePath),
            Path.GetFullPath(candidateBundlePath),
            fullOutputJsonPath,
            fullOutputHtmlPath,
            regressionResult);
        return ExitSuccess;
    }

    private static async Task<int> RunSpecValidationAsync(ParsedCommand command)
    {
        if (!TryGetRequiredOption(command, "spec", out var specPath, out var errorMessage))
        {
            Console.Error.WriteLine(errorMessage);
            PrintCommandUsage(command);
            return ExitInvalidArguments;
        }

        var outputDirectory = GetOption(command, "output", Directory.GetCurrentDirectory());
        if (!TryBuildRunConfiguration(command, out var artifactOptions, out var runInput, out errorMessage))
        {
            Console.Error.WriteLine(errorMessage);
            PrintCommandUsage(command);
            return ExitInvalidArguments;
        }
        var service = new SpecValidationService();
        var result = await service.ValidateAsync(specPath, outputDirectory, artifactOptions, runInput, CancellationToken.None);

        PrintValidationResult(result);
        return string.Equals(result.Status, "Passed", StringComparison.OrdinalIgnoreCase)
            ? ExitSuccess
            : ExitValidationFailure;
    }

    private static int HandleUnsupportedCommand(ParsedCommand command)
    {
        Console.Error.WriteLine($"Command '{command.Category} {command.Action}' is not supported.");
        PrintRootUsage();
        return ExitInvalidArguments;
    }

    private static void PrintTestResult(TestResult result)
    {
        Console.WriteLine($"Command: {result.CommandName}");
        Console.WriteLine($"Recipe: {result.RecipeName}");
        Console.WriteLine($"Device: {result.DeviceName}");
        Console.WriteLine($"Session: {result.SessionId}");
        Console.WriteLine($"SN: {FormatDisplayValue(result.RunInput.SerialNumber)}");
        Console.WriteLine($"Station: {FormatDisplayValue(result.RunInput.Station)}");
        Console.WriteLine($"Mode: {FormatDisplayValue(result.RunInput.Mode)}");

        foreach (var step in result.Steps)
        {
            Console.WriteLine($"{step.StepName}: {step.FinalStatus}");

            foreach (var measurement in step.Measurements)
            {
                var unitSuffix = string.IsNullOrWhiteSpace(measurement.Unit) ? string.Empty : $" {measurement.Unit}";
                Console.WriteLine($"  measurement {measurement.FullKey} = {measurement.Value}{unitSuffix}");
            }

            foreach (var specResult in step.SpecResults)
            {
                Console.WriteLine($"  {BuildRuleSummary(specResult)}");
            }
        }

        if (result.Steps.Count == 0)
        {
            foreach (var script in result.Scripts)
            {
                Console.WriteLine(
                    $"{script.ScriptName}: {script.Status} ({script.ActualValue}, operator {script.Operator}, message {script.Message})");
            }
        }

        foreach (var error in result.Errors)
        {
            Console.Error.WriteLine(error);
        }

        Console.WriteLine($"Status: {result.Status}");
        PrintArtifactPaths(result.ResultJsonPath, result.ResultCsvPath, result.SessionLogPath, result.StructuredLogPath);
    }

    private static void PrintAiAnalysisResult(string resultJsonPath, string eventsJsonlPath, AiRunAnalysisResult result)
    {
        Console.WriteLine("Command: ai analyze");
        Console.WriteLine($"Target: {Path.GetFullPath(resultJsonPath)}");

        if (!string.IsNullOrWhiteSpace(eventsJsonlPath))
        {
            Console.WriteLine($"Events: {Path.GetFullPath(eventsJsonlPath)}");
        }

        Console.WriteLine($"Analyzer: {result.AnalyzerName}");
        Console.WriteLine($"Primary Category: {FormatDisplayValue(result.PrimaryCategory)}");
        Console.WriteLine($"Primary Cause: {FormatDisplayValue(result.PrimaryCause)}");

        if (result.Confidence.HasValue)
        {
            Console.WriteLine($"Confidence: {result.Confidence.Value:0.00}");
        }

        Console.WriteLine($"Summary: {result.Summary}");

        foreach (var observation in result.Observations)
        {
            Console.WriteLine($"[{observation.Severity}] {observation.Title}: {observation.Detail}");
        }

        if (result.RecommendedActions.Count > 0)
        {
            Console.WriteLine("Recommended Actions:");
            foreach (var action in result.RecommendedActions)
            {
                Console.WriteLine($"- {action}");
            }
        }

        if (result.Evidence.Count > 0)
        {
            Console.WriteLine("Evidence:");
            foreach (var evidence in result.Evidence)
            {
                Console.WriteLine($"- [{evidence.Type}] {evidence.Message} | source={evidence.Source} | value={FormatDisplayValue(evidence.Value)}");
            }
        }
    }

    private static async Task<AiAnalysisBundle> ReadAnalysisBundleAsync(string bundleJsonPath, CancellationToken cancellationToken)
    {
        var fullBundlePath = Path.GetFullPath(bundleJsonPath);
        if (!File.Exists(fullBundlePath))
        {
            throw new FileNotFoundException("Bundle JSON file was not found.", fullBundlePath);
        }

        var json = await File.ReadAllTextAsync(fullBundlePath, cancellationToken);
        return JsonSerializer.Deserialize<AiAnalysisBundle>(json, AnalysisJsonOptions)
            ?? throw new InvalidOperationException("Bundle JSON file could not be parsed.");
    }

    private static void PrintProviderResponse(AiProviderResponse response)
    {
        Console.WriteLine($"Provider: {FormatDisplayValue(response.ProviderName)}");
        Console.WriteLine($"Provider Bundle Schema: {FormatDisplayValue(response.BundleSchemaVersion)}");
        Console.WriteLine($"Provider Primary Category: {FormatDisplayValue(response.PrimaryCategory)}");
        Console.WriteLine($"Provider Primary Cause: {FormatDisplayValue(response.PrimaryCause)}");
        Console.WriteLine($"Provider Summary: {FormatDisplayValue(response.Summary)}");

        if (response.Highlights.Count > 0)
        {
            Console.WriteLine("Provider Highlights:");
            foreach (var highlight in response.Highlights)
            {
                Console.WriteLine($"- {highlight}");
            }
        }

        if (response.RecommendedActions.Count > 0)
        {
            Console.WriteLine("Provider Recommended Actions:");
            foreach (var action in response.RecommendedActions)
            {
                Console.WriteLine($"- {action}");
            }
        }
    }

    private static void PrintAiRenderResult(string bundleJsonPath, string outputHtmlPath, AiAnalysisBundle bundle)
    {
        Console.WriteLine("Command: ai render");
        Console.WriteLine($"Bundle: {Path.GetFullPath(bundleJsonPath)}");
        Console.WriteLine($"Output: {Path.GetFullPath(outputHtmlPath)}");
        Console.WriteLine($"Analyzer: {FormatDisplayValue(bundle.AnalyzerName)}");
        Console.WriteLine($"Primary Category: {FormatDisplayValue(bundle.Analysis.PrimaryCategory)}");
        Console.WriteLine($"Primary Cause: {FormatDisplayValue(bundle.Analysis.PrimaryCause)}");
        Console.WriteLine($"Schema Version: {FormatDisplayValue(bundle.SchemaVersion)}");
    }

    private static void PrintAiCompareResult(
        string leftBundlePath,
        string rightBundlePath,
        string outputHtmlPath,
        AiAnalysisBundleComparison comparison)
    {
        Console.WriteLine("Command: ai compare");
        Console.WriteLine($"Left Bundle: {Path.GetFullPath(leftBundlePath)}");
        Console.WriteLine($"Right Bundle: {Path.GetFullPath(rightBundlePath)}");
        Console.WriteLine($"Output: {Path.GetFullPath(outputHtmlPath)}");
        Console.WriteLine($"Left Primary Category: {FormatDisplayValue(comparison.LeftBundle.Analysis.PrimaryCategory)}");
        Console.WriteLine($"Right Primary Category: {FormatDisplayValue(comparison.RightBundle.Analysis.PrimaryCategory)}");
        Console.WriteLine($"Differences Detected: {(comparison.HasDifferences ? "Yes" : "No")}");
        Console.WriteLine($"Changed Summary Counts: {comparison.SummaryCountChanges.Count}");
        Console.WriteLine($"Changed Matched Rules: {comparison.AddedMatchedRules.Count + comparison.RemovedMatchedRules.Count}");
    }

    private static void PrintAiRegressionResult(
        string baselineBundlePath,
        string candidateBundlePath,
        string outputJsonPath,
        string outputHtmlPath,
        AiRegressionCheckResult result)
    {
        Console.WriteLine("Command: ai regress");
        Console.WriteLine($"Baseline Bundle: {Path.GetFullPath(baselineBundlePath)}");
        Console.WriteLine($"Candidate Bundle: {Path.GetFullPath(candidateBundlePath)}");
        Console.WriteLine($"Output JSON: {Path.GetFullPath(outputJsonPath)}");
        Console.WriteLine($"Output HTML: {Path.GetFullPath(outputHtmlPath)}");
        Console.WriteLine($"Regression Status: {result.Status}");
        Console.WriteLine($"Finding Count: {result.Findings.Count}");
        Console.WriteLine($"Summary: {result.Summary}");

        foreach (var finding in result.Findings)
        {
            Console.WriteLine($"[{finding.Code}] {finding.Message}");
        }
    }

    private static void PrintValidationResult(ValidationResult result)
    {
        Console.WriteLine($"Command: {result.CommandName}");
        Console.WriteLine($"Target: {result.TargetPath}");
        Console.WriteLine($"SN: {FormatDisplayValue(result.RunInput.SerialNumber)}");
        Console.WriteLine($"Station: {FormatDisplayValue(result.RunInput.Station)}");
        Console.WriteLine($"Mode: {FormatDisplayValue(result.RunInput.Mode)}");

        if (!string.IsNullOrWhiteSpace(result.RelatedPath))
        {
            Console.WriteLine($"Related: {result.RelatedPath}");
        }

        foreach (var error in result.Errors)
        {
            Console.Error.WriteLine(error);
        }

        foreach (var warning in result.Warnings)
        {
            Console.WriteLine($"Warning: {warning}");
        }

        Console.WriteLine($"Status: {result.Status}");
        PrintArtifactPaths(result.ResultJsonPath, result.ResultCsvPath, result.SessionLogPath, result.StructuredLogPath);
    }

    private static int MapTestExitCode(TestResult result)
    {
        return result.Status.ToUpperInvariant() switch
        {
            "PASSED" => ExitSuccess,
            "FAILED" => ExitTestFailure,
            "INVALID" => ExitValidationFailure,
            _ => ExitRuntimeError
        };
    }

    private static bool TryParseCommand(string[] args, out ParsedCommand command, out string errorMessage)
    {
        command = new ParsedCommand(
            string.Empty,
            string.Empty,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
            new HashSet<string>(StringComparer.OrdinalIgnoreCase),
            false);
        errorMessage = string.Empty;

        if (args.Length < 2)
        {
            errorMessage = "Command requires <category> <action>.";
            return false;
        }

        var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var flags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var showHelp = false;

        for (var index = 2; index < args.Length; index++)
        {
            var argument = args[index];

            if (IsHelpFlag(argument))
            {
                showHelp = true;
                continue;
            }

            if (!argument.StartsWith("--", StringComparison.Ordinal))
            {
                errorMessage = $"Unknown argument '{argument}'.";
                return false;
            }

            var optionName = argument[2..];
            if (index + 1 >= args.Length || args[index + 1].StartsWith("--", StringComparison.Ordinal))
            {
                if (IsBooleanFlagOption(optionName))
                {
                    flags.Add(optionName);
                    continue;
                }

                errorMessage = $"Option '{argument}' requires a value.";
                return false;
            }

            options[optionName] = args[index + 1];
            index++;
        }

        command = new ParsedCommand(
            args[0].ToLowerInvariant(),
            args[1].ToLowerInvariant(),
            options,
            flags,
            showHelp);

        return true;
    }

    private static bool TryGetRequiredOption(
        ParsedCommand command,
        string optionName,
        out string value,
        out string errorMessage)
    {
        value = string.Empty;
        errorMessage = string.Empty;

        if (!command.Options.TryGetValue(optionName, out var optionValue) || string.IsNullOrWhiteSpace(optionValue))
        {
            errorMessage = $"Option '--{optionName}' is required.";
            return false;
        }

        value = optionValue;
        return true;
    }

    private static string GetOption(ParsedCommand command, string optionName, string defaultValue = "")
    {
        return command.Options.TryGetValue(optionName, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : defaultValue;
    }

    private static bool IsHelpFlag(string argument)
    {
        return string.Equals(argument, "--help", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(argument, "-h", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsBooleanFlagOption(string optionName)
    {
        return string.Equals(optionName, "prompt-sn", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryResolveBundleProvider(
        string providerName,
        out IAiBundleAnalysisProvider provider,
        out string errorMessage)
    {
        if (string.Equals(providerName, "fake", StringComparison.OrdinalIgnoreCase))
        {
            provider = new FakeBundleAnalysisProvider();
            errorMessage = string.Empty;
            return true;
        }

        provider = null!;
        errorMessage = $"Provider '{providerName}' is not supported. Supported providers: fake.";
        return false;
    }

    private static void PrintRootUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  ats test simulate --recipe <file> [--spec <file>] [--output <directory>] [--output-template <path>] [--json-template <path>] [--csv-template <path>] [--log-template <path>] [--sn <value>] [--prompt-sn] [--station <name>] [--mode <name>] [--vars <k=v;...>]");
        Console.WriteLine("  ats test run --recipe <file> [--spec <file>] [--output <directory>] [--output-template <path>] [--json-template <path>] [--csv-template <path>] [--log-template <path>] [--sn <value>] [--prompt-sn] [--station <name>] [--mode <name>] [--vars <k=v;...>]");
        Console.WriteLine("  ats script run --recipe <file> --script <name> [--spec <file>] [--output <directory>] [--output-template <path>] [--json-template <path>] [--csv-template <path>] [--log-template <path>] [--sn <value>] [--prompt-sn] [--station <name>] [--mode <name>] [--vars <k=v;...>]");
        Console.WriteLine("  ats device exec --command <text> [--output <directory>] [--output-template <path>] [--json-template <path>] [--csv-template <path>] [--log-template <path>] [--sn <value>] [--prompt-sn] [--station <name>] [--mode <name>] [--vars <k=v;...>]");
        Console.WriteLine("  ats ai analyze --result-json <file> [--events-jsonl <file>] [--output-json <file>] [--output-bundle-json <file>] [--provider fake]");
        Console.WriteLine("  ats ai render --bundle-json <file> --output-html <file>");
        Console.WriteLine("  ats ai compare --left-bundle <file> --right-bundle <file> --output-html <file>");
        Console.WriteLine("  ats ai regress --baseline-bundle <file> --candidate-bundle <file> --output-json <file> --output-html <file>");
        Console.WriteLine("  ats recipe validate --recipe <file> [--spec <file>] [--output <directory>] [--output-template <path>] [--json-template <path>] [--csv-template <path>] [--log-template <path>] [--sn <value>] [--prompt-sn] [--station <name>] [--mode <name>] [--vars <k=v;...>]");
        Console.WriteLine("  ats spec validate --spec <file> [--output <directory>] [--output-template <path>] [--json-template <path>] [--csv-template <path>] [--log-template <path>] [--sn <value>] [--prompt-sn] [--station <name>] [--mode <name>] [--vars <k=v;...>]");
        Console.WriteLine();
        Console.WriteLine("Exit Codes:");
        Console.WriteLine("  0 success");
        Console.WriteLine("  1 test completed with failed spec result");
        Console.WriteLine("  2 invalid CLI arguments");
        Console.WriteLine("  3 recipe/spec validation failed");
        Console.WriteLine("  4 runtime or device execution error");
    }

    private static void PrintCommandUsage(ParsedCommand command)
    {
        var usage = (command.Category, command.Action) switch
        {
            ("test", "simulate") => "ats test simulate --recipe <file> [--spec <file>] [--output <directory>] [--output-template <path>] [--json-template <path>] [--csv-template <path>] [--log-template <path>] [--sn <value>] [--prompt-sn] [--station <name>] [--mode <name>] [--vars <k=v;...>]",
            ("test", "run") => "ats test run --recipe <file> [--spec <file>] [--output <directory>] [--output-template <path>] [--json-template <path>] [--csv-template <path>] [--log-template <path>] [--sn <value>] [--prompt-sn] [--station <name>] [--mode <name>] [--vars <k=v;...>]",
            ("script", "run") => "ats script run --recipe <file> --script <name> [--spec <file>] [--output <directory>] [--output-template <path>] [--json-template <path>] [--csv-template <path>] [--log-template <path>] [--sn <value>] [--prompt-sn] [--station <name>] [--mode <name>] [--vars <k=v;...>]",
            ("device", "exec") => "ats device exec --command <text> [--output <directory>] [--output-template <path>] [--json-template <path>] [--csv-template <path>] [--log-template <path>] [--sn <value>] [--prompt-sn] [--station <name>] [--mode <name>] [--vars <k=v;...>]",
            ("ai", "analyze") => "ats ai analyze --result-json <file> [--events-jsonl <file>] [--output-json <file>] [--output-bundle-json <file>] [--provider fake]",
            ("ai", "render") => "ats ai render --bundle-json <file> --output-html <file>",
            ("ai", "compare") => "ats ai compare --left-bundle <file> --right-bundle <file> --output-html <file>",
            ("ai", "regress") => "ats ai regress --baseline-bundle <file> --candidate-bundle <file> --output-json <file> --output-html <file>",
            ("recipe", "validate") => "ats recipe validate --recipe <file> [--spec <file>] [--output <directory>] [--output-template <path>] [--json-template <path>] [--csv-template <path>] [--log-template <path>] [--sn <value>] [--prompt-sn] [--station <name>] [--mode <name>] [--vars <k=v;...>]",
            ("spec", "validate") => "ats spec validate --spec <file> [--output <directory>] [--output-template <path>] [--json-template <path>] [--csv-template <path>] [--log-template <path>] [--sn <value>] [--prompt-sn] [--station <name>] [--mode <name>] [--vars <k=v;...>]",
            _ => string.Empty
        };

        if (string.IsNullOrWhiteSpace(usage))
        {
            PrintRootUsage();
            return;
        }

        Console.WriteLine("Usage:");
        Console.WriteLine($"  {usage}");
    }

    private static void PrintArtifactPaths(string resultJsonPath, string resultCsvPath, string sessionLogPath, string structuredLogPath)
    {
        Console.WriteLine($"result.json: {Path.GetFullPath(resultJsonPath)}");
        Console.WriteLine($"result.csv: {Path.GetFullPath(resultCsvPath)}");
        Console.WriteLine($"session.log: {Path.GetFullPath(sessionLogPath)}");
        Console.WriteLine($"session.events.jsonl: {Path.GetFullPath(structuredLogPath)}");
    }

    private static bool TryBuildRunConfiguration(
        ParsedCommand command,
        out SessionArtifactOptions artifactOptions,
        out RunInputModel runInput,
        out string errorMessage)
    {
        var inputValues = ParseTemplateVariables(GetOption(command, "vars"));
        var serialNumber = ResolveSerialNumber(command, inputValues);
        if (HasFlag(command, "prompt-sn") && string.IsNullOrWhiteSpace(serialNumber))
        {
            Console.Write("Please scan or enter ProductSN: ");
            serialNumber = Console.ReadLine()?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                artifactOptions = new SessionArtifactOptions();
                runInput = new RunInputModel();
                errorMessage = "Serial number is required when '--prompt-sn' is used.";
                return false;
            }
        }

        if (!string.IsNullOrWhiteSpace(serialNumber))
        {
            inputValues["SN"] = serialNumber;
            inputValues["ProductSN"] = serialNumber;
        }

        var station = ResolveFirstNonEmpty(
            GetOption(command, "station"),
            GetDictionaryValue(inputValues, "Station"));
        if (!string.IsNullOrWhiteSpace(station))
        {
            inputValues["Station"] = station;
        }

        var mode = ResolveFirstNonEmpty(
            GetOption(command, "mode"),
            GetDictionaryValue(inputValues, "Mode"),
            ResolveDefaultMode(command));
        if (!string.IsNullOrWhiteSpace(mode))
        {
            inputValues["Mode"] = mode;
        }

        if (!TryResolvePromptVariables(command, inputValues, out errorMessage))
        {
            artifactOptions = new SessionArtifactOptions();
            runInput = new RunInputModel();
            return false;
        }

        serialNumber = ResolveFirstNonEmpty(
            serialNumber,
            GetDictionaryValue(inputValues, "SN"),
            GetDictionaryValue(inputValues, "ProductSN"));
        station = ResolveFirstNonEmpty(
            GetOption(command, "station"),
            GetDictionaryValue(inputValues, "Station"));
        mode = ResolveFirstNonEmpty(
            GetOption(command, "mode"),
            GetDictionaryValue(inputValues, "Mode"),
            ResolveDefaultMode(command));

        if (!string.IsNullOrWhiteSpace(serialNumber))
        {
            inputValues["SN"] = serialNumber;
            inputValues["ProductSN"] = serialNumber;
        }

        if (!string.IsNullOrWhiteSpace(station))
        {
            inputValues["Station"] = station;
        }

        if (!string.IsNullOrWhiteSpace(mode))
        {
            inputValues["Mode"] = mode;
        }

        artifactOptions = new SessionArtifactOptions
        {
            OutputDirectoryTemplate = GetOption(command, "output-template"),
            ResultJsonTemplate = GetOption(command, "json-template", "result.json"),
            ResultCsvTemplate = GetOption(command, "csv-template", "result.csv"),
            SessionLogTemplate = GetOption(command, "log-template", "session_%SessionId%.log"),
            Variables = inputValues
        };

        runInput = new RunInputModel
        {
            SerialNumber = serialNumber,
            Station = station,
            Mode = mode,
            Values = new Dictionary<string, string>(inputValues, StringComparer.OrdinalIgnoreCase)
        };

        errorMessage = string.Empty;
        return true;
    }

    private static bool TryResolvePromptVariables(
        ParsedCommand command,
        Dictionary<string, string> inputValues,
        out string errorMessage)
    {
        errorMessage = string.Empty;

        foreach (var variableName in ParseVariableNames(GetOption(command, "prompt-vars")))
        {
            if (inputValues.TryGetValue(variableName, out var existingValue) &&
                !string.IsNullOrWhiteSpace(existingValue))
            {
                continue;
            }

            Console.Write(BuildPromptLabel(variableName));
            var inputValue = Console.ReadLine()?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(inputValue))
            {
                errorMessage = $"Variable '{variableName}' is required.";
                return false;
            }

            inputValues[variableName] = inputValue;
        }

        return true;
    }

    private static Dictionary<string, string> ParseTemplateVariables(string rawValue)
    {
        var variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return variables;
        }

        var assignments = rawValue.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var assignment in assignments)
        {
            var separatorIndex = assignment.IndexOf('=', StringComparison.Ordinal);
            if (separatorIndex <= 0)
            {
                continue;
            }

            var name = assignment[..separatorIndex].Trim();
            var value = separatorIndex == assignment.Length - 1
                ? string.Empty
                : assignment[(separatorIndex + 1)..].Trim();

            if (!string.IsNullOrWhiteSpace(name))
            {
                variables[name] = value;
            }
        }

        return variables;
    }

    private static IReadOnlyList<string> ParseVariableNames(string rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return Array.Empty<string>();
        }

        return rawValue
            .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string BuildPromptLabel(string variableName)
    {
        return string.Equals(variableName, "ProductSN", StringComparison.OrdinalIgnoreCase)
               || string.Equals(variableName, "SN", StringComparison.OrdinalIgnoreCase)
            ? "Please scan or enter ProductSN: "
            : $"Please enter {variableName}: ";
    }

    private static string ResolveSerialNumber(ParsedCommand command, IReadOnlyDictionary<string, string> inputValues)
    {
        return ResolveFirstNonEmpty(
            GetOption(command, "sn"),
            GetOption(command, "product-sn"),
            GetDictionaryValue(inputValues, "SN"),
            GetDictionaryValue(inputValues, "ProductSN"));
    }

    private static string ResolveDefaultMode(ParsedCommand command)
    {
        return (command.Category, command.Action) switch
        {
            ("test", "simulate") => "SIMULATE",
            ("test", "run") => "RUN",
            ("script", "run") => "SCRIPT",
            ("device", "exec") => "DEVICE",
            ("recipe", "validate") => "VALIDATE",
            ("spec", "validate") => "VALIDATE",
            _ => command.Action.ToUpperInvariant()
        };
    }

    private static bool HasFlag(ParsedCommand command, string flagName)
    {
        return command.Flags.Contains(flagName);
    }

    private static string ResolveFirstNonEmpty(params string[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return string.Empty;
    }

    private static string GetDictionaryValue(IReadOnlyDictionary<string, string> values, string key)
    {
        return values.TryGetValue(key, out var value) ? value : string.Empty;
    }

    private static string FormatDisplayValue(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "N/A" : value;
    }

    private static string BuildSpecSummary(SpecEvaluationResult specResult)
    {
        return specResult.RuleType switch
        {
            "Range" => $"{FormatDecimal(specResult.Minimum)}..{FormatDecimal(specResult.Maximum)}",
            "Regex" => $"Regex {specResult.Pattern}",
            "GreaterThan" => $"> {specResult.Expected}",
            "LessThan" => $"< {specResult.Expected}",
            "Bypass" => "Bypass",
            "Contain" => $"Contain {specResult.Expected}",
            "NotEqual" => $"!= {specResult.Expected}",
            "Equal" => $"= {specResult.Expected}",
            _ => $"{specResult.RuleType} {specResult.Expected}".Trim()
        };
    }

    private static string BuildRuleSummary(SpecEvaluationResult specResult)
    {
        var status = string.Equals(specResult.PassFail, "Passed", StringComparison.OrdinalIgnoreCase)
            ? "[PASS]"
            : "[FAIL]";
        var code = string.IsNullOrWhiteSpace(specResult.ErrorCode)
            ? string.Empty
            : $" | code={specResult.ErrorCode}";

        return $"{status} {specResult.RuleName} | {specResult.TargetKey} | {BuildSpecSummary(specResult)}{code}";
    }

    private static string FormatDecimal(decimal? value)
    {
        return value?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private sealed record ParsedCommand(
        string Category,
        string Action,
        Dictionary<string, string> Options,
        HashSet<string> Flags,
        bool ShowHelp);
}
