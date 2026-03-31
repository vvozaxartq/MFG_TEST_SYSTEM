using ATS.Application.Devices;
using ATS.Core.Devices;
using ATS.Core.Models;

namespace ATS.Application.Execution;

public sealed class DeviceExecutor
{
    private readonly SessionFactory _sessionFactory;
    private readonly SessionArtifactWriter _artifactWriter;

    public DeviceExecutor()
        : this(new SessionFactory(), new SessionArtifactWriter())
    {
    }

    public DeviceExecutor(SessionFactory sessionFactory, SessionArtifactWriter artifactWriter)
    {
        _sessionFactory = sessionFactory;
        _artifactWriter = artifactWriter;
    }

    public async Task<DeviceCommandResult> ExecuteAsync(
        DeviceExecutionRequest request,
        CancellationToken cancellationToken)
    {
        var context = _sessionFactory.Create(request.CommandName, request.OutputDirectory);
        var device = new FakeDevice();
        var responseText = string.Empty;
        var status = "Error";
        var message = string.Empty;
        var errors = new List<string>();

        context.Log($"Session '{context.SessionId}' created for '{request.CommandName}'.");

        try
        {
            await device.ConnectAsync(cancellationToken);
            context.Log("Device connected.");

            var response = await device.ExecuteAsync(
                new DeviceCommandRequest
                {
                    Command = request.Command
                },
                cancellationToken);

            responseText = response.Response;
            message = response.Message;
            status = response.Success ? "Passed" : "Error";
            context.Log($"Command '{request.Command}' returned '{response.Response}'.");
        }
        catch (Exception exception)
        {
            status = "Error";
            message = exception.Message;
            errors.Add(exception.Message);
            context.LogError(exception.Message);
        }
        finally
        {
            try
            {
                await device.DisconnectAsync(cancellationToken);
                context.Log("Device disconnected.");
            }
            catch (Exception exception)
            {
                errors.Add(exception.Message);
                context.LogError($"Device disconnect failed: {exception.Message}");
            }
        }

        var result = new DeviceCommandResult
        {
            SessionId = context.SessionId,
            CommandName = request.CommandName,
            DeviceName = device.Name,
            Command = request.Command,
            Response = responseText,
            Status = status,
            Message = string.IsNullOrWhiteSpace(message) ? status : message,
            StartedAtUtc = context.StartedAtUtc,
            CompletedAtUtc = DateTimeOffset.UtcNow,
            Errors = errors
        };

        _artifactWriter.WriteDeviceResult(result, context);
        return result;
    }
}
