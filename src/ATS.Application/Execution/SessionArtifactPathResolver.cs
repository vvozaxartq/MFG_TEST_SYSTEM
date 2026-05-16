using System.Text.RegularExpressions;
using ATS.Core.Models;

namespace ATS.Application.Execution;

internal sealed class SessionArtifactPathResolver
{
    private static readonly Regex VariablePattern = new("%(?<name>[A-Za-z0-9_]+)%", RegexOptions.Compiled);

    public SessionArtifactPaths Resolve(
        string baseOutputDirectory,
        string commandName,
        string recipePath,
        string specPath,
        string selectedScriptName,
        string sessionId,
        SessionArtifactOptions? options,
        RunInputModel runInput)
    {
        var normalizedBaseDirectory = Path.GetFullPath(
            string.IsNullOrWhiteSpace(baseOutputDirectory)
                ? Directory.GetCurrentDirectory()
                : baseOutputDirectory);

        var variables = BuildVariables(commandName, recipePath, specPath, selectedScriptName, sessionId, options, runInput);
        var outputDirectory = ResolveDirectoryPath(
            normalizedBaseDirectory,
            string.IsNullOrWhiteSpace(options?.OutputDirectoryTemplate)
                ? string.Empty
                : options.OutputDirectoryTemplate,
            variables);

        Directory.CreateDirectory(outputDirectory);

        var resultJsonPath = EnsureUniqueFilePath(
            ResolveFilePath(outputDirectory, options?.ResultJsonTemplate, "result.json", variables),
            sessionId);
        var resultCsvPath = EnsureUniqueFilePath(
            ResolveFilePath(outputDirectory, options?.ResultCsvTemplate, "result.csv", variables),
            sessionId);
        var sessionLogPath = EnsureUniqueFilePath(
            ResolveFilePath(outputDirectory, options?.SessionLogTemplate, "session.log", variables),
            sessionId);
        var structuredLogPath = EnsureUniqueFilePath(
            ResolveFilePath(outputDirectory, options?.StructuredLogTemplate, "session.events.jsonl", variables),
            sessionId);

        EnsureParentDirectory(resultJsonPath);
        EnsureParentDirectory(resultCsvPath);
        EnsureParentDirectory(sessionLogPath);
        EnsureParentDirectory(structuredLogPath);

        return new SessionArtifactPaths
        {
            OutputDirectory = outputDirectory,
            ResultJsonPath = resultJsonPath,
            ResultCsvPath = resultCsvPath,
            SessionLogPath = sessionLogPath,
            StructuredLogPath = structuredLogPath
        };
    }

    private static Dictionary<string, string> BuildVariables(
        string commandName,
        string recipePath,
        string specPath,
        string selectedScriptName,
        string sessionId,
        SessionArtifactOptions? options,
        RunInputModel runInput)
    {
        var now = DateTimeOffset.Now;
        var variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["CurTime"] = now.ToString("yyyyMMdd_HHmmssfff"),
            ["CurDate"] = now.ToString("yyyyMMdd"),
            ["CurDateTime"] = now.ToString("yyyyMMdd_HHmmss"),
            ["SessionId"] = sessionId,
            ["CommandName"] = commandName,
            ["RecipeFileName"] = Path.GetFileName(recipePath),
            ["RecipeFileNameNoExt"] = Path.GetFileNameWithoutExtension(recipePath),
            ["SpecFileName"] = Path.GetFileName(specPath),
            ["SpecFileNameNoExt"] = Path.GetFileNameWithoutExtension(specPath),
            ["Recipe"] = NormalizeDefinitionName(recipePath, "recipe"),
            ["Spec"] = NormalizeDefinitionName(specPath, "spec"),
            ["SelectedScriptName"] = selectedScriptName,
            ["ComputerName"] = Environment.MachineName,
            ["UserName"] = Environment.UserName,
            ["SN"] = runInput.SerialNumber,
            ["ProductSN"] = runInput.SerialNumber,
            ["Station"] = runInput.Station,
            ["Mode"] = runInput.Mode
        };

        foreach (var pair in runInput.Values)
        {
            variables[pair.Key] = pair.Value;
        }

        if (options is not null)
        {
            foreach (var pair in options.Variables)
            {
                variables[pair.Key] = pair.Value;
            }
        }

        return variables;
    }

    private static string ResolveDirectoryPath(
        string baseOutputDirectory,
        string template,
        IReadOnlyDictionary<string, string> variables)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return baseOutputDirectory;
        }

        var resolvedTemplate = ResolveTemplate(template, variables);
        return Path.IsPathRooted(resolvedTemplate)
            ? Path.GetFullPath(resolvedTemplate)
            : Path.GetFullPath(Path.Combine(baseOutputDirectory, resolvedTemplate));
    }

    private static string ResolveFilePath(
        string outputDirectory,
        string? template,
        string defaultFileName,
        IReadOnlyDictionary<string, string> variables)
    {
        var resolvedTemplate = ResolveTemplate(
            string.IsNullOrWhiteSpace(template) ? defaultFileName : template,
            variables);

        return Path.IsPathRooted(resolvedTemplate)
            ? Path.GetFullPath(resolvedTemplate)
            : Path.GetFullPath(Path.Combine(outputDirectory, resolvedTemplate));
    }

    private static string ResolveTemplate(string template, IReadOnlyDictionary<string, string> variables)
    {
        var resolved = VariablePattern.Replace(
            template,
            match => ResolveToken(match.Groups["name"].Value, variables));

        return Regex.Replace(
            resolved,
            "\\{(?<name>[^{}]+)\\}",
            match => ResolveToken(match.Groups["name"].Value, variables));
    }

    private static string SanitizePathValue(string value)
    {
        var invalidCharacters = Path.GetInvalidFileNameChars().Concat(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }).Distinct().ToArray();
        var sanitized = value;

        foreach (var character in invalidCharacters)
        {
            sanitized = sanitized.Replace(character, '_');
        }

        return sanitized.Trim();
    }

    private static void EnsureParentDirectory(string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    private static string ResolveToken(string token, IReadOnlyDictionary<string, string> variables)
    {
        if (TryResolveVariable(token, variables, out var value))
        {
            return value;
        }

        if (TryResolveDateToken(token, out value))
        {
            return value;
        }

        var environmentValue = Environment.GetEnvironmentVariable(token);
        return string.IsNullOrWhiteSpace(environmentValue)
            ? "NA"
            : SanitizePathValue(environmentValue);
    }

    private static bool TryResolveVariable(
        string token,
        IReadOnlyDictionary<string, string> variables,
        out string value)
    {
        if (variables.TryGetValue(token, out var rawValue) && !string.IsNullOrWhiteSpace(rawValue))
        {
            value = SanitizePathValue(rawValue);
            return true;
        }

        value = string.Empty;
        return false;
    }

    private static bool TryResolveDateToken(string token, out string value)
    {
        var format = token.StartsWith("Now:", StringComparison.OrdinalIgnoreCase)
            ? token["Now:".Length..]
            : token;

        if (string.IsNullOrWhiteSpace(format) || !LooksLikeDateTimeFormat(format))
        {
            value = string.Empty;
            return false;
        }

        value = SanitizePathValue(DateTimeOffset.Now.ToString(format));
        return true;
    }

    private static bool LooksLikeDateTimeFormat(string token)
    {
        if (token.IndexOfAny(new[] { 'y', 'M', 'd', 'H', 'h', 'm', 's', 'f' }) < 0)
        {
            return false;
        }

        return token.All(character =>
            character is 'y' or 'M' or 'd' or 'H' or 'h' or 'm' or 's' or 'f' ||
            character is '_' or '-' or '.' or ':' or ' ' or '/' or '\\');
    }

    private static string EnsureUniqueFilePath(string path, string sessionId)
    {
        if (!File.Exists(path))
        {
            return path;
        }

        var directory = Path.GetDirectoryName(path) ?? Directory.GetCurrentDirectory();
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
        var extension = Path.GetExtension(path);
        var candidate = Path.Combine(directory, $"{fileNameWithoutExtension}_{sessionId}{extension}");

        if (!File.Exists(candidate))
        {
            return candidate;
        }

        var counter = 1;
        while (true)
        {
            candidate = Path.Combine(directory, $"{fileNameWithoutExtension}_{sessionId}_{counter}{extension}");
            if (!File.Exists(candidate))
            {
                return candidate;
            }

            counter++;
        }
    }

    private static string NormalizeDefinitionName(string path, string suffix)
    {
        var fileName = Path.GetFileNameWithoutExtension(path);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return string.Empty;
        }

        var normalizedSuffix = $".{suffix}";
        return fileName.EndsWith(normalizedSuffix, StringComparison.OrdinalIgnoreCase)
            ? fileName[..^normalizedSuffix.Length]
            : fileName;
    }
}
