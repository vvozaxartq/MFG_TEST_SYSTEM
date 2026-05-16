namespace ATS.Core.Models;

public sealed record AiProviderRequest
{
    public AiAnalysisBundle Bundle { get; init; } = new();
}
