using ATS.Application.Execution;
using ATS.Application.Recipes;
using ATS.Application.Specs;
using ATS.Core.Models;

return await CliProgram.RunAsync(args);

internal static class CliProgram
{
    private const int ExitSuccess = 0;
    private const int ExitTestFailure = 1;
    private const int ExitInvalidArguments = 2;
    private const int ExitValidationFailure = 3;
    private const int ExitRuntimeError = 4;

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
        var runner = new TestRunner();
        var result = await runner.RunAsync(
            new TestRunRequest(
                $"{command.Category} {command.Action}",
                recipePath,
                specPath,
                outputDirectory,
                string.Empty),
            CancellationToken.None);

        PrintTestResult(result, outputDirectory);
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
        var runner = new ScriptRunner();
        var result = await runner.RunAsync(
            recipePath,
            specPath,
            scriptName,
            outputDirectory,
            CancellationToken.None);

        PrintTestResult(result, outputDirectory);
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
        var executor = new DeviceExecutor();
        var result = await executor.ExecuteAsync(
            new DeviceExecutionRequest("device exec", deviceCommand, outputDirectory),
            CancellationToken.None);

        Console.WriteLine($"Device: {result.DeviceName}");
        Console.WriteLine($"Session: {result.SessionId}");
        Console.WriteLine($"Command: {result.Command}");
        Console.WriteLine($"Response: {result.Response}");
        Console.WriteLine($"Status: {result.Status}");
        Console.WriteLine($"Message: {result.Message}");
        PrintArtifactPaths(outputDirectory);
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
        var service = new RecipeValidationService();
        var result = await service.ValidateAsync(recipePath, specPath, outputDirectory, CancellationToken.None);

        PrintValidationResult(result, outputDirectory);
        return string.Equals(result.Status, "Passed", StringComparison.OrdinalIgnoreCase)
            ? ExitSuccess
            : ExitValidationFailure;
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
        var service = new SpecValidationService();
        var result = await service.ValidateAsync(specPath, outputDirectory, CancellationToken.None);

        PrintValidationResult(result, outputDirectory);
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

    private static void PrintTestResult(TestResult result, string outputDirectory)
    {
        Console.WriteLine($"Command: {result.CommandName}");
        Console.WriteLine($"Recipe: {result.RecipeName}");
        Console.WriteLine($"Device: {result.DeviceName}");
        Console.WriteLine($"Session: {result.SessionId}");

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
                Console.WriteLine(
                    $"  rule {specResult.RuleName} -> {specResult.PassFail} ({specResult.TargetKey} = {specResult.ActualValue})");
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
        PrintArtifactPaths(outputDirectory);
    }

    private static void PrintValidationResult(ValidationResult result, string outputDirectory)
    {
        Console.WriteLine($"Command: {result.CommandName}");
        Console.WriteLine($"Target: {result.TargetPath}");

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
        PrintArtifactPaths(outputDirectory);
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
        command = new ParsedCommand(string.Empty, string.Empty, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase), false);
        errorMessage = string.Empty;

        if (args.Length < 2)
        {
            errorMessage = "Command requires <category> <action>.";
            return false;
        }

        var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
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

            if (index + 1 >= args.Length || args[index + 1].StartsWith("--", StringComparison.Ordinal))
            {
                errorMessage = $"Option '{argument}' requires a value.";
                return false;
            }

            options[argument[2..]] = args[index + 1];
            index++;
        }

        command = new ParsedCommand(
            args[0].ToLowerInvariant(),
            args[1].ToLowerInvariant(),
            options,
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

    private static void PrintRootUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  ats test simulate --recipe <file> [--spec <file>] [--output <directory>]");
        Console.WriteLine("  ats test run --recipe <file> [--spec <file>] [--output <directory>]");
        Console.WriteLine("  ats script run --recipe <file> --script <name> [--spec <file>] [--output <directory>]");
        Console.WriteLine("  ats device exec --command <text> [--output <directory>]");
        Console.WriteLine("  ats recipe validate --recipe <file> [--spec <file>] [--output <directory>]");
        Console.WriteLine("  ats spec validate --spec <file> [--output <directory>]");
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
            ("test", "simulate") => "ats test simulate --recipe <file> [--spec <file>] [--output <directory>]",
            ("test", "run") => "ats test run --recipe <file> [--spec <file>] [--output <directory>]",
            ("script", "run") => "ats script run --recipe <file> --script <name> [--spec <file>] [--output <directory>]",
            ("device", "exec") => "ats device exec --command <text> [--output <directory>]",
            ("recipe", "validate") => "ats recipe validate --recipe <file> [--spec <file>] [--output <directory>]",
            ("spec", "validate") => "ats spec validate --spec <file> [--output <directory>]",
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

    private static void PrintArtifactPaths(string outputDirectory)
    {
        var normalizedOutputDirectory = Path.GetFullPath(outputDirectory);
        Console.WriteLine($"result.json: {Path.Combine(normalizedOutputDirectory, "result.json")}");
        Console.WriteLine($"result.csv: {Path.Combine(normalizedOutputDirectory, "result.csv")}");
        Console.WriteLine($"session.log: {Path.Combine(normalizedOutputDirectory, "session.log")}");
    }

    private sealed record ParsedCommand(
        string Category,
        string Action,
        Dictionary<string, string> Options,
        bool ShowHelp);
}
