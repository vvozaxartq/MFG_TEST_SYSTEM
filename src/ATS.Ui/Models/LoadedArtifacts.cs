using ATS.Core.Models;

namespace ATS.Ui.Models;

internal sealed class LoadedArtifacts
{
    public string SourcePath { get; init; } = string.Empty;

    public string ResultJsonPath { get; init; } = string.Empty;

    public string StructuredLogPath { get; init; } = string.Empty;

    public string SessionLogPath { get; init; } = string.Empty;

    public TestResult? TestResult { get; init; }

    public DeviceCommandResult? DeviceCommandResult { get; init; }

    public ValidationResult? ValidationResult { get; init; }

    public IReadOnlyList<StructuredLogEntry> StructuredEntries { get; init; } = Array.Empty<StructuredLogEntry>();

    public string SessionLogText { get; init; } = string.Empty;
}
