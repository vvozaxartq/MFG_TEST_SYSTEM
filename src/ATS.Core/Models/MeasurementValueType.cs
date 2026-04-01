using System.Text.Json.Serialization;

namespace ATS.Core.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MeasurementValueType
{
    Unknown,
    Number,
    Text,
    Boolean
}
