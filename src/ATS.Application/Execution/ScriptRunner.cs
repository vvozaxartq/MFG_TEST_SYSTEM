using ATS.Core.Models;

namespace ATS.Application.Execution;

public sealed class ScriptRunner
{
    private readonly TestRunner _testRunner;

    public ScriptRunner()
        : this(new TestRunner())
    {
    }

    public ScriptRunner(TestRunner testRunner)
    {
        _testRunner = testRunner;
    }

    public Task<TestResult> RunAsync(
        string recipePath,
        string specPath,
        string scriptName,
        string outputDirectory,
        SessionArtifactOptions? artifactOptions,
        RunInputModel? runInput,
        CancellationToken cancellationToken)
    {
        return _testRunner.RunAsync(
            new TestRunRequest(
                "script run",
                recipePath,
                specPath,
                outputDirectory,
                scriptName,
                artifactOptions,
                runInput),
            cancellationToken);
    }
}
