using ATS.Core.Models;

namespace ATS.Application.Execution;

public sealed class SessionFactory
{
    public TestContext Create(
        string commandName,
        string outputDirectory,
        string recipePath = "",
        string specPath = "",
        string selectedScriptName = "")
    {
        var normalizedOutputDirectory = Path.GetFullPath(
            string.IsNullOrWhiteSpace(outputDirectory)
                ? Directory.GetCurrentDirectory()
                : outputDirectory);

        Directory.CreateDirectory(normalizedOutputDirectory);

        return new TestContext(
            commandName,
            normalizedOutputDirectory,
            NormalizePath(recipePath),
            NormalizePath(specPath),
            selectedScriptName);
    }

    private static string NormalizePath(string path)
    {
        return string.IsNullOrWhiteSpace(path)
            ? string.Empty
            : Path.GetFullPath(path);
    }
}
