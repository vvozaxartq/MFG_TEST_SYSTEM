using System.Globalization;
using ATS.Core.Devices;

namespace ATS.Application.Devices;

public sealed class FakeDevice : IDevice
{
    private bool _isConnected;

    public string Name => "FakeDevice";

    public Task ConnectAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _isConnected = true;
        return Task.CompletedTask;
    }

    public Task DisconnectAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _isConnected = false;
        return Task.CompletedTask;
    }

    public async Task<DeviceCommandResponse> ExecuteAsync(
        DeviceCommandRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_isConnected)
        {
            throw new InvalidOperationException("FakeDevice is not connected.");
        }

        await Task.Delay(TimeSpan.FromMilliseconds(10), cancellationToken);

        var response = string.IsNullOrWhiteSpace(request.SimulatedResponse)
            ? ResolveResponse(request.Command)
            : request.SimulatedResponse;

        return new DeviceCommandResponse
        {
            Command = request.Command,
            Response = response,
            Success = true,
            Message = "FakeDevice command completed successfully."
        };
    }

    private static string ResolveResponse(string command)
    {
        return command.Trim().ToUpperInvariant() switch
        {
            "PING" => "PONG",
            "READ_VOLTAGE" => 12.3m.ToString(CultureInfo.InvariantCulture),
            "READ_CURRENT" => 1.4m.ToString(CultureInfo.InvariantCulture),
            "READ_SERIAL" => "ATS-FAKE-001",
            "READ_MODEL" => "MFG TEST SYSTEM",
            "READ_STATION" => "STATION-A",
            "READ_TEMPERATURE" => 35.5m.ToString(CultureInfo.InvariantCulture),
            "READ_LEAK_RATE" => 0.2m.ToString(CultureInfo.InvariantCulture),
            "READ_ERROR" => "NONE",
            "CALIBRATION_STATUS" => "SKIPPED",
            _ => $"ACK:{command}"
        };
    }
}
