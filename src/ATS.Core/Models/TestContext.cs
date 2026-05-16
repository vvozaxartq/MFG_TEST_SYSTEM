namespace ATS.Core.Models;

public sealed class TestContext
{
    private readonly object _logSync = new();
    private readonly List<string> _logs = new();
    private long _structuredLogSequence;
    private IStructuredLogSink? _structuredLogSink;

    public TestContext(
        string sessionId,
        string commandName,
        SessionArtifactPaths artifactPaths,
        SessionArtifactOptions artifactOptions,
        RunInputModel runInput,
        string recipePath = "",
        string specPath = "",
        string selectedScriptName = "")
    {
        SessionId = sessionId;
        CommandName = commandName;
        RecipePath = recipePath;
        SpecPath = specPath;
        SelectedScriptName = selectedScriptName;
        ArtifactPaths = artifactPaths;
        ArtifactOptions = artifactOptions;
        RunInput = runInput;
        OutputDirectory = artifactPaths.OutputDirectory;
        StartedAtUtc = DateTimeOffset.UtcNow;

        Directory.CreateDirectory(OutputDirectory);
        Directory.CreateDirectory(Path.GetDirectoryName(ArtifactPaths.SessionLogPath) ?? OutputDirectory);
        Directory.CreateDirectory(Path.GetDirectoryName(ArtifactPaths.StructuredLogPath) ?? OutputDirectory);
        if (!File.Exists(ArtifactPaths.SessionLogPath))
        {
            File.WriteAllText(ArtifactPaths.SessionLogPath, string.Empty);
        }

        if (!File.Exists(ArtifactPaths.StructuredLogPath))
        {
            File.WriteAllText(ArtifactPaths.StructuredLogPath, string.Empty);
        }
    }

    public string SessionId { get; }

    public string CommandName { get; }

    public string RecipePath { get; }

    public string SpecPath { get; }

    public string SelectedScriptName { get; }

    public string OutputDirectory { get; }

    public SessionArtifactPaths ArtifactPaths { get; }

    public SessionArtifactOptions ArtifactOptions { get; }

    public RunInputModel RunInput { get; }

    public DateTimeOffset StartedAtUtc { get; }

    public DataCollection Data { get; } = new();

    public IReadOnlyList<string> Logs
    {
        get
        {
            lock (_logSync)
            {
                return _logs.ToList();
            }
        }
    }

    public void Log(string message, string itemName = "")
    {
        WriteLog("INFO", message, itemName, StructuredLogEntryType.Message);
    }

    public void LogError(string message, string itemName = "")
    {
        WriteLog("ERROR", message, itemName, StructuredLogEntryType.Error);
    }

    public void SetStructuredLogSink(IStructuredLogSink structuredLogSink)
    {
        _structuredLogSink = structuredLogSink;
    }

    public void LogEvent(
        string level,
        StructuredLogEntryType entryType,
        string message,
        string itemName = "",
        string stepName = "",
        string dutId = "",
        string fullKey = "",
        string status = "",
        IReadOnlyDictionary<string, object?>? data = null)
    {
        WriteLog(level, message, itemName, entryType, stepName, dutId, fullKey, status, data);
    }

    private void WriteLog(
        string level,
        string message,
        string itemName,
        StructuredLogEntryType entryType,
        string stepName = "",
        string dutId = "",
        string fullKey = "",
        string status = "",
        IReadOnlyDictionary<string, object?>? data = null)
    {
        var localTimestamp = DateTimeOffset.Now;
        var elapsed = DateTimeOffset.UtcNow - StartedAtUtc;
        var logLine =
            $"{localTimestamp:yyyy-MM-dd HH:mm:ss.fff} | +{FormatElapsed(elapsed)} | {level} | item={ResolveItemName(itemName)} | {message}";
        var structuredLogEntry = new StructuredLogEntry
        {
            Sequence = Interlocked.Increment(ref _structuredLogSequence),
            SessionId = SessionId,
            TimestampUtc = DateTimeOffset.UtcNow,
            ElapsedMs = (long)Math.Max(0, elapsed.TotalMilliseconds),
            Level = level,
            EntryType = entryType,
            ItemName = ResolveItemName(itemName),
            StepName = string.IsNullOrWhiteSpace(stepName) ? ResolveItemName(itemName) : stepName,
            DutId = dutId,
            FullKey = fullKey,
            Status = status,
            Message = message,
            Data = data is null
                ? new Dictionary<string, object?>()
                : new Dictionary<string, object?>(data, StringComparer.OrdinalIgnoreCase)
        };

        lock (_logSync)
        {
            _logs.Add(logLine);
            File.AppendAllLines(ArtifactPaths.SessionLogPath, new[] { logLine });
            _structuredLogSink?.Append(ArtifactPaths.StructuredLogPath, structuredLogEntry);
        }
    }

    public void AppendLogBlock(IEnumerable<string> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);

        var bufferedLines = lines.ToList();
        if (bufferedLines.Count == 0)
        {
            return;
        }

        lock (_logSync)
        {
            _logs.AddRange(bufferedLines);
            File.AppendAllLines(ArtifactPaths.SessionLogPath, bufferedLines);
        }
    }

    private static string ResolveItemName(string itemName)
    {
        return string.IsNullOrWhiteSpace(itemName)
            ? "SESSION"
            : itemName;
    }

    private static string FormatElapsed(TimeSpan elapsed)
    {
        return $"{(int)elapsed.TotalHours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}.{elapsed.Milliseconds:000}";
    }
}
