namespace ATS.Core.Models;

public sealed class SessionArtifactManifest
{
    public string OutputDirectory { get; init; } = string.Empty;

    public string ResultJsonPath { get; init; } = string.Empty;

    public string ResultCsvPath { get; init; } = string.Empty;

    public string SessionLogPath { get; init; } = string.Empty;

    public string StructuredLogPath { get; init; } = string.Empty;
}
