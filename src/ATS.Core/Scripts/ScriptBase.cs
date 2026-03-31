using ATS.Core.Devices;
using ATS.Core.Models;

namespace ATS.Core.Scripts;

public abstract class ScriptBase
{
    protected ScriptBase(
        string name,
        string command,
        string measurementKey,
        string unit,
        string specKey,
        string simulatedResponse)
    {
        Name = name;
        Command = command;
        MeasurementKey = measurementKey;
        Unit = unit;
        SpecKey = specKey;
        SimulatedResponse = simulatedResponse;
    }

    public string Name { get; }

    public string Command { get; }

    public string MeasurementKey { get; }

    public string Unit { get; }

    public string SpecKey { get; }

    public string SimulatedResponse { get; }

    public abstract Task<ScriptExecutionResult> ExecuteAsync(
        IDevice device,
        TestContext context,
        CancellationToken cancellationToken);
}
