using System.Text.Json;
using System.Text.Json.Serialization;
using ATS.Core.Models;

namespace ATS.Application.Ai;

public sealed class AiAnalysisBundleWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public async Task<string> WriteAsync(
        AiAnalysisBundle bundle,
        string outputPath,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(bundle);

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            throw new ArgumentException("Bundle output path is required.", nameof(outputPath));
        }

        cancellationToken.ThrowIfCancellationRequested();

        var fullOutputPath = Path.GetFullPath(outputPath);
        var outputDirectory = Path.GetDirectoryName(fullOutputPath);
        if (!string.IsNullOrWhiteSpace(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        var json = JsonSerializer.Serialize(bundle, JsonOptions);
        await File.WriteAllTextAsync(fullOutputPath, json, cancellationToken);
        return fullOutputPath;
    }
}
