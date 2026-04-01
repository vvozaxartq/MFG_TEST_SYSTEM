using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ATS.Core.Models;

namespace ATS.Application.Execution;

public sealed class SessionArtifactWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
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
        builder.AppendLine("record_type,step_name,command,prefix,full_key,key,value,value_type,unit,rule_name,target_key,rule_type,pass_fail,error_code,message");

        foreach (var step in result.Steps)
        {
            foreach (var measurement in step.Measurements)
            {
                builder.Append(EscapeCsv("Measurement")).Append(',')
                    .Append(EscapeCsv(step.StepName)).Append(',')
                    .Append(EscapeCsv(step.Command)).Append(',')
                    .Append(EscapeCsv(step.Prefix)).Append(',')
                    .Append(EscapeCsv(measurement.FullKey)).Append(',')
                    .Append(EscapeCsv(measurement.Key)).Append(',')
                    .Append(EscapeCsv(measurement.Value)).Append(',')
                    .Append(EscapeCsv(measurement.ValueType.ToString())).Append(',')
                    .Append(EscapeCsv(measurement.Unit)).Append(',')
                    .Append(",,,,,")
                    .AppendLine();
            }

            foreach (var specResult in step.SpecResults)
            {
                builder.Append(EscapeCsv("SpecResult")).Append(',')
                    .Append(EscapeCsv(step.StepName)).Append(',')
                    .Append(EscapeCsv(step.Command)).Append(',')
                    .Append(EscapeCsv(step.Prefix)).Append(',')
                    .Append(",,,,")
                    .Append(EscapeCsv(specResult.RuleName)).Append(',')
                    .Append(EscapeCsv(specResult.TargetKey)).Append(',')
                    .Append(EscapeCsv(specResult.RuleType)).Append(',')
                    .Append(EscapeCsv(specResult.PassFail)).Append(',')
                    .Append(EscapeCsv(specResult.ErrorCode)).Append(',')
                    .Append(EscapeCsv(specResult.Reason))
                    .AppendLine();
            }
        }

        foreach (var script in result.Scripts)
        {
            builder.Append(EscapeCsv("Summary")).Append(',')
                .Append(EscapeCsv(script.ScriptName)).Append(',')
                .Append(EscapeCsv(script.Command)).Append(',')
                .Append(EscapeCsv(script.Prefix)).Append(',')
                .Append(EscapeCsv(script.FullKey)).Append(',')
                .Append(EscapeCsv(script.MeasurementKey)).Append(',')
                .Append(EscapeCsv(script.ActualValue)).Append(',')
                .Append(EscapeCsv(script.NumericValue.HasValue ? "Number" : string.Empty)).Append(',')
                .Append(EscapeCsv(script.Unit)).Append(',')
                .Append(EscapeCsv(script.RuleName)).Append(',')
                .Append(EscapeCsv(script.SpecKey)).Append(',')
                .Append(EscapeCsv(script.Operator)).Append(',')
                .Append(EscapeCsv(script.Status)).Append(',')
                .Append(EscapeCsv(script.ErrorCode)).Append(',')
                .Append(EscapeCsv(script.Message))
                .AppendLine();
        }

        foreach (var error in result.Errors)
        {
            builder.Append(EscapeCsv("Error")).Append(",,,,,,,,,,,,,")
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
}
