namespace ATS.Core.Devices;

public sealed class DeviceCommandResponse
{
    public string Command { get; init; } = string.Empty;

    public string Response { get; init; } = string.Empty;

    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;
}
