namespace ATS.Core.Models;

public sealed class DeviceCommandResult
{
    public string SessionId { get; init; } = string.Empty;

    public string CommandName { get; init; } = string.Empty;

    public string DeviceName { get; init; } = string.Empty;

    public string Command { get; init; } = string.Empty;

    public string Response { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public DateTimeOffset StartedAtUtc { get; init; }

    public DateTimeOffset CompletedAtUtc { get; init; }

    public double DurationSeconds => (CompletedAtUtc - StartedAtUtc).TotalSeconds;

    public List<string> Errors { get; init; } = new();
}
