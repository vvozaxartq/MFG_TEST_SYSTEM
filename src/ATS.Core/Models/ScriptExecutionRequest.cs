using ATS.Core.Devices;

namespace ATS.Core.Models;

public sealed record ScriptExecutionRequest
{
    public string StepName { get; init; } = string.Empty;

    public string Command { get; init; } = string.Empty;

    public string SimulatedResponse { get; init; } = string.Empty;

    public int AttemptNumber { get; init; } = 1;

    public StepExecutionPolicy Policy { get; init; } = new();

    public DutExecutionRuntime DutRuntime { get; init; } = new();

    public TestContext Context { get; init; } = null!;

    public IDeviceSession DeviceSession { get; init; } = null!;
}
