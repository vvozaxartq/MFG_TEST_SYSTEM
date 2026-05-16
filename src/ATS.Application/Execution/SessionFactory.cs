using ATS.Core.Models;

namespace ATS.Application.Execution;

public sealed class SessionFactory
{
    private readonly SessionArtifactPathResolver _artifactPathResolver = new();
    private readonly StructuredLogWriter _structuredLogWriter = new();

    public TestContext Create(
        string commandName,
        string outputDirectory,
        SessionArtifactOptions? artifactOptions = null,
        RunInputModel? runInput = null,
        string recipePath = "",
        string specPath = "",
        string selectedScriptName = "")
    {
        var sessionId = Guid.NewGuid().ToString("N");
        var normalizedRecipePath = NormalizePath(recipePath);
        var normalizedSpecPath = NormalizePath(specPath);
        var resolvedArtifactOptions = artifactOptions ?? new SessionArtifactOptions();
        var resolvedRunInput = NormalizeRunInput(runInput);
        var artifactPaths = _artifactPathResolver.Resolve(
            outputDirectory,
            commandName,
            normalizedRecipePath,
            normalizedSpecPath,
            selectedScriptName,
            sessionId,
            resolvedArtifactOptions,
            resolvedRunInput);

        var context = new TestContext(
            sessionId,
            commandName,
            artifactPaths,
            resolvedArtifactOptions,
            resolvedRunInput,
            normalizedRecipePath,
            normalizedSpecPath,
            selectedScriptName);
        context.SetStructuredLogSink(_structuredLogWriter);

        context.AppendLogBlock(SessionLogReportBuilder.BuildSessionHeader(context));
        context.LogEvent(
            "INFO",
            StructuredLogEntryType.SessionStarted,
            $"Session '{context.SessionId}' created for '{commandName}'.",
            commandName,
            status: "Started",
            data: new Dictionary<string, object?>
            {
                ["commandName"] = commandName,
                ["recipePath"] = normalizedRecipePath,
                ["specPath"] = normalizedSpecPath,
                ["selectedScriptName"] = selectedScriptName,
                ["outputDirectory"] = context.OutputDirectory,
                ["sessionLogPath"] = context.ArtifactPaths.SessionLogPath,
                ["structuredLogPath"] = context.ArtifactPaths.StructuredLogPath
            });

        foreach (var pair in BuildInputEvents(context.RunInput))
        {
            context.LogEvent(
                "INFO",
                StructuredLogEntryType.InputCaptured,
                $"Input '{pair.Key}' = '{pair.Value}'.",
                commandName,
                status: "Captured",
                data: new Dictionary<string, object?>
                {
                    ["name"] = pair.Key,
                    ["value"] = pair.Value
                });
        }

        return context;
    }

    private static string NormalizePath(string path)
    {
        return string.IsNullOrWhiteSpace(path)
            ? string.Empty
            : Path.GetFullPath(path);
    }

    private static RunInputModel NormalizeRunInput(RunInputModel? runInput)
    {
        if (runInput is null)
        {
            return new RunInputModel();
        }

        return new RunInputModel
        {
            SerialNumber = runInput.SerialNumber?.Trim() ?? string.Empty,
            Station = runInput.Station?.Trim() ?? string.Empty,
            Mode = runInput.Mode?.Trim() ?? string.Empty,
            Values = new Dictionary<string, string>(runInput.Values, StringComparer.OrdinalIgnoreCase)
        };
    }

    private static IReadOnlyDictionary<string, string> BuildInputEvents(RunInputModel runInput)
    {
        var values = new Dictionary<string, string>(runInput.Values, StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(runInput.SerialNumber))
        {
            values["SN"] = runInput.SerialNumber;
        }

        if (!string.IsNullOrWhiteSpace(runInput.Station))
        {
            values["Station"] = runInput.Station;
        }

        if (!string.IsNullOrWhiteSpace(runInput.Mode))
        {
            values["Mode"] = runInput.Mode;
        }

        return values;
    }
}
