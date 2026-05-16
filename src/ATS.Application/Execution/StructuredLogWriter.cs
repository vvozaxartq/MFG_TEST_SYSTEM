using System.Text.Json;
using System.Text.Json.Serialization;
using ATS.Core.Models;

namespace ATS.Application.Execution;

internal sealed class StructuredLogWriter : IStructuredLogSink
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public void Append(string path, StructuredLogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(entry);

        var json = JsonSerializer.Serialize(entry, JsonOptions);
        File.AppendAllLines(path, new[] { json });
    }
}
