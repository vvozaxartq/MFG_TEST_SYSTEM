using System.Globalization;
using System.Text.Json;
using ATS.Application.Recipes;
using ATS.Core.Models;

namespace ATS.Application.Measurements;

internal sealed class MeasurementSetBuilder
{
    public MeasurementSet Build(
        RecipeDefinition recipe,
        RecipeScriptDefinition step,
        string rawPayload,
        DateTimeOffset collectedAt)
    {
        var prefix = RecipeStepDefinitionHelper.GetEffectivePrefix(recipe, step);
        var declaredMeasurements = RecipeStepDefinitionHelper.GetDeclaredMeasurements(recipe, step);
        var items = declaredMeasurements.Count > 0
            ? BuildFromDeclaredMeasurements(prefix, declaredMeasurements, rawPayload)
            : BuildFromPayload(prefix, rawPayload);

        return new MeasurementSet
        {
            Source = step.Name,
            Command = step.Command,
            CollectedAt = collectedAt,
            RawPayload = rawPayload,
            Items = items
        };
    }

    private static List<MeasurementItem> BuildFromDeclaredMeasurements(
        string prefix,
        IReadOnlyList<DeclaredMeasurementDefinition> declaredMeasurements,
        string rawPayload)
    {
        if (ShouldTreatAsStructuredPayload(rawPayload) && TryParseJson(rawPayload, out var document))
        {
            using (document)
            {
                return declaredMeasurements
                    .Select(item =>
                    {
                        if (!TryResolveJsonValue(document!.RootElement, item.SourcePath, out var valueElement))
                        {
                            throw new InvalidOperationException(
                                $"Measurement source path '{item.SourcePath}' was not found in payload.");
                        }

                        var rawText = ExtractText(valueElement);
                        return CreateItem(prefix, item.Key, rawText, item.Unit, item.Description, item.ValueType);
                    })
                    .ToList();
            }
        }

        if (declaredMeasurements.Count > 1)
        {
            throw new InvalidOperationException("Multiple measurements require a structured JSON payload.");
        }

        var declaredMeasurement = declaredMeasurements[0];
        return new List<MeasurementItem>
        {
            CreateItem(
                prefix,
                declaredMeasurement.Key,
                rawPayload,
                declaredMeasurement.Unit,
                declaredMeasurement.Description,
                declaredMeasurement.ValueType)
        };
    }

    private static List<MeasurementItem> BuildFromPayload(string prefix, string rawPayload)
    {
        if (ShouldTreatAsStructuredPayload(rawPayload) && TryParseJson(rawPayload, out var document))
        {
            using (document)
            {
                if (document!.RootElement.ValueKind != JsonValueKind.Object)
                {
                    throw new InvalidOperationException("Structured measurement payload must be a JSON object.");
                }

                var items = new List<MeasurementItem>();

                foreach (var property in document.RootElement.EnumerateObject())
                {
                    items.Add(CreateItem(prefix, property.Name, ExtractText(property.Value), string.Empty, string.Empty, string.Empty));
                }

                return items;
            }
        }

        if (string.IsNullOrWhiteSpace(rawPayload))
        {
            throw new InvalidOperationException("Measurement payload is empty.");
        }

        return new List<MeasurementItem>
        {
            CreateItem(prefix, "value", rawPayload, string.Empty, string.Empty, string.Empty)
        };
    }

    private static bool ShouldTreatAsStructuredPayload(string rawPayload)
    {
        if (string.IsNullOrWhiteSpace(rawPayload))
        {
            return false;
        }

        var trimmed = rawPayload.TrimStart();
        return trimmed.StartsWith("{", StringComparison.Ordinal) || trimmed.StartsWith("[", StringComparison.Ordinal);
    }

    private static MeasurementItem CreateItem(
        string prefix,
        string key,
        string value,
        string unit,
        string description,
        string declaredValueType)
    {
        var valueType = ResolveValueType(declaredValueType, value);
        return new MeasurementItem
        {
            Key = key,
            Prefix = prefix,
            FullKey = RecipeStepDefinitionHelper.BuildFullKey(prefix, key),
            Value = value,
            ValueType = valueType,
            Unit = unit,
            Description = description,
            RawText = value
        };
    }

    private static MeasurementValueType ResolveValueType(string declaredValueType, string value)
    {
        if (!string.IsNullOrWhiteSpace(declaredValueType) &&
            Enum.TryParse<MeasurementValueType>(declaredValueType, true, out var parsedDeclaredType))
        {
            return parsedDeclaredType;
        }

        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
        {
            return MeasurementValueType.Number;
        }

        if (bool.TryParse(value, out _))
        {
            return MeasurementValueType.Boolean;
        }

        return MeasurementValueType.Text;
    }

    private static bool TryParseJson(string rawPayload, out JsonDocument? document)
    {
        document = null;

        if (string.IsNullOrWhiteSpace(rawPayload))
        {
            return false;
        }

        try
        {
            document = JsonDocument.Parse(rawPayload);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool TryResolveJsonValue(JsonElement element, string path, out JsonElement value)
    {
        value = element;
        var current = element;

        foreach (var part in path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(part, out var next))
            {
                value = default;
                return false;
            }

            current = next;
        }

        value = current;
        return true;
    }

    private static string ExtractText(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number => element.GetRawText(),
            JsonValueKind.True => bool.TrueString,
            JsonValueKind.False => bool.FalseString,
            JsonValueKind.Null => string.Empty,
            _ => element.GetRawText()
        };
    }
}
