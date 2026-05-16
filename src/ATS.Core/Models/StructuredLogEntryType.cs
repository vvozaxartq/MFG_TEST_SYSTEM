namespace ATS.Core.Models;

public enum StructuredLogEntryType
{
    SessionStarted,
    InputCaptured,
    RecipeStarted,
    RecipeCompleted,
    DeviceConnected,
    StepStarted,
    VariableResolved,
    VariableResolutionFailed,
    MeasurementCollected,
    DataCollectionWrite,
    SpecEvaluated,
    ContainerStarted,
    ContainerCompleted,
    BranchEvaluated,
    BranchSelected,
    LoopStarted,
    LoopIterationStarted,
    LoopIterationCompleted,
    LoopConditionEvaluated,
    LoopMaxIterationsReached,
    LoopCompleted,
    StepRetried,
    StepTimedOut,
    StepCompleted,
    DeviceDisconnected,
    ArtifactWritten,
    SessionCompleted,
    Error,
    Message
}
