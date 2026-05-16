using ATS.Core.Models;

namespace ATS.Application.Ai;

public sealed class AiRegressionHtmlWriter
{
    private readonly AiRegressionHtmlRenderer _renderer;

    public AiRegressionHtmlWriter()
        : this(new AiRegressionHtmlRenderer())
    {
    }

    internal AiRegressionHtmlWriter(AiRegressionHtmlRenderer renderer)
    {
        _renderer = renderer;
    }

    public async Task<string> WriteAsync(
        AiRegressionCheckResult result,
        AiAnalysisBundle baselineBundle,
        AiAnalysisBundle candidateBundle,
        string outputPath,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(baselineBundle);
        ArgumentNullException.ThrowIfNull(candidateBundle);

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

        var html = _renderer.Render(result, baselineBundle, candidateBundle);
        await File.WriteAllTextAsync(fullOutputPath, html, cancellationToken);
        return fullOutputPath;
    }
}
