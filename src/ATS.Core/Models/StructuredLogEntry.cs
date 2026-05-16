namespace ATS.Core.Models;

public sealed class StructuredLogEntry
{
    public const string CurrentSchemaVersion = "ats.structured-log.v1";

    public string SchemaVersion { get; init; } = CurrentSchemaVersion;

    public long Sequence { get; init; }

    public string SessionId { get; init; } = string.Empty;

    public DateTimeOffset TimestampUtc { get; init; }

    public long ElapsedMs { get; init; }

    public string Level { get; init; } = string.Empty;

    public StructuredLogEntryType EntryType { get; init; }

    public string ItemName { get; init; } = string.Empty;

    public string StepName { get; init; } = string.Empty;

    public string DutId { get; init; } = string.Empty;

    public string FullKey { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public Dictionary<string, object?> Data { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
