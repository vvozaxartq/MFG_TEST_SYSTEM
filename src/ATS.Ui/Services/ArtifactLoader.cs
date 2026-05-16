using System.Text.Json;
using System.Text.Json.Serialization;
using ATS.Core.Models;
using ATS.Ui.Models;

namespace ATS.Ui.Services;

internal sealed class ArtifactLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public LoadedArtifacts Load(string inputPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);

        var resolved = ResolveArtifactPaths(Path.GetFullPath(inputPath));
        var testResult = default(TestResult);
        var deviceCommandResult = default(DeviceCommandResult);
        var validationResult = default(ValidationResult);

        if (!string.IsNullOrWhiteSpace(resolved.ResultJsonPath) && File.Exists(resolved.ResultJsonPath))
        {
            var json = File.ReadAllText(resolved.ResultJsonPath);
            var kind = DetectResultKind(json);
            switch (kind)
            {
                case ResultArtifactKind.Test:
                    testResult = JsonSerializer.Deserialize<TestResult>(json, JsonOptions);
                    break;
                case ResultArtifactKind.Device:
                    deviceCommandResult = JsonSerializer.Deserialize<DeviceCommandResult>(json, JsonOptions);
                    break;
                case ResultArtifactKind.Validation:
                    validationResult = JsonSerializer.Deserialize<ValidationResult>(json, JsonOptions);
                    break;
            }

            resolved = resolved.WithFallbackPaths(
                testResult?.StructuredLogPath ?? deviceCommandResult?.StructuredLogPath ?? validationResult?.StructuredLogPath ?? string.Empty,
                testResult?.SessionLogPath ?? deviceCommandResult?.SessionLogPath ?? validationResult?.SessionLogPath ?? string.Empty);
        }

        var structuredEntries = LoadStructuredEntries(resolved.StructuredLogPath);
        var sessionLogText = LoadSessionLogText(resolved.SessionLogPath);

        return new LoadedArtifacts
        {
            SourcePath = inputPath,
            ResultJsonPath = resolved.ResultJsonPath,
            StructuredLogPath = resolved.StructuredLogPath,
            SessionLogPath = resolved.SessionLogPath,
            TestResult = testResult,
            DeviceCommandResult = deviceCommandResult,
            ValidationResult = validationResult,
            StructuredEntries = structuredEntries,
            SessionLogText = sessionLogText
        };
    }

    private static ResolvedArtifactPaths ResolveArtifactPaths(string inputPath)
    {
        if (Directory.Exists(inputPath))
        {
            return ResolveFromDirectory(inputPath);
        }

        if (!File.Exists(inputPath))
        {
            throw new FileNotFoundException($"Artifact path was not found: {inputPath}");
        }

        var extension = Path.GetExtension(inputPath);
        if (string.Equals(extension, ".jsonl", StringComparison.OrdinalIgnoreCase))
        {
            return new ResolvedArtifactPaths(
                FindSiblingResultJson(inputPath),
                inputPath,
                FindSiblingSessionLog(inputPath));
        }

        if (string.Equals(extension, ".log", StringComparison.OrdinalIgnoreCase))
        {
            return new ResolvedArtifactPaths(
                FindSiblingResultJson(inputPath),
                FindSiblingStructuredLog(inputPath),
                inputPath);
        }

        if (string.Equals(extension, ".json", StringComparison.OrdinalIgnoreCase))
        {
            return new ResolvedArtifactPaths(
                inputPath,
                FindSiblingStructuredLog(inputPath),
                FindSiblingSessionLog(inputPath));
        }

        throw new InvalidOperationException("Unsupported artifact input. Use a session folder, result.json, session.events.jsonl, or session.log.");
    }

    private static ResolvedArtifactPaths ResolveFromDirectory(string directoryPath)
    {
        var resultJsonPath = Path.Combine(directoryPath, "result.json");
        if (!File.Exists(resultJsonPath))
        {
            resultJsonPath = Directory.GetFiles(directoryPath, "*.json", SearchOption.TopDirectoryOnly)
                .FirstOrDefault(path => !path.EndsWith(".jsonl", StringComparison.OrdinalIgnoreCase))
                ?? string.Empty;
        }

        var structuredLogPath = Directory.GetFiles(directoryPath, "*.jsonl", SearchOption.TopDirectoryOnly)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault()
            ?? string.Empty;

        var sessionLogPath = Directory.GetFiles(directoryPath, "*.log", SearchOption.TopDirectoryOnly)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault()
            ?? string.Empty;

        return new ResolvedArtifactPaths(resultJsonPath, structuredLogPath, sessionLogPath);
    }

    private static string FindSiblingResultJson(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath) ?? Directory.GetCurrentDirectory();
        var candidate = Path.Combine(directory, "result.json");
        if (File.Exists(candidate))
        {
            return candidate;
        }

        return Directory.GetFiles(directory, "*.json", SearchOption.TopDirectoryOnly)
            .FirstOrDefault(path => !path.EndsWith(".jsonl", StringComparison.OrdinalIgnoreCase))
            ?? string.Empty;
    }

    private static string FindSiblingStructuredLog(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath) ?? Directory.GetCurrentDirectory();
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        if (fileNameWithoutExtension.EndsWith(".events", StringComparison.OrdinalIgnoreCase))
        {
            return filePath;
        }

        return Directory.GetFiles(directory, "*.jsonl", SearchOption.TopDirectoryOnly)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault()
            ?? string.Empty;
    }

    private static string FindSiblingSessionLog(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath) ?? Directory.GetCurrentDirectory();
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        if (fileNameWithoutExtension.EndsWith(".events", StringComparison.OrdinalIgnoreCase))
        {
            var stem = fileNameWithoutExtension[..^".events".Length];
            var candidate = Path.Combine(directory, $"{stem}.log");
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return Directory.GetFiles(directory, "*.log", SearchOption.TopDirectoryOnly)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault()
            ?? string.Empty;
    }

    private static ResultArtifactKind DetectResultKind(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (root.TryGetProperty("Steps", out _))
        {
            return ResultArtifactKind.Test;
        }

        if (root.TryGetProperty("ValidationType", out _))
        {
            return ResultArtifactKind.Validation;
        }

        if (root.TryGetProperty("DeviceName", out _) && root.TryGetProperty("Response", out _))
        {
            return ResultArtifactKind.Device;
        }

        return ResultArtifactKind.Unknown;
    }

    private static IReadOnlyList<StructuredLogEntry> LoadStructuredEntries(string structuredLogPath)
    {
        if (string.IsNullOrWhiteSpace(structuredLogPath) || !File.Exists(structuredLogPath))
        {
            return Array.Empty<StructuredLogEntry>();
        }

        return File.ReadAllLines(structuredLogPath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonSerializer.Deserialize<StructuredLogEntry>(line, JsonOptions))
            .OfType<StructuredLogEntry>()
            .ToList();
    }

    private static string LoadSessionLogText(string sessionLogPath)
    {
        return !string.IsNullOrWhiteSpace(sessionLogPath) && File.Exists(sessionLogPath)
            ? File.ReadAllText(sessionLogPath)
            : string.Empty;
    }
}

internal readonly record struct ResolvedArtifactPaths(
    string ResultJsonPath,
    string StructuredLogPath,
    string SessionLogPath)
{
    public ResolvedArtifactPaths WithFallbackPaths(string structuredLogPath, string sessionLogPath)
    {
        return new ResolvedArtifactPaths(
            ResultJsonPath,
            string.IsNullOrWhiteSpace(StructuredLogPath) ? structuredLogPath : StructuredLogPath,
            string.IsNullOrWhiteSpace(SessionLogPath) ? sessionLogPath : SessionLogPath);
    }
}

internal enum ResultArtifactKind
{
    Unknown,
    Test,
    Device,
    Validation
}
