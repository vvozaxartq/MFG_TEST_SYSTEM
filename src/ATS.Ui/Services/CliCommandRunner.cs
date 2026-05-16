using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace ATS.Ui.Services;

internal sealed class CliCommandRunner
{
    private static readonly Regex ArtifactLinePattern = new(
        "^(?<name>result\\.json|result\\.csv|session\\.log|session\\.events\\.jsonl):\\s*(?<path>.+)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly string _repositoryRoot;
    private readonly string _cliProjectPath;

    public CliCommandRunner(string repositoryRoot)
    {
        _repositoryRoot = repositoryRoot;
        _cliProjectPath = Path.Combine(repositoryRoot, "src", "ATS.Cli", "ATS.Cli.csproj");
    }

    public async Task<CliRunResult> RunAsync(
        string cliArguments,
        IProgress<string> outputProgress,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cliArguments);
        ArgumentNullException.ThrowIfNull(outputProgress);

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{_cliProjectPath}\" -- {cliArguments}",
            WorkingDirectory = _repositoryRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true
        };

        var outputBuilder = new StringBuilder();
        var artifactPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var completionSource = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

        void HandleLine(string line, bool isError)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return;
            }

            var rendered = isError ? $"[stderr] {line}" : line;
            lock (outputBuilder)
            {
                outputBuilder.AppendLine(rendered);
            }

            outputProgress.Report(rendered);

            var match = ArtifactLinePattern.Match(line.Trim());
            if (match.Success)
            {
                artifactPaths[match.Groups["name"].Value] = match.Groups["path"].Value.Trim();
            }
        }

        process.OutputDataReceived += (_, eventArgs) =>
        {
            if (eventArgs.Data is not null)
            {
                HandleLine(eventArgs.Data, isError: false);
            }
        };

        process.ErrorDataReceived += (_, eventArgs) =>
        {
            if (eventArgs.Data is not null)
            {
                HandleLine(eventArgs.Data, isError: true);
            }
        };

        process.Exited += (_, _) => completionSource.TrySetResult(process.ExitCode);

        if (!process.Start())
        {
            throw new InvalidOperationException("Failed to start ATS CLI process.");
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        using var registration = cancellationToken.Register(() =>
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }
            }
            catch
            {
                // Ignore process kill race conditions during cancellation.
            }
        });

        var exitCode = await completionSource.Task.WaitAsync(cancellationToken);
        var outputText = outputBuilder.ToString();

        return new CliRunResult
        {
            ExitCode = exitCode,
            OutputText = outputText,
            ResultJsonPath = GetArtifactPath(artifactPaths, "result.json"),
            StructuredLogPath = GetArtifactPath(artifactPaths, "session.events.jsonl"),
            SessionLogPath = GetArtifactPath(artifactPaths, "session.log")
        };
    }

    private static string GetArtifactPath(
        IReadOnlyDictionary<string, string> artifactPaths,
        string artifactName)
    {
        return artifactPaths.TryGetValue(artifactName, out var path)
            ? path
            : string.Empty;
    }
}

internal sealed class CliRunResult
{
    public int ExitCode { get; init; }

    public string OutputText { get; init; } = string.Empty;

    public string ResultJsonPath { get; init; } = string.Empty;

    public string StructuredLogPath { get; init; } = string.Empty;

    public string SessionLogPath { get; init; } = string.Empty;
}
