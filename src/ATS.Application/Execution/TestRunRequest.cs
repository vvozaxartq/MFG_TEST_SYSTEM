namespace ATS.Application.Execution;

public sealed record TestRunRequest(
    string CommandName,
    string RecipePath,
    string SpecPath,
    string OutputDirectory,
    string SelectedScriptName);
