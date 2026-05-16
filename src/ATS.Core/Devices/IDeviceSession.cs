namespace ATS.Core.Devices;

public interface IDeviceSession
{
    string DeviceName { get; }

    Task<DeviceCommandResponse> ExecuteAsync(DeviceCommandRequest request, CancellationToken cancellationToken);
}
