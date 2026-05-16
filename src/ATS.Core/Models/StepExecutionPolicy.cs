namespace ATS.Core.Models;

public sealed record StepExecutionPolicy
{
    public int RetryCount { get; init; }

    public int TimeoutMs { get; init; }

    public bool ContinueOnFailure { get; init; }

    public int MaxAttempts => Math.Max(1, RetryCount + 1);
}
