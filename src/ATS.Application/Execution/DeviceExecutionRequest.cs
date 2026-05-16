using ATS.Core.Models;

namespace ATS.Application.Execution;

public sealed record DeviceExecutionRequest(
    string CommandName,
    string Command,
    string OutputDirectory,
    SessionArtifactOptions? ArtifactOptions = null,
    RunInputModel? RunInput = null);
