using ATS.Core.Scripts;

namespace ATS.Core.Devices;

public interface IDevice
{
    string Name { get; }

    Task ConnectAsync(CancellationToken cancellationToken);

    Task DisconnectAsync(CancellationToken cancellationToken);

    Task<DeviceCommandResponse> ExecuteAsync(DeviceCommandRequest request, CancellationToken cancellationToken);
}
