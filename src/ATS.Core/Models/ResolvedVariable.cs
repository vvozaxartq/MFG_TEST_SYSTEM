namespace ATS.Core.Models;

public sealed class ResolvedVariable
{
    public string RequestedName { get; init; } = string.Empty;

    public string ResolvedName { get; init; } = string.Empty;

    public string Name => ResolvedName;

    public string Value { get; init; } = string.Empty;

    public VariableScope Scope { get; init; }

    public string Source { get; init; } = string.Empty;
}
