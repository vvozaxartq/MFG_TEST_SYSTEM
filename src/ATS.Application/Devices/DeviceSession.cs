using ATS.Core.Devices;

namespace ATS.Application.Devices;

internal sealed class DeviceSession : IDeviceSession, IAsyncDisposable
{
    private readonly IDevice _device;
    private bool _isConnected;

    public DeviceSession(IDevice device)
    {
        _device = device;
    }

    public string DeviceName => _device.Name;

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        if (_isConnected)
        {
            return;
        }

        await _device.ConnectAsync(cancellationToken);
        _isConnected = true;
    }

    public Task<DeviceCommandResponse> ExecuteAsync(DeviceCommandRequest request, CancellationToken cancellationToken)
    {
        if (!_isConnected)
        {
            throw new InvalidOperationException("Device session is not connected.");
        }

        return _device.ExecuteAsync(request, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (!_isConnected)
        {
            return;
        }

        try
        {
            await _device.DisconnectAsync(CancellationToken.None);
        }
        finally
        {
            _isConnected = false;
        }
    }
}
