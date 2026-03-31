using System.Globalization;
using System.Text;
using System.Text.Json;
using ATS.Core.Models;

namespace ATS.Application.Execution;

public sealed class SessionArtifactWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public void WriteTestResult(TestResult result, TestContext context)
    {
        WriteCommonArtifacts(
            context,
            JsonSerializer.Serialize(result, JsonOptions),
            BuildTestCsv(result));
    }

    public void WriteDeviceResult(DeviceCommandResult result, TestContext context)
    {
        WriteCommonArtifacts(
            context,
            JsonSerializer.Serialize(result, JsonOptions),
            BuildDeviceCsv(result));
    }

    public void WriteValidationResult(ValidationResult result, TestContext context)
    {
        WriteCommonArtifacts(
            context,
            JsonSerializer.Serialize(result, JsonOptions),
            BuildValidationCsv(result));
    }

    private static void WriteCommonArtifacts(TestContext context, string jsonPayload, string csvPayload)
    {
        File.WriteAllText(Path.Combine(context.OutputDirectory, "result.json"), jsonPayload);
        File.WriteAllText(Path.Combine(context.OutputDirectory, "result.csv"), csvPayload);
        File.WriteAllLines(Path.Combine(context.OutputDirectory, "session.log"), context.Logs);
    }

    private static string BuildTestCsv(TestResult result)
    {
        var builder = new StringBuilder();
        builder.AppendLine("script_name,command,measurement_key,spec_key,actual_value,numeric_value,unit,operator,expected,min,max,status,message");

        foreach (var script in result.Scripts)
        {
            builder.Append(EscapeCsv(script.ScriptName)).Append(',')
                .Append(EscapeCsv(script.Command)).Append(',')
                .Append(EscapeCsv(script.MeasurementKey)).Append(',')
                .Append(EscapeCsv(script.SpecKey)).Append(',')
                .Append(EscapeCsv(script.ActualValue)).Append(',')
                .Append(FormatNullableDecimal(script.NumericValue)).Append(',')
                .Append(EscapeCsv(script.Unit)).Append(',')
                .Append(EscapeCsv(script.Operator)).Append(',')
                .Append(EscapeCsv(script.Expected)).Append(',')
                .Append(FormatNullableDecimal(script.Minimum)).Append(',')
                .Append(FormatNullableDecimal(script.Maximum)).Append(',')
                .Append(EscapeCsv(script.Status)).Append(',')
                .Append(EscapeCsv(script.Message))
                .AppendLine();
        }

        foreach (var error in result.Errors)
        {
            builder.Append(",,,,,,,,,,,")
                .Append(EscapeCsv("Error"))
                .Append(',')
                .Append(EscapeCsv(error))
                .AppendLine();
        }

        return builder.ToString();
    }

    private static string BuildDeviceCsv(DeviceCommandResult result)
    {
        var builder = new StringBuilder();
        builder.AppendLine("device_name,command,response,status,message");
        builder.Append(EscapeCsv(result.DeviceName)).Append(',')
            .Append(EscapeCsv(result.Command)).Append(',')
            .Append(EscapeCsv(result.Response)).Append(',')
            .Append(EscapeCsv(result.Status)).Append(',')
            .Append(EscapeCsv(result.Message))
            .AppendLine();

        return builder.ToString();
    }

    private static string BuildValidationCsv(ValidationResult result)
    {
        var builder = new StringBuilder();
        builder.AppendLine("severity,message");

        foreach (var error in result.Errors)
        {
            builder.Append(EscapeCsv("Error")).Append(',').Append(EscapeCsv(error)).AppendLine();
        }

        foreach (var warning in result.Warnings)
        {
            builder.Append(EscapeCsv("Warning")).Append(',').Append(EscapeCsv(warning)).AppendLine();
        }

        if (result.Errors.Count == 0 && result.Warnings.Count == 0)
        {
            builder.Append(EscapeCsv("Info")).Append(',').Append(EscapeCsv("Validation passed.")).AppendLine();
        }

        return builder.ToString();
    }

    private static string EscapeCsv(string value)
    {
        return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
    }

    private static string FormatNullableDecimal(decimal? value)
    {
        return value.HasValue
            ? value.Value.ToString(CultureInfo.InvariantCulture)
            : string.Empty;
    }
}
