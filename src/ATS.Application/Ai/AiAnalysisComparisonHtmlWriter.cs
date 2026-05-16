namespace ATS.Application.Ai;

public sealed class AiAnalysisComparisonHtmlWriter
{
    private readonly AiAnalysisComparisonHtmlRenderer _renderer;

    public AiAnalysisComparisonHtmlWriter()
        : this(new AiAnalysisComparisonHtmlRenderer())
    {
    }

    internal AiAnalysisComparisonHtmlWriter(AiAnalysisComparisonHtmlRenderer renderer)
    {
        _renderer = renderer;
    }

    public async Task<string> WriteAsync(
        AiAnalysisBundleComparison comparison,
        string outputPath,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(comparison);

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            throw new ArgumentException("HTML output path is required.", nameof(outputPath));
        }

        cancellationToken.ThrowIfCancellationRequested();

        var fullOutputPath = Path.GetFullPath(outputPath);
        var outputDirectory = Path.GetDirectoryName(fullOutputPath);
        if (!string.IsNullOrWhiteSpace(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        var html = _renderer.Render(comparison);
        await File.WriteAllTextAsync(fullOutputPath, html, cancellationToken);
        return fullOutputPath;
    }
}
