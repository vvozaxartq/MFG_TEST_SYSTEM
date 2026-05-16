using System.Text.Json;

namespace ATS.Application.Recipes;

public sealed class RecipeLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public RecipeDefinition Load(string recipePath)
    {
        if (string.IsNullOrWhiteSpace(recipePath))
        {
            throw new ArgumentException("Recipe path is required.", nameof(recipePath));
        }

        if (!File.Exists(recipePath))
        {
            throw new FileNotFoundException("Recipe file was not found.", recipePath);
        }

        var json = File.ReadAllText(recipePath);
        var recipe = JsonSerializer.Deserialize<RecipeDefinition>(json, JsonOptions)
            ?? throw new InvalidOperationException("Recipe file could not be parsed.");

        recipe.Rules ??= new List<ATS.Core.Models.SpecRule>();
        recipe.Scripts ??= new List<RecipeScriptDefinition>();
        recipe.Specs ??= new List<SpecDefinition>();
        recipe.Variables ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var script in recipe.Scripts)
        {
            script.Measurements ??= new List<RecipeMeasurementDefinition>();
            script.Variables ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        if (recipe.Flow is not null)
        {
            NormalizeSequenceNode(recipe.Flow);
        }

        return recipe;
    }

    private static void NormalizeSequenceNode(FlowSequenceNodeDefinition sequenceNode)
    {
        sequenceNode.OutcomePolicy ??= string.Empty;
        sequenceNode.Nodes ??= new List<FlowNodeDefinition>();

        foreach (var node in sequenceNode.Nodes)
        {
            NormalizeNode(node);
        }
    }

    private static void NormalizeNode(FlowNodeDefinition node)
    {
        switch (node)
        {
            case FlowSequenceNodeDefinition sequenceNode:
                NormalizeSequenceNode(sequenceNode);
                break;
            case FlowConditionNodeDefinition conditionNode:
                conditionNode.Condition ??= new FlowConditionDefinition();
                conditionNode.WhenTrue ??= new List<FlowNodeDefinition>();
                conditionNode.WhenFalse ??= new List<FlowNodeDefinition>();

                foreach (var childNode in conditionNode.WhenTrue)
                {
                    NormalizeNode(childNode);
                }

                foreach (var childNode in conditionNode.WhenFalse)
                {
                    NormalizeNode(childNode);
                }

                break;
            case FlowRepeatUntilNodeDefinition repeatUntilNode:
                repeatUntilNode.Nodes ??= new List<FlowNodeDefinition>();
                repeatUntilNode.Until ??= new FlowConditionDefinition();
                repeatUntilNode.OutcomePolicy ??= string.Empty;

                foreach (var childNode in repeatUntilNode.Nodes)
                {
                    NormalizeNode(childNode);
                }

                break;
        }
    }
}
