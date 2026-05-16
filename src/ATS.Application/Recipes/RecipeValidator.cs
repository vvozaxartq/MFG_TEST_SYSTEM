using ATS.Application.Execution;
using ATS.Application.Specs;

namespace ATS.Application.Recipes;

public sealed class RecipeValidator
{
    private readonly VariableResolver _variableResolver = new();

    public List<string> Validate(
        RecipeDefinition recipe,
        SpecDocument specDocument,
        string selectedScriptName)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(recipe.Name))
        {
            errors.Add("Recipe name is required.");
        }

        if (recipe.Scripts.Count == 0)
        {
            errors.Add("Recipe must contain at least one script.");
        }

        ValidateVariableDictionary(recipe.Variables, "recipe.variables", errors);

        var duplicateScriptNames = recipe.Scripts
            .Where(item => !string.IsNullOrWhiteSpace(item.Name))
            .GroupBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();
        var declaredScriptNames = recipe.Scripts
            .Where(item => !string.IsNullOrWhiteSpace(item.Name))
            .Select(item => item.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var duplicateScriptName in duplicateScriptNames)
        {
            errors.Add($"Duplicate script name '{duplicateScriptName}' was found.");
        }

        var legacySpecKeys = new HashSet<string>(
            specDocument.Specs.Select(item => item.Key),
            StringComparer.OrdinalIgnoreCase);

        if (legacySpecKeys.Count == 0 && specDocument.Rules.Count == 0)
        {
            errors.Add("No spec definitions were found. Provide inline recipe rules/specs or use --spec.");
        }

        var declaredFullKeys = new List<(string StepName, string FullKey)>();

        foreach (var script in recipe.Scripts)
        {
            if (string.IsNullOrWhiteSpace(script.Name))
            {
                errors.Add("Recipe script name is required.");
            }

            if (string.IsNullOrWhiteSpace(script.Command))
            {
                errors.Add($"Script '{script.Name}' command is required.");
            }

            if (script.RetryCount < 0)
            {
                errors.Add($"Script '{script.Name}' retryCount must be greater than or equal to 0.");
            }

            if (script.TimeoutMs < 0)
            {
                errors.Add($"Script '{script.Name}' timeoutMs must be greater than or equal to 0.");
            }

            ValidateVariableDictionary(script.Variables, $"script '{script.Name}' variables", errors);
            ValidateTemplateField(script.Command, "Command", script.Name, errors);
            ValidateTemplateField(script.SimulatedResponse, "SimulatedResponse", script.Name, errors);
            ValidateTemplateField(script.Unit, "Unit", script.Name, errors);

            var measurements = RecipeStepDefinitionHelper.GetDeclaredMeasurements(recipe, script);

            if (measurements.Count == 0)
            {
                errors.Add($"Script '{script.Name}' must declare at least one measurement.");
            }

            var duplicateMeasurementKeys = measurements
                .GroupBy(item => item.FullKey, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();

            foreach (var duplicateMeasurementKey in duplicateMeasurementKeys)
            {
                errors.Add($"Script '{script.Name}' contains duplicate measurement fullKey '{duplicateMeasurementKey}'.");
            }

            foreach (var measurement in measurements)
            {
                declaredFullKeys.Add((script.Name, measurement.FullKey));
            }

            foreach (var measurement in script.Measurements)
            {
                ValidateTemplateField(
                    measurement.SourcePath,
                    $"Measurements[{measurement.Key}].SourcePath",
                    script.Name,
                    errors);
                ValidateTemplateField(
                    measurement.Unit,
                    $"Measurements[{measurement.Key}].Unit",
                    script.Name,
                    errors);
                ValidateTemplateField(
                    measurement.Description,
                    $"Measurements[{measurement.Key}].Description",
                    script.Name,
                    errors);
            }

            if (!string.IsNullOrWhiteSpace(script.SpecKey))
            {
                if (measurements.Count != 1)
                {
                    errors.Add(
                        $"Script '{script.Name}' uses legacy specKey '{script.SpecKey}' but declares multiple measurements.");
                }
                else if (!legacySpecKeys.Contains(script.SpecKey))
                {
                    errors.Add($"Script '{script.Name}' references missing spec '{script.SpecKey}'.");
                }
            }
            else
            {
                foreach (var measurement in measurements)
                {
                    var hasMatchingRule = specDocument.Rules.Any(item =>
                        string.Equals(item.TargetKey, measurement.FullKey, StringComparison.OrdinalIgnoreCase));

                    if (!hasMatchingRule)
                    {
                        errors.Add(
                            $"Script '{script.Name}' measurement fullKey '{measurement.FullKey}' does not have an exact matching spec rule targetKey.");
                    }
                }
            }
        }

        if (recipe.Flow is not null)
        {
            ValidateSequenceNode(recipe.Flow, "recipe.flow", declaredScriptNames, errors);
        }

        var duplicateFullKeysAcrossSteps = declaredFullKeys
            .GroupBy(item => item.FullKey, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        foreach (var duplicateFullKey in duplicateFullKeysAcrossSteps)
        {
            errors.Add($"Duplicate measurement fullKey '{duplicateFullKey}' was found across recipe steps.");
        }

        var declaredFullKeySet = declaredFullKeys
            .Select(item => item.FullKey)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var rule in specDocument.Rules)
        {
            if (string.IsNullOrWhiteSpace(rule.TargetKey))
            {
                continue;
            }

            if (!declaredFullKeySet.Contains(rule.TargetKey))
            {
                errors.Add(
                    $"Spec rule '{rule.Name}' targetKey '{rule.TargetKey}' does not match any declared measurement fullKey.");
            }
        }

        if (!string.IsNullOrWhiteSpace(selectedScriptName) &&
            !recipe.Scripts.Any(item => string.Equals(item.Name, selectedScriptName, StringComparison.OrdinalIgnoreCase)))
        {
            errors.Add($"Script '{selectedScriptName}' was not found in recipe '{recipe.Name}'.");
        }

        return errors;
    }

    public List<string> GetValidationWarnings(RecipeDefinition recipe)
    {
        var warnings = new List<string>();

        foreach (var script in recipe.Scripts)
        {
            CollectRuntimeProvidedWarnings(script.Command, "Command", script.Name, warnings);
            CollectRuntimeProvidedWarnings(script.SimulatedResponse, "SimulatedResponse", script.Name, warnings);
            CollectRuntimeProvidedWarnings(script.Unit, "Unit", script.Name, warnings);

            foreach (var measurement in script.Measurements)
            {
                CollectRuntimeProvidedWarnings(
                    measurement.SourcePath,
                    $"Measurements[{measurement.Key}].SourcePath",
                    script.Name,
                    warnings);
                CollectRuntimeProvidedWarnings(
                    measurement.Unit,
                    $"Measurements[{measurement.Key}].Unit",
                    script.Name,
                    warnings);
                CollectRuntimeProvidedWarnings(
                    measurement.Description,
                    $"Measurements[{measurement.Key}].Description",
                    script.Name,
                    warnings);
            }
        }

        return warnings;
    }

    private static void ValidateSequenceNode(
        FlowSequenceNodeDefinition sequenceNode,
        string nodePath,
        IReadOnlySet<string> declaredScriptNames,
        List<string> errors)
    {
        ValidateOutcomePolicy(sequenceNode.OutcomePolicy, ResolveNodeName(sequenceNode, nodePath), "Flow sequence", errors);

        if (sequenceNode.Nodes.Count == 0)
        {
            errors.Add($"Flow sequence '{ResolveNodeName(sequenceNode, nodePath)}' must contain at least one child node.");
        }

        for (var index = 0; index < sequenceNode.Nodes.Count; index++)
        {
            ValidateFlowNode(
                sequenceNode.Nodes[index],
                $"{nodePath}.nodes[{index}]",
                declaredScriptNames,
                errors);
        }
    }

    private static void ValidateFlowNode(
        FlowNodeDefinition node,
        string nodePath,
        IReadOnlySet<string> declaredScriptNames,
        List<string> errors)
    {
        switch (node)
        {
            case FlowStepNodeDefinition stepNode:
                if (string.IsNullOrWhiteSpace(stepNode.Step))
                {
                    errors.Add($"Flow step node '{ResolveNodeName(stepNode, nodePath)}' must reference a non-empty script name.");
                }
                else if (!declaredScriptNames.Contains(stepNode.Step))
                {
                    errors.Add($"Flow step node '{ResolveNodeName(stepNode, nodePath)}' references unknown script '{stepNode.Step}'.");
                }

                break;
            case FlowSequenceNodeDefinition sequenceNode:
                ValidateSequenceNode(sequenceNode, nodePath, declaredScriptNames, errors);
                break;
            case FlowConditionNodeDefinition conditionNode:
                ValidateConditionNode(conditionNode, nodePath, declaredScriptNames, errors);
                break;
            case FlowRepeatUntilNodeDefinition repeatUntilNode:
                ValidateRepeatUntilNode(repeatUntilNode, nodePath, declaredScriptNames, errors);
                break;
            default:
                errors.Add($"Flow node '{nodePath}' uses unsupported type '{node.GetType().Name}'.");
                break;
        }
    }

    private static void ValidateConditionNode(
        FlowConditionNodeDefinition conditionNode,
        string nodePath,
        IReadOnlySet<string> declaredScriptNames,
        List<string> errors)
    {
        var conditionName = ResolveNodeName(conditionNode, nodePath);
        ValidateConditionDefinition(conditionNode.Condition, conditionName, "Flow condition node", errors);

        if (conditionNode.WhenTrue.Count == 0 && conditionNode.WhenFalse.Count == 0)
        {
            errors.Add($"Flow condition node '{conditionName}' must declare at least one child node in whenTrue or whenFalse.");
        }

        for (var index = 0; index < conditionNode.WhenTrue.Count; index++)
        {
            ValidateFlowNode(
                conditionNode.WhenTrue[index],
                $"{nodePath}.whenTrue[{index}]",
                declaredScriptNames,
                errors);
        }

        for (var index = 0; index < conditionNode.WhenFalse.Count; index++)
        {
            ValidateFlowNode(
                conditionNode.WhenFalse[index],
                $"{nodePath}.whenFalse[{index}]",
                declaredScriptNames,
                errors);
        }
    }

    private static void ValidateRepeatUntilNode(
        FlowRepeatUntilNodeDefinition repeatUntilNode,
        string nodePath,
        IReadOnlySet<string> declaredScriptNames,
        List<string> errors)
    {
        var loopName = ResolveNodeName(repeatUntilNode, nodePath);
        if (repeatUntilNode.Nodes.Count == 0)
        {
            errors.Add($"Flow repeatUntil node '{loopName}' must contain at least one child node.");
        }

        if (repeatUntilNode.MaxIterations <= 0)
        {
            errors.Add($"Flow repeatUntil node '{loopName}' must declare maxIterations greater than 0.");
        }

        ValidateOutcomePolicy(repeatUntilNode.OutcomePolicy, loopName, "Flow repeatUntil node", errors);
        ValidateConditionDefinition(repeatUntilNode.Until, loopName, "Flow repeatUntil node", errors);

        for (var index = 0; index < repeatUntilNode.Nodes.Count; index++)
        {
            ValidateFlowNode(
                repeatUntilNode.Nodes[index],
                $"{nodePath}.nodes[{index}]",
                declaredScriptNames,
                errors);
        }
    }

    private static void ValidateConditionDefinition(
        FlowConditionDefinition condition,
        string ownerName,
        string ownerType,
        List<string> errors)
    {
        var normalizedType = NormalizeConditionType(condition.Type);

        if (string.IsNullOrWhiteSpace(normalizedType))
        {
            errors.Add($"{ownerType} '{ownerName}' must declare condition.type.");
            return;
        }

        switch (normalizedType)
        {
            case "previousstepstatus":
                if (string.IsNullOrWhiteSpace(condition.Status))
                {
                    errors.Add($"{ownerType} '{ownerName}' using previousStepStatus must declare condition.status.");
                }

                break;
            case "dataexists":
                if (string.IsNullOrWhiteSpace(condition.Key))
                {
                    errors.Add($"{ownerType} '{ownerName}' using dataExists must declare condition.key.");
                }

                break;
            case "dataequals":
                if (string.IsNullOrWhiteSpace(condition.Key))
                {
                    errors.Add($"{ownerType} '{ownerName}' using dataEquals must declare condition.key.");
                }

                if (string.IsNullOrWhiteSpace(condition.Value))
                {
                    errors.Add($"{ownerType} '{ownerName}' using dataEquals must declare condition.value.");
                }

                break;
            default:
                errors.Add(
                    $"{ownerType} '{ownerName}' uses unsupported condition.type '{condition.Type}'. Supported values: previousStepStatus, dataExists, dataEquals.");
                break;
        }
    }

    private static void ValidateOutcomePolicy(
        string outcomePolicy,
        string ownerName,
        string ownerType,
        List<string> errors)
    {
        var normalizedPolicy = NormalizeOutcomePolicy(outcomePolicy);
        if (string.IsNullOrWhiteSpace(normalizedPolicy))
        {
            return;
        }

        if (normalizedPolicy is not ("breakonstepfailure" or "breakonstepsuccess"))
        {
            errors.Add(
                $"{ownerType} '{ownerName}' uses unsupported outcomePolicy '{outcomePolicy}'. Supported values: breakOnStepFailure, breakOnStepSuccess.");
        }
    }

    private void ValidateTemplateField(
        string template,
        string fieldName,
        string stepName,
        List<string> errors)
    {
        try
        {
            _variableResolver.ValidateTemplateSyntax(template, fieldName, stepName);
        }
        catch (InvalidOperationException exception)
        {
            errors.Add(exception.Message);
            return;
        }

        foreach (var variableName in _variableResolver.GetRequestedVariableNames(template))
        {
            if (VariableResolver.IsDutVariableName(variableName) &&
                !VariableResolver.IsSupportedDutVariableName(variableName))
            {
                errors.Add(
                    $"Field '{fieldName}' in step '{ScriptNameOrSession(stepName)}' references unsupported DUT variable '{variableName}'. Supported values: dut.id, dut.index, dut.sn, dut.station, dut.slot, dut.isSimulated.");
            }
        }
    }

    private void CollectRuntimeProvidedWarnings(
        string template,
        string fieldName,
        string stepName,
        List<string> warnings)
    {
        if (string.IsNullOrEmpty(template))
        {
            return;
        }

        try
        {
            _variableResolver.ValidateTemplateSyntax(template, fieldName, stepName);
        }
        catch (InvalidOperationException)
        {
            return;
        }

        foreach (var variableName in _variableResolver.GetRequestedVariableNames(template))
        {
            if (!VariableResolver.IsSupportedDutVariableName(variableName))
            {
                continue;
            }

            warnings.Add(
                $"Field '{fieldName}' in step '{ScriptNameOrSession(stepName)}' uses runtime-provided DUT variable '{NormalizeDutVariableName(variableName)}'. Validation keeps DUT values unresolved until execution.");
        }
    }

    private static void ValidateVariableDictionary(
        IReadOnlyDictionary<string, string> variables,
        string owner,
        List<string> errors)
    {
        foreach (var pair in variables)
        {
            if (string.IsNullOrWhiteSpace(pair.Key))
            {
                errors.Add($"{owner} contains an empty variable name.");
            }
        }
    }

    private static string ScriptNameOrSession(string stepName)
    {
        return string.IsNullOrWhiteSpace(stepName) ? "SESSION" : stepName;
    }

    private static string NormalizeDutVariableName(string variableName)
    {
        return variableName[4..].ToLowerInvariant() switch
        {
            "id" => "dut.id",
            "index" => "dut.index",
            "sn" => "dut.sn",
            "station" => "dut.station",
            "slot" => "dut.slot",
            "issimulated" => "dut.isSimulated",
            _ => variableName
        };
    }

    private static string NormalizeConditionType(string value)
    {
        return string.Concat(value.Where(character => !char.IsWhiteSpace(character)))
            .ToLowerInvariant();
    }

    private static string NormalizeOutcomePolicy(string value)
    {
        return string.Concat((value ?? string.Empty)
                .Where(character => !char.IsWhiteSpace(character) && character is not '-' and not '_'))
            .ToLowerInvariant();
    }

    private static string ResolveNodeName(FlowNodeDefinition node, string fallback)
    {
        return string.IsNullOrWhiteSpace(node.Name)
            ? fallback
            : node.Name;
    }
}
