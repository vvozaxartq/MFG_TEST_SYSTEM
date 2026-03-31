using ATS.Core.Models;

namespace ATS.Application.Flow;

public sealed class FlowExecutionResult
{
    public string DeviceName { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public List<ScriptResult> Scripts { get; init; } = new();

    public List<string> Errors { get; init; } = new();
}
