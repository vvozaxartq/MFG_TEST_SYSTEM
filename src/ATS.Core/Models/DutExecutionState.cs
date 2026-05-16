namespace ATS.Core.Models;

public sealed class DutExecutionState
{
    public string ActiveStepName { get; set; } = string.Empty;

    public string LastStepName { get; set; } = string.Empty;

    public string LastStepStatus { get; set; } = string.Empty;

    public int CompletedStepCount { get; set; }

    public int FailedStepCount { get; set; }

    public string LastError { get; set; } = string.Empty;
}
