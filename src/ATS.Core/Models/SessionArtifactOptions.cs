namespace ATS.Core.Models;

public sealed class SessionArtifactOptions
{
    public string OutputDirectoryTemplate { get; init; } = string.Empty;

    public string ResultJsonTemplate { get; init; } = "result.json";

    public string ResultCsvTemplate { get; init; } = "result.csv";

    public string SessionLogTemplate { get; init; } = "session_%SessionId%.log";

    public string StructuredLogTemplate { get; init; } = "session_%SessionId%.events.jsonl";

    public Dictionary<string, string> Variables { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
