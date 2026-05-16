using System.Text.Json.Serialization;

namespace ATS.Application.Recipes;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(FlowStepNodeDefinition), "step")]
[JsonDerivedType(typeof(FlowSequenceNodeDefinition), "sequence")]
[JsonDerivedType(typeof(FlowConditionNodeDefinition), "condition")]
[JsonDerivedType(typeof(FlowRepeatUntilNodeDefinition), "repeatUntil")]
public abstract class FlowNodeDefinition
{
    public string Name { get; set; } = string.Empty;
}

public sealed class FlowStepNodeDefinition : FlowNodeDefinition
{
    public string Step { get; set; } = string.Empty;
}

public sealed class FlowSequenceNodeDefinition : FlowNodeDefinition
{
    public string OutcomePolicy { get; set; } = string.Empty;

    public List<FlowNodeDefinition> Nodes { get; set; } = new();
}

public sealed class FlowConditionNodeDefinition : FlowNodeDefinition
{
    public FlowConditionDefinition Condition { get; set; } = new();

    public List<FlowNodeDefinition> WhenTrue { get; set; } = new();

    public List<FlowNodeDefinition> WhenFalse { get; set; } = new();
}

public sealed class FlowRepeatUntilNodeDefinition : FlowNodeDefinition
{
    public List<FlowNodeDefinition> Nodes { get; set; } = new();

    public FlowConditionDefinition Until { get; set; } = new();

    public string OutcomePolicy { get; set; } = string.Empty;

    public int MaxIterations { get; set; } = 1;

    public bool FailOnMaxIterations { get; set; } = true;
}

public sealed class FlowConditionDefinition
{
    public string Type { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}
