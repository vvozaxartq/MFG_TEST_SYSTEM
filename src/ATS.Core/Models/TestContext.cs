namespace ATS.Core.Models;

public sealed class TestContext
{
    private readonly List<string> _logs = new();

    public TestContext(
        string commandName,
        string outputDirectory,
        string recipePath = "",
        string specPath = "",
        string selectedScriptName = "")
    {
        CommandName = commandName;
        RecipePath = recipePath;
        SpecPath = specPath;
        SelectedScriptName = selectedScriptName;
        OutputDirectory = outputDirectory;
        SessionId = Guid.NewGuid().ToString("N");
        StartedAtUtc = DateTimeOffset.UtcNow;
    }

    public string SessionId { get; }

    public string CommandName { get; }

    public string RecipePath { get; }

    public string SpecPath { get; }

    public string SelectedScriptName { get; }

    public string OutputDirectory { get; }

    public DateTimeOffset StartedAtUtc { get; }

    public DataCollection Data { get; } = new();

    public IReadOnlyList<string> Logs => _logs;

    public void Log(string message)
    {
        _logs.Add($"{DateTimeOffset.UtcNow:O} {message}");
    }

    public void LogError(string message)
    {
        Log($"ERROR {message}");
    }
}
