namespace ATS.Core.Models;

public interface IAiBundleAnalysisProvider
{
    string Name { get; }

    Task<AiProviderResponse> AnalyzeAsync(AiProviderRequest request, CancellationToken cancellationToken);
}
