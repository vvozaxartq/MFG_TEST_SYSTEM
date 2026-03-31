namespace ATS.Core.Devices;

public sealed class DeviceCommandRequest
{
    public string Command { get; init; } = string.Empty;

    public string SimulatedResponse { get; init; } = string.Empty;
}
