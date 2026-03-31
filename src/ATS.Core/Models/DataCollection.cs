namespace ATS.Core.Models;

public sealed class DataCollection
{
    private readonly Dictionary<string, string> _values = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, string> Values => _values;

    public void Set(string key, string value)
    {
        _values[key] = value;
    }

    public bool TryGetValue(string key, out string? value)
    {
        var found = _values.TryGetValue(key, out var storedValue);
        value = storedValue;
        return found;
    }
}
