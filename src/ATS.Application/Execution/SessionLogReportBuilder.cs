using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using ATS.Core.Models;

namespace ATS.Application.Execution;

internal static class SessionLogReportBuilder
{
    public static IReadOnlyList<string> BuildSessionHeader(TestContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var entryAssembly = Assembly.GetEntryAssembly() ?? typeof(SessionLogReportBuilder).Assembly;
        var assemblyPath = entryAssembly.Location;
        var applicationReleaseTime = File.Exists(assemblyPath)
            ? File.GetLastWriteTime(assemblyPath).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
            : "N/A";
        var applicationVersion = entryAssembly.GetName().Version?.ToString() ?? "N/A";
        var configPath = !string.IsNullOrWhiteSpace(context.RecipePath)
            ? context.RecipePath
            : context.SpecPath;
        var configLastWriteTime = File.Exists(configPath)
            ? File.GetLastWriteTime(configPath).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
            : "N/A";
        var networkInfo = GetPrimaryNetworkInfo();
        var loginUser = ResolveInputValue(context.RunInput, "LoginUser");
        var memoryBytes = GetPhysicalMemoryBytes();
        var lines = new List<string>
        {
            "========================= Session Header =========================",
            FormatHeaderLine("Session ID", context.SessionId),
            FormatHeaderLine("Command Name", context.CommandName),
            FormatHeaderLine("Session Start Time", FormatLocalTimestamp(context.StartedAtUtc)),
            FormatHeaderLine("Recipe File", string.IsNullOrWhiteSpace(context.RecipePath) ? "N/A" : Path.GetFileName(context.RecipePath)),
            FormatHeaderLine("Spec File", string.IsNullOrWhiteSpace(context.SpecPath) ? "N/A" : Path.GetFileName(context.SpecPath)),
            FormatHeaderLine("Station", FormatDisplayValue(context.RunInput.Station)),
            FormatHeaderLine("Mode", FormatDisplayValue(context.RunInput.Mode)),
            FormatHeaderLine("Product Serial Number", FormatDisplayValue(context.RunInput.SerialNumber)),
            FormatHeaderLine("Login User", loginUser),
            FormatHeaderLine("Output Folder", context.OutputDirectory),
            FormatHeaderLine("Session Log Path", context.ArtifactPaths.SessionLogPath),
            FormatHeaderLine("Structured Log Path", context.ArtifactPaths.StructuredLogPath),
            string.Empty,
            "==================== Environment Information ====================",
            FormatHeaderLine("Application Release Time", applicationReleaseTime),
            FormatHeaderLine("Application Version", applicationVersion),
            FormatHeaderLine("Test Config File Name", string.IsNullOrWhiteSpace(configPath) ? "N/A" : Path.GetFileName(configPath)),
            FormatHeaderLine("Test Config File LastWriteTime", configLastWriteTime),
            FormatHeaderLine("Test Computer Name", Environment.MachineName),
            FormatHeaderLine("Test Computer User Name", Environment.UserName),
            FormatHeaderLine("Test Computer Operating System Bit", Environment.Is64BitOperatingSystem ? "64" : "32"),
            FormatHeaderLine("Test Process System Bit", Environment.Is64BitProcess ? "64" : "32"),
            FormatHeaderLine("CPU Info", $"{ResolveCpuInfo()}, Cores: {Environment.ProcessorCount}"),
            FormatHeaderLine("Memory", FormatGigabytes(memoryBytes)),
            FormatHeaderLine("Local IP Address", networkInfo.IpAddress),
            FormatHeaderLine($"MAC Address ({networkInfo.InterfaceType} / {networkInfo.Description})", networkInfo.MacAddress),
            string.Empty
        };

        AppendInputValues(lines, context.RunInput);
        lines.Add("========================= Runtime Logs =========================");
        lines.Add(string.Empty);
        return lines;
    }

    public static IReadOnlyList<string> BuildTestSummary(TestResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var lines = new List<string>
        {
            string.Empty,
            "========================= Session Result =========================",
            FormatHeaderLine("Recipe Name", FormatDisplayValue(result.RecipeName)),
            FormatHeaderLine("Recipe Path", FormatDisplayValue(result.RecipePath)),
            FormatHeaderLine("Spec Path", FormatDisplayValue(result.SpecPath)),
            FormatHeaderLine("Station", FormatDisplayValue(result.RunInput.Station)),
            FormatHeaderLine("Mode", FormatDisplayValue(result.RunInput.Mode)),
            FormatHeaderLine("Product Serial Number", FormatDisplayValue(result.RunInput.SerialNumber)),
            FormatHeaderLine("Session Start Time", FormatLocalTimestamp(result.StartedAtUtc)),
            FormatHeaderLine("Session End Time", FormatLocalTimestamp(result.CompletedAtUtc)),
            FormatHeaderLine("Duration", FormatElapsed(result.CompletedAtUtc - result.StartedAtUtc)),
            FormatHeaderLine("Final Result", NormalizeStatus(result.Status)),
            FormatHeaderLine("Output Folder", result.OutputDirectory),
            FormatHeaderLine("Result JSON Path", result.ResultJsonPath),
            FormatHeaderLine("Result CSV Path", result.ResultCsvPath),
            FormatHeaderLine("Session Log Path", result.SessionLogPath),
            FormatHeaderLine("Structured Log Path", result.StructuredLogPath),
        };

        AppendInputValues(lines, result.RunInput);
        lines.Add("======================= Test Summary =======================");
        lines.Add($"Total Items : {result.Steps.Count}");
        lines.Add($"PASS        : {result.Steps.Count(item => string.Equals(item.FinalStatus, "Passed", StringComparison.OrdinalIgnoreCase))}");
        lines.Add($"FAIL        : {result.Steps.Count(item => string.Equals(item.FinalStatus, "Failed", StringComparison.OrdinalIgnoreCase))}");
        lines.Add($"ERROR       : {result.Steps.Count(item => string.Equals(item.FinalStatus, "Error", StringComparison.OrdinalIgnoreCase))}");
        lines.Add($"Total Time  : {FormatElapsed(result.CompletedAtUtc - result.StartedAtUtc)}");
        lines.Add(string.Empty);
        lines.Add("No   Item                                Result   TestTime(s) Elapse       Retry   Data");

        for (var index = 0; index < result.Steps.Count; index++)
        {
            var step = result.Steps[index];
            var elapsed = step.CompletedAtUtc - result.StartedAtUtc;
            lines.Add(
                $"{index,-4} {Limit(step.StepName, 34),-34} {NormalizeStatus(step.FinalStatus),-8} {step.DurationSeconds.ToString("0.###", CultureInfo.InvariantCulture),-11} {FormatElapsed(elapsed),-12} {step.RetryCount,-7} {BuildMeasurementSummary(step.Measurements)}");
        }

        if (result.Steps.Count == 0)
        {
            lines.Add("N/A  No step results available.");
        }

        return lines;
    }

    public static IReadOnlyList<string> BuildDeviceSummary(DeviceCommandResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var lines = new List<string>
        {
            string.Empty,
            "========================= Session Result =========================",
            FormatHeaderLine("Command", result.CommandName),
            FormatHeaderLine("Station", FormatDisplayValue(result.RunInput.Station)),
            FormatHeaderLine("Mode", FormatDisplayValue(result.RunInput.Mode)),
            FormatHeaderLine("Product Serial Number", FormatDisplayValue(result.RunInput.SerialNumber)),
            FormatHeaderLine("Session Start Time", FormatLocalTimestamp(result.StartedAtUtc)),
            FormatHeaderLine("Session End Time", FormatLocalTimestamp(result.CompletedAtUtc)),
            FormatHeaderLine("Duration", FormatElapsed(result.CompletedAtUtc - result.StartedAtUtc)),
            FormatHeaderLine("Final Result", NormalizeStatus(result.Status)),
            FormatHeaderLine("Output Folder", result.OutputDirectory),
            FormatHeaderLine("Result JSON Path", result.ResultJsonPath),
            FormatHeaderLine("Result CSV Path", result.ResultCsvPath),
            FormatHeaderLine("Session Log Path", result.SessionLogPath),
            FormatHeaderLine("Structured Log Path", result.StructuredLogPath),
        };

        AppendInputValues(lines, result.RunInput);
        lines.Add("======================= Command Summary =======================");
        lines.Add($"Command:{result.Command}");
        lines.Add($"Result:{NormalizeStatus(result.Status)}");
        lines.Add($"Duration:{result.DurationSeconds.ToString("0.###", CultureInfo.InvariantCulture)}");
        lines.Add($"Response:{result.Response}");
        return lines;
    }

    public static IReadOnlyList<string> BuildValidationSummary(ValidationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var lines = new List<string>
        {
            string.Empty,
            "========================= Session Result =========================",
            FormatHeaderLine("Command", result.CommandName),
            FormatHeaderLine("Station", FormatDisplayValue(result.RunInput.Station)),
            FormatHeaderLine("Mode", FormatDisplayValue(result.RunInput.Mode)),
            FormatHeaderLine("Product Serial Number", FormatDisplayValue(result.RunInput.SerialNumber)),
            FormatHeaderLine("Session Start Time", FormatLocalTimestamp(result.StartedAtUtc)),
            FormatHeaderLine("Session End Time", FormatLocalTimestamp(result.CompletedAtUtc)),
            FormatHeaderLine("Duration", FormatElapsed(result.CompletedAtUtc - result.StartedAtUtc)),
            FormatHeaderLine("Final Result", NormalizeStatus(result.Status)),
            FormatHeaderLine("Output Folder", result.OutputDirectory),
            FormatHeaderLine("Result JSON Path", result.ResultJsonPath),
            FormatHeaderLine("Result CSV Path", result.ResultCsvPath),
            FormatHeaderLine("Session Log Path", result.SessionLogPath),
            FormatHeaderLine("Structured Log Path", result.StructuredLogPath),
        };

        AppendInputValues(lines, result.RunInput);
        lines.Add("======================= Validation Summary =======================");
        lines.Add($"Type:{result.ValidationType}");
        lines.Add($"Result:{NormalizeStatus(result.Status)}");
        lines.Add($"Duration:{result.DurationSeconds.ToString("0.###", CultureInfo.InvariantCulture)}");
        lines.Add($"Errors:{result.Errors.Count}");
        lines.Add($"Warnings:{result.Warnings.Count}");
        return lines;
    }

    private static string ResolveInputValue(RunInputModel runInput, string variableName)
    {
        return runInput.Values.TryGetValue(variableName, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : "N/A";
    }

    private static string ResolveCpuInfo()
    {
        return Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER")
               ?? RuntimeInformation.ProcessArchitecture.ToString();
    }

    private static ulong GetPhysicalMemoryBytes()
    {
        return (ulong)Math.Max(GC.GetGCMemoryInfo().TotalAvailableMemoryBytes, 0);
    }

    private static string FormatGigabytes(ulong bytes)
    {
        var gigaBytes = bytes / 1024d / 1024d / 1024d;
        return $"{gigaBytes:0.0} GB";
    }

    private static (string IpAddress, string MacAddress, string Description, string InterfaceType) GetPrimaryNetworkInfo()
    {
        foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (networkInterface.OperationalStatus != OperationalStatus.Up)
            {
                continue;
            }

            var ipProperties = networkInterface.GetIPProperties();
            var address = ipProperties.UnicastAddresses
                .FirstOrDefault(item =>
                    item.Address.AddressFamily == AddressFamily.InterNetwork &&
                    !IPAddress.IsLoopback(item.Address));

            if (address is null)
            {
                continue;
            }

            return (
                address.Address.ToString(),
                FormatMacAddress(networkInterface.GetPhysicalAddress()),
                networkInterface.Description,
                networkInterface.NetworkInterfaceType.ToString());
        }

        return ("N/A", "N/A", "N/A", "N/A");
    }

    private static string FormatMacAddress(PhysicalAddress physicalAddress)
    {
        var raw = physicalAddress.ToString();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return "N/A";
        }

        return string.Join("-", raw.Chunk(2).Select(part => new string(part)));
    }

    private static string BuildMeasurementSummary(IReadOnlyCollection<MeasurementItem> measurements)
    {
        if (measurements.Count == 0)
        {
            return "N/A";
        }

        var summary = string.Join(
            "; ",
            measurements.Select(item =>
            {
                var unitSuffix = string.IsNullOrWhiteSpace(item.Unit) ? string.Empty : $" {item.Unit}";
                return $"{item.FullKey}={item.Value}{unitSuffix}";
            }));

        return Limit(summary, 120);
    }

    private static string Limit(string value, int maxLength)
    {
        return value.Length <= maxLength
            ? value
            : $"{value[..Math.Max(0, maxLength - 3)]}...";
    }

    private static string FormatHeaderLine(string label, string value)
    {
        return $"{label,-32}: {value}";
    }

    private static string FormatDisplayValue(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "N/A" : value;
    }

    private static string FormatLocalTimestamp(DateTimeOffset timestamp)
    {
        return timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
    }

    private static void AppendInputValues(List<string> lines, RunInputModel runInput)
    {
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentNullException.ThrowIfNull(runInput);

        lines.Add("========================== Input Values ==========================");
        lines.Add(FormatHeaderLine("SN", FormatDisplayValue(runInput.SerialNumber)));

        foreach (var pair in runInput.Values.OrderBy(item => item.Key, StringComparer.OrdinalIgnoreCase))
        {
            if (string.Equals(pair.Key, "SN", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(pair.Key, "ProductSN", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            lines.Add(FormatHeaderLine(pair.Key, FormatDisplayValue(pair.Value)));
        }

        lines.Add(string.Empty);
    }

    private static string NormalizeStatus(string status)
    {
        return status.ToUpperInvariant() switch
        {
            "PASSED" => "PASS",
            "FAILED" => "FAIL",
            "ERROR" => "ERROR",
            "INVALID" => "INVALID",
            _ => status.ToUpperInvariant()
        };
    }

    private static string FormatElapsed(TimeSpan elapsed)
    {
        return $"{(int)elapsed.TotalHours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}.{elapsed.Milliseconds:000}";
    }
}
