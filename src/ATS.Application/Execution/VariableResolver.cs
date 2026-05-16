using System.Globalization;
using System.Text;
using ATS.Application.Recipes;
using ATS.Core.Models;

namespace ATS.Application.Execution;

public sealed class VariableResolver
{
    private static readonly string[] SupportedDutVariableNames =
    [
        "dut.id",
        "dut.index",
        "dut.sn",
        "dut.station",
        "dut.slot",
        "dut.issimulated"
    ];

    public RecipeScriptDefinition ResolveStepDefinition(
        RecipeScriptDefinition definition,
        VariableContext variableContext,
        TestContext? testContext = null,
        string dutId = "")
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(variableContext);

        return new RecipeScriptDefinition
        {
            Name = definition.Name,
            Command = ResolveTemplate(
                definition.Command,
                variableContext,
                "Command",
                definition.Name,
                testContext,
                dutId),
            Prefix = definition.Prefix,
            Variables = new Dictionary<string, string>(definition.Variables, StringComparer.OrdinalIgnoreCase),
            MeasurementKey = definition.MeasurementKey,
            Unit = ResolveTemplate(
                definition.Unit,
                variableContext,
                "Unit",
                definition.Name,
                testContext,
                dutId),
            SpecKey = definition.SpecKey,
            Measurements = definition.Measurements
                .Select(item => new RecipeMeasurementDefinition
                {
                    Key = item.Key,
                    SourcePath = ResolveTemplate(
                        item.SourcePath,
                        variableContext,
                        $"Measurements[{item.Key}].SourcePath",
                        definition.Name,
                        testContext,
                        dutId),
                    ValueType = item.ValueType,
                    Unit = ResolveTemplate(
                        item.Unit,
                        variableContext,
                        $"Measurements[{item.Key}].Unit",
                        definition.Name,
                        testContext,
                        dutId),
                    Description = ResolveTemplate(
                        item.Description,
                        variableContext,
                        $"Measurements[{item.Key}].Description",
                        definition.Name,
                        testContext,
                        dutId)
                })
                .ToList(),
            SimulatedValue = definition.SimulatedValue,
            SimulatedResponse = ResolveTemplate(
                definition.SimulatedResponse,
                variableContext,
                "SimulatedResponse",
                definition.Name,
                testContext,
                dutId)
        };
    }

    public string ResolveTemplate(
        string template,
        VariableContext variableContext,
        string fieldName,
        string stepName = "",
        TestContext? testContext = null,
        string dutId = "")
    {
        return ResolveTemplateWithTrace(template, variableContext, fieldName, stepName, testContext, dutId).ResolvedText;
    }

    internal IReadOnlyList<string> GetRequestedVariableNames(string template)
    {
        if (string.IsNullOrEmpty(template))
        {
            return Array.Empty<string>();
        }

        var variableNames = new List<string>();
        for (var index = 0; index < template.Length; index++)
        {
            if (!IsVariableStart(template, index))
            {
                continue;
            }

            var closingIndex = template.IndexOf('}', index + 2);
            if (closingIndex < 0)
            {
                break;
            }

            variableNames.Add(template.Substring(index + 2, closingIndex - (index + 2)));
            index = closingIndex;
        }

        return variableNames;
    }

    internal VariableResolutionResult ResolveTemplateWithTrace(
        string template,
        VariableContext variableContext,
        string fieldName,
        string stepName = "",
        TestContext? testContext = null,
        string dutId = "")
    {
        ArgumentNullException.ThrowIfNull(variableContext);

        if (string.IsNullOrEmpty(template))
        {
            return new VariableResolutionResult(string.Empty, Array.Empty<ResolvedVariable>());
        }

        ValidateTemplateSyntax(template, fieldName, stepName);

        var builder = new StringBuilder(template.Length);
        var resolvedVariables = new List<ResolvedVariable>();

        for (var index = 0; index < template.Length; index++)
        {
            if (IsVariableStart(template, index))
            {
                var closingIndex = template.IndexOf('}', index + 2);
                if (closingIndex < 0)
                {
                    FailResolution(
                        $"Field '{fieldName}' in step '{NormalizeStepName(stepName)}' contains an unterminated variable placeholder.",
                        fieldName,
                        string.Empty,
                        string.Empty,
                        stepName,
                        testContext,
                        dutId);
                }

                var variableName = template.Substring(index + 2, closingIndex - (index + 2));
                var resolved = ResolveVariable(variableName, variableContext, fieldName, stepName, testContext, dutId);
                builder.Append(resolved.Value);
                resolvedVariables.Add(resolved);
                index = closingIndex;
                continue;
            }

            builder.Append(template[index]);
        }

        var resolvedText = builder.ToString();
        if (resolvedText.Contains("${", StringComparison.Ordinal))
        {
            FailResolution(
                $"Field '{fieldName}' in step '{NormalizeStepName(stepName)}' contains nested variable expansion, which is not supported.",
                fieldName,
                string.Empty,
                string.Empty,
                stepName,
                testContext,
                dutId);
        }

        return new VariableResolutionResult(resolvedText, resolvedVariables);
    }

    public void ValidateTemplateSyntax(string template, string fieldName, string stepName = "")
    {
        if (string.IsNullOrEmpty(template))
        {
            return;
        }

        for (var index = 0; index < template.Length; index++)
        {
            if (template[index] != '$' || index + 1 >= template.Length || template[index + 1] != '{')
            {
                continue;
            }

            var closingIndex = template.IndexOf('}', index + 2);
            if (closingIndex < 0)
            {
                throw new InvalidOperationException(
                    $"Field '{fieldName}' in step '{NormalizeStepName(stepName)}' contains an unterminated variable placeholder.");
            }

            var variableName = template.Substring(index + 2, closingIndex - (index + 2));
            if (string.IsNullOrWhiteSpace(variableName))
            {
                throw new InvalidOperationException(
                    $"Field '{fieldName}' in step '{NormalizeStepName(stepName)}' contains an empty variable placeholder.");
            }

            if (variableName != variableName.Trim() || variableName.Any(char.IsWhiteSpace))
            {
                throw new InvalidOperationException(
                    $"Field '{fieldName}' in step '{NormalizeStepName(stepName)}' contains invalid variable name '{variableName}'.");
            }

            index = closingIndex;
        }
    }

    internal static bool IsDutVariableName(string variableName)
    {
        return variableName.StartsWith("dut.", StringComparison.OrdinalIgnoreCase);
    }

    internal static bool IsSupportedDutVariableName(string variableName)
    {
        return SupportedDutVariableNames.Contains(
            GetCanonicalDutVariableName(variableName),
            StringComparer.Ordinal);
    }

    private ResolvedVariable ResolveVariable(
        string variableName,
        VariableContext variableContext,
        string fieldName,
        string stepName,
        TestContext? testContext,
        string dutId)
    {
        if (IsDutVariableName(variableName))
        {
            if (TryResolveDutVariable(variableName, variableContext.DutContext, out var dutResolved))
            {
                return CreateResolvedVariable(
                    dutResolved.RequestedName,
                    dutResolved.ResolvedName,
                    dutResolved.Value,
                    dutResolved.Scope,
                    dutResolved.Source,
                    fieldName,
                    stepName,
                    testContext,
                    dutId);
            }

            FailResolution(
                $"Variable '{variableName}' required by field '{fieldName}' in step '{NormalizeStepName(stepName)}' was not found in DutContext.",
                fieldName,
                variableName,
                "Dut",
                stepName,
                testContext,
                dutId);
        }

        if (variableContext.StepVariables.TryGetValue(variableName, out var stepValue))
        {
            return CreateResolvedVariable(
                variableName,
                variableName,
                stepValue,
                VariableScope.Step,
                GetSource(variableContext.StepSources, variableName, $"step.variables.{variableName}"),
                fieldName,
                stepName,
                testContext,
                dutId);
        }

        if (variableContext.GlobalVariables.TryGetValue(variableName, out var globalValue))
        {
            return CreateResolvedVariable(
                variableName,
                variableName,
                globalValue,
                VariableScope.Global,
                GetSource(variableContext.GlobalSources, variableName, $"global.variables.{variableName}"),
                fieldName,
                stepName,
                testContext,
                dutId);
        }

        FailResolution(
            $"Variable '{variableName}' required by field '{fieldName}' in step '{NormalizeStepName(stepName)}' was not found. Search order: Step > Dut > Global.",
            fieldName,
            variableName,
            "Step>Dut>Global",
            stepName,
            testContext,
            dutId);
        return new ResolvedVariable();
    }

    private static bool TryResolveDutVariable(
        string requestedName,
        DutContext dutContext,
        out ResolvedVariable resolved)
    {
        var canonicalName = GetCanonicalDutVariableName(requestedName);

        resolved = canonicalName switch
        {
            "dut.id" when !string.IsNullOrWhiteSpace(dutContext.Id) => CreateDutResolvedVariable(
                requestedName,
                "dut.id",
                dutContext.Id,
                "DutContext.Id"),
            "dut.index" => CreateDutResolvedVariable(
                requestedName,
                "dut.index",
                dutContext.Index.ToString(CultureInfo.InvariantCulture),
                "DutContext.Index"),
            "dut.sn" when !string.IsNullOrWhiteSpace(dutContext.SerialNumber) => CreateDutResolvedVariable(
                requestedName,
                "dut.sn",
                dutContext.SerialNumber,
                "DutContext.SerialNumber"),
            "dut.station" when !string.IsNullOrWhiteSpace(dutContext.Station) => CreateDutResolvedVariable(
                requestedName,
                "dut.station",
                dutContext.Station,
                "DutContext.Station"),
            "dut.slot" when !string.IsNullOrWhiteSpace(dutContext.Slot) => CreateDutResolvedVariable(
                requestedName,
                "dut.slot",
                dutContext.Slot,
                "DutContext.Slot"),
            "dut.issimulated" => CreateDutResolvedVariable(
                requestedName,
                "dut.isSimulated",
                dutContext.IsSimulated ? "true" : "false",
                "DutContext.IsSimulated"),
            _ => new ResolvedVariable()
        };

        return !string.IsNullOrWhiteSpace(resolved.ResolvedName);
    }

    private static ResolvedVariable CreateDutResolvedVariable(
        string requestedName,
        string resolvedName,
        string value,
        string source)
    {
        return new ResolvedVariable
        {
            RequestedName = requestedName,
            ResolvedName = resolvedName,
            Value = value,
            Scope = VariableScope.Dut,
            Source = source
        };
    }

    private ResolvedVariable CreateResolvedVariable(
        string requestedName,
        string resolvedName,
        string value,
        VariableScope scope,
        string source,
        string fieldName,
        string stepName,
        TestContext? testContext,
        string dutId)
    {
        var resolved = new ResolvedVariable
        {
            RequestedName = requestedName,
            ResolvedName = resolvedName,
            Value = value ?? string.Empty,
            Scope = scope,
            Source = source
        };

        testContext?.LogEvent(
            "INFO",
            StructuredLogEntryType.VariableResolved,
            $"Variable '{requestedName}' for field '{fieldName}' resolved from {scope}.",
            stepName,
            stepName: stepName,
            dutId: dutId,
            status: "Resolved",
            data: new Dictionary<string, object?>
            {
                ["fieldName"] = fieldName,
                ["requestedName"] = resolved.RequestedName,
                ["resolvedName"] = resolved.ResolvedName,
                ["scope"] = resolved.Scope.ToString(),
                ["source"] = resolved.Source,
                ["value"] = resolved.Value,
                ["variableName"] = resolved.RequestedName,
                ["resolvedValue"] = resolved.Value
            });

        return resolved;
    }

    private static string GetSource(
        IReadOnlyDictionary<string, string> sources,
        string variableName,
        string defaultSource)
    {
        return sources.TryGetValue(variableName, out var source) && !string.IsNullOrWhiteSpace(source)
            ? source
            : defaultSource;
    }

    private static string NormalizeStepName(string stepName)
    {
        return string.IsNullOrWhiteSpace(stepName) ? "SESSION" : stepName;
    }

    private static bool IsVariableStart(string template, int index)
    {
        return template[index] == '$' &&
               index + 1 < template.Length &&
               template[index + 1] == '{';
    }

    private static string GetCanonicalDutVariableName(string variableName)
    {
        return IsDutVariableName(variableName)
            ? $"dut.{variableName[4..].ToLowerInvariant()}"
            : variableName;
    }

    private static void FailResolution(
        string message,
        string fieldName,
        string variableName,
        string searchedScopes,
        string stepName,
        TestContext? testContext,
        string dutId)
    {
        testContext?.LogEvent(
            "ERROR",
            StructuredLogEntryType.VariableResolutionFailed,
            message,
            stepName,
            stepName: stepName,
            dutId: dutId,
            status: "Failed",
            data: new Dictionary<string, object?>
            {
                ["fieldName"] = fieldName,
                ["requestedName"] = variableName,
                ["variableName"] = variableName,
                ["searchedScopes"] = searchedScopes
            });
        throw new InvalidOperationException(message);
    }
}

internal sealed record VariableResolutionResult(
    string ResolvedText,
    IReadOnlyList<ResolvedVariable> ResolvedVariables);
