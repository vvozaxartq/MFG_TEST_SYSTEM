namespace ATS.Core.Models;

public sealed class VariableContext
{
    public Dictionary<string, string> GlobalVariables { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, string> GlobalSources { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public DutContext DutContext { get; init; } = new();

    public Dictionary<string, string> StepVariables { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, string> StepSources { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
