using ATS.Application.Recipes;
using ATS.Core.Models;

namespace ATS.Application.Flow;

internal static class FlowRuntimeContextBuilder
{
    public static VariableContext BuildVariableContext(
        RecipeDefinition recipe,
        RecipeScriptDefinition step,
        TestContext context,
        DutContext dutContext)
    {
        var runInput = context.RunInput;
        var globalValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var globalSources = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var stepValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var stepSources = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var pair in recipe.Variables)
        {
            globalValues[pair.Key] = pair.Value;
            globalSources[pair.Key] = $"recipe.variables.{pair.Key}";
        }

        foreach (var pair in runInput.Values)
        {
            globalValues[pair.Key] = pair.Value;
            globalSources[pair.Key] = $"runInput.values.{pair.Key}";
        }

        if (!string.IsNullOrWhiteSpace(runInput.SerialNumber))
        {
            globalValues["SN"] = runInput.SerialNumber;
            globalSources["SN"] = "runInput.serialNumber";
            globalValues["ProductSN"] = runInput.SerialNumber;
            globalSources["ProductSN"] = "runInput.serialNumber";
        }

        if (!string.IsNullOrWhiteSpace(runInput.Station))
        {
            globalValues["Station"] = runInput.Station;
            globalSources["Station"] = "runInput.station";
        }

        if (!string.IsNullOrWhiteSpace(runInput.Mode))
        {
            globalValues["Mode"] = runInput.Mode;
            globalSources["Mode"] = "runInput.mode";
        }

        foreach (var pair in step.Variables)
        {
            stepValues[pair.Key] = pair.Value;
            stepSources[pair.Key] = $"step.variables.{pair.Key}";
        }

        return new VariableContext
        {
            GlobalVariables = globalValues,
            GlobalSources = globalSources,
            DutContext = dutContext,
            StepVariables = stepValues,
            StepSources = stepSources
        };
    }

    public static DutContext BuildDutContext(TestContext context)
    {
        var runInput = context.RunInput;
        return new DutContext
        {
            Id = ResolveFirstNonEmpty(
                GetDictionaryValue(runInput.Values, "DutId"),
                runInput.SerialNumber),
            Index = ResolveDutIndex(runInput.Values),
            SerialNumber = runInput.SerialNumber,
            Station = runInput.Station,
            Slot = GetDictionaryValue(runInput.Values, "Slot"),
            IsSimulated = string.Equals(context.CommandName, "test simulate", StringComparison.OrdinalIgnoreCase)
        };
    }

    private static int ResolveDutIndex(IReadOnlyDictionary<string, string> values)
    {
        var rawValue = ResolveFirstNonEmpty(
            GetDictionaryValue(values, "DutIndex"),
            GetDictionaryValue(values, "Index"));

        return int.TryParse(rawValue, out var index)
            ? index
            : 0;
    }

    private static string ResolveFirstNonEmpty(params string[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return string.Empty;
    }

    private static string GetDictionaryValue(IReadOnlyDictionary<string, string> values, string key)
    {
        return values.TryGetValue(key, out var value)
            ? value
            : string.Empty;
    }
}
