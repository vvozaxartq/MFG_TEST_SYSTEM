using ATS.Core.Models;

namespace ATS.Application.Ai;

public sealed class AiAnalysisHtmlWriter
{
    private readonly AiAnalysisHtmlRenderer _renderer;

    public AiAnalysisHtmlWriter()
        : this(new AiAnalysisHtmlRenderer())
    {
    }

    internal AiAnalysisHtmlWriter(AiAnalysisHtmlRenderer renderer)
    {
        _renderer = renderer;
    }

    public async Task<string> WriteAsync(
        AiAnalysisBundle bundle,
        string outputPath,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(bundle);

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

        var html = _renderer.Render(bundle);
        await File.WriteAllTextAsync(fullOutputPath, html, cancellationToken);
        return fullOutputPath;
    }
}
