namespace ATS.Core.Models;

public sealed record AiProviderResponse
{
    public string ProviderName { get; init; } = string.Empty;

    public string BundleSchemaVersion { get; init; } = string.Empty;

    public string PrimaryCategory { get; init; } = string.Empty;

    public string PrimaryCause { get; init; } = string.Empty;

    public string Summary { get; init; } = string.Empty;

    public List<string> Highlights { get; init; } = new();

    public List<string> RecommendedActions { get; init; } = new();
}
