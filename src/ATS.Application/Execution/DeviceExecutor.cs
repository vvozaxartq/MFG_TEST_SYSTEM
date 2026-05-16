using ATS.Application.Devices;
using ATS.Core.Devices;
using ATS.Core.Models;

namespace ATS.Application.Execution;

public sealed class DeviceExecutor
{
    private readonly SessionFactory _sessionFactory;
    private readonly SessionArtifactWriter _artifactWriter;
    private readonly IDeviceFactory _deviceFactory;

    public DeviceExecutor()
        : this(new SessionFactory(), new SessionArtifactWriter(), new FakeDeviceFactory())
    {
    }

    public DeviceExecutor(SessionFactory sessionFactory, SessionArtifactWriter artifactWriter, IDeviceFactory deviceFactory)
    {
        _sessionFactory = sessionFactory;
        _artifactWriter = artifactWriter;
        _deviceFactory = deviceFactory;
    }

    public async Task<DeviceCommandResult> ExecuteAsync(
        DeviceExecutionRequest request,
        CancellationToken cancellationToken)
    {
        var context = _sessionFactory.Create(request.CommandName, request.OutputDirectory, request.ArtifactOptions, request.RunInput);
        var device = _deviceFactory.CreateDevice();
        var responseText = string.Empty;
        var status = "Error";
        var message = string.Empty;
        var errors = new List<string>();
        var deviceConnected = false;

        context.Log($"Session '{context.SessionId}' created for '{request.CommandName}'.", request.CommandName);
        var deviceSession = new DeviceSession(device);

        try
        {
            await deviceSession.ConnectAsync(cancellationToken);
            deviceConnected = true;
            context.Log("Device connected.", request.CommandName);

            var response = await deviceSession.ExecuteAsync(
                new DeviceCommandRequest
                {
                    Command = request.Command
                },
                cancellationToken);

            responseText = response.Response;
            message = response.Message;
            status = response.Success ? "Passed" : "Error";
            context.Log($"Command '{request.Command}' returned '{response.Response}'.", request.Command);
        }
        catch (Exception exception)
        {
            status = "Error";
            message = exception.Message;
            errors.Add(exception.Message);
            context.LogError(exception.Message, request.Command);
        }
        finally
        {
            try
            {
                await deviceSession.DisposeAsync();
                if (deviceConnected)
                {
                    context.Log("Device disconnected.", request.CommandName);
                }
            }
            catch (Exception exception)
            {
                errors.Add(exception.Message);
                context.LogError($"Device disconnect failed: {exception.Message}", request.CommandName);
            }
        }

        var completedAtUtc = DateTimeOffset.UtcNow;
        var result = new DeviceCommandResult
        {
            SessionId = context.SessionId,
            CommandName = request.CommandName,
            DeviceName = device.Name,
            Command = request.Command,
            Response = responseText,
            Status = status,
            Message = string.IsNullOrWhiteSpace(message) ? status : message,
            OutputDirectory = context.OutputDirectory,
            ResultJsonPath = context.ArtifactPaths.ResultJsonPath,
            ResultCsvPath = context.ArtifactPaths.ResultCsvPath,
            SessionLogPath = context.ArtifactPaths.SessionLogPath,
            StructuredLogPath = context.ArtifactPaths.StructuredLogPath,
            RunInput = context.RunInput,
            SessionInfo = SessionInfoBuilder.Build(context, string.Empty, status, completedAtUtc),
            StartedAtUtc = context.StartedAtUtc,
            CompletedAtUtc = completedAtUtc,
            Errors = errors
        };

        context.LogEvent(
            string.Equals(status, "Passed", StringComparison.OrdinalIgnoreCase) ? "INFO" : "ERROR",
            StructuredLogEntryType.SessionCompleted,
            $"Session '{context.SessionId}' completed with status '{status}'.",
            request.CommandName,
            status: status,
            data: new Dictionary<string, object?>
            {
                ["command"] = request.Command,
                ["response"] = responseText,
                ["resultJsonPath"] = context.ArtifactPaths.ResultJsonPath,
                ["resultCsvPath"] = context.ArtifactPaths.ResultCsvPath,
                ["sessionLogPath"] = context.ArtifactPaths.SessionLogPath,
                ["structuredLogPath"] = context.ArtifactPaths.StructuredLogPath
            });
        _artifactWriter.WriteDeviceResult(result, context);
        return result;
    }
}
