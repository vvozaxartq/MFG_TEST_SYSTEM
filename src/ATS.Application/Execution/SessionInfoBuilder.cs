using ATS.Core.Models;

namespace ATS.Application.Execution;

internal static class SessionInfoBuilder
{
    public static SessionInfo Build(
        TestContext context,
        string recipeName,
        string finalStatus,
        DateTimeOffset completedAtUtc,
        string primaryDutId = "default")
    {
        return new SessionInfo
        {
            SessionId = context.SessionId,
            CommandName = context.CommandName,
            RecipeName = recipeName,
            RecipePath = context.RecipePath,
            SpecPath = context.SpecPath,
            SerialNumber = context.RunInput.SerialNumber,
            Station = context.RunInput.Station,
            Mode = context.RunInput.Mode,
            FinalStatus = finalStatus,
            PrimaryDutId = primaryDutId,
            Inputs = BuildInputs(context.RunInput),
            Artifacts = new SessionArtifactManifest
            {
                OutputDirectory = context.OutputDirectory,
                ResultJsonPath = context.ArtifactPaths.ResultJsonPath,
                ResultCsvPath = context.ArtifactPaths.ResultCsvPath,
                SessionLogPath = context.ArtifactPaths.SessionLogPath,
                StructuredLogPath = context.ArtifactPaths.StructuredLogPath
            },
            StartedAtUtc = context.StartedAtUtc,
            CompletedAtUtc = completedAtUtc
        };
    }

    private static Dictionary<string, string> BuildInputs(RunInputModel runInput)
    {
        var inputs = new Dictionary<string, string>(runInput.Values, StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(runInput.SerialNumber))
        {
            inputs["SN"] = runInput.SerialNumber;
        }

        if (!string.IsNullOrWhiteSpace(runInput.Station))
        {
            inputs["Station"] = runInput.Station;
        }

        if (!string.IsNullOrWhiteSpace(runInput.Mode))
        {
            inputs["Mode"] = runInput.Mode;
        }

        return inputs;
    }
}
