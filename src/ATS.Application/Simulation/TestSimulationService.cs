using ATS.Application.Execution;
using ATS.Core.Models;

namespace ATS.Application.Simulation;

public sealed class TestSimulationService
{
    private readonly TestRunner _testRunner;

    public TestSimulationService()
        : this(new TestRunner())
    {
    }

    public TestSimulationService(TestRunner testRunner)
    {
        _testRunner = testRunner;
    }

    public Task<TestResult> RunAsync(
        string recipePath,
        string outputDirectory,
        CancellationToken cancellationToken)
    {
        return _testRunner.RunAsync(
            new TestRunRequest(
                "test simulate",
                recipePath,
                string.Empty,
                outputDirectory,
                string.Empty),
            cancellationToken);
    }
}
