namespace ATS.Core.Models;

public interface IAiRunAnalyzer
{
    Task<AiRunAnalysisResult> AnalyzeAsync(AiRunAnalysisRequest request, CancellationToken cancellationToken);
}
