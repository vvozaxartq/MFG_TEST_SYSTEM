using System.Collections.Concurrent;

namespace ATS.Core.Models;

public sealed class DataCollection
{
    private readonly ConcurrentDictionary<string, string> _values = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, MeasurementItem> _items = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, string> Values => _values;

    public IReadOnlyDictionary<string, MeasurementItem> Items => _items;

    public void Set(string fullKey, string value)
    {
        if (string.IsNullOrWhiteSpace(fullKey))
        {
            throw new InvalidOperationException("Measurement fullKey is required.");
        }

        var separatorIndex = fullKey.LastIndexOf(".", StringComparison.Ordinal);
        var key = separatorIndex >= 0 ? fullKey[(separatorIndex + 1)..] : fullKey;
        var prefix = separatorIndex >= 0 ? fullKey[..separatorIndex] : string.Empty;

        _values[fullKey] = value;
        _items[fullKey] = new MeasurementItem
        {
            Key = key,
            Prefix = prefix,
            FullKey = fullKey,
            Value = value,
            ValueType = MeasurementValueType.Unknown,
            RawText = value
        };
    }

    public void Set(MeasurementItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (string.IsNullOrWhiteSpace(item.FullKey))
        {
            throw new InvalidOperationException("Measurement fullKey is required.");
        }

        _values[item.FullKey] = item.Value;
        _items[item.FullKey] = item;
    }

    public void Set(MeasurementSet measurementSet)
    {
        foreach (var item in measurementSet.Items)
        {
            Set(item);
        }
    }

    public bool TryGetValue(string fullKey, out string? value)
    {
        var found = _values.TryGetValue(fullKey, out var storedValue);
        value = storedValue;
        return found;
    }

    public bool TryGetItem(string fullKey, out MeasurementItem? item)
    {
        var found = _items.TryGetValue(fullKey, out var storedItem);
        item = storedItem;
        return found;
    }
}
