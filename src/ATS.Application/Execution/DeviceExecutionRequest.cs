namespace ATS.Application.Execution;

public sealed record DeviceExecutionRequest(
    string CommandName,
    string Command,
    string OutputDirectory);
