namespace ATS.Core.Models;

public sealed class DataCollection
{
    private readonly Dictionary<string, string> _values = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, MeasurementItem> _items = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, string> Values => _values;

    public IReadOnlyDictionary<string, MeasurementItem> Items => _items;

    public void Set(string key, string value)
    {
        _values[key] = value;
        _items[key] = new MeasurementItem
        {
            Key = key,
            Prefix = string.Empty,
            FullKey = key,
            Value = value,
            ValueType = MeasurementValueType.Unknown,
            RawText = value
        };
    }

    public void Set(MeasurementItem item)
    {
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

    public bool TryGetValue(string key, out string? value)
    {
        var found = _values.TryGetValue(key, out var storedValue);
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
