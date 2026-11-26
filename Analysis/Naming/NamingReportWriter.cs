using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>
/// Serializes naming-analysis reports to portable formats.
/// </summary>
public static class NamingReportWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes a report as indented JSON.
    /// </summary>
    public static string ToJson(NamingAnalysisReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        return JsonSerializer.Serialize(report, JsonOptions);
    }

    /// <summary>
    /// Serializes report violations as CSV.
    /// </summary>
    public static string ViolationsToCsv(NamingAnalysisReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        StringBuilder builder = new("Project,Name,ExpectedStyle,ActualStyle,Severity,Confidence,File,Line,Message");
        builder.AppendLine();
        foreach (NamingViolation violation in report.Violations)
        {
            builder.AppendLine(string.Join(',',
                Csv(violation.Project?.Name), Csv(violation.Name), violation.ExpectedStyle, violation.ActualStyle,
                violation.Severity, violation.Confidence.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture),
                Csv(violation.Location?.FilePath), violation.Location?.StartLine.ToString() ?? string.Empty, Csv(violation.Message)));
        }
        return builder.ToString();
    }

    private static string Csv(string? value) => value is null ? string.Empty : $"\"{value.Replace("\"", "\"\"")}\"";
}
