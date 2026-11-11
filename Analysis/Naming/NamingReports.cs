using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>Provides a stable report projection for naming-analysis results.</summary>
public sealed class NamingAnalysisReport
{
    /// <summary>Gets the report schema version.</summary>
    public string SchemaVersion { get; init; } = "1.0";
    /// <summary>Gets the report generation time.</summary>
    public DateTimeOffset GeneratedAt { get; init; } = DateTimeOffset.UtcNow;
    /// <summary>Gets the analysis target.</summary>
    public NamingAnalysisTarget Target { get; init; } = new();
    /// <summary>Gets the completion status.</summary>
    public NamingAnalysisStatus Status { get; init; }
    /// <summary>Gets aggregate statistics.</summary>
    public NamingStatistics Statistics { get; init; } = new();
    /// <summary>Gets inferred patterns.</summary>
    public IReadOnlyList<NamingPattern> Patterns { get; init; } = Array.Empty<NamingPattern>();
    /// <summary>Gets naming violations.</summary>
    public IReadOnlyList<NamingViolation> Violations { get; init; } = Array.Empty<NamingViolation>();
    /// <summary>Gets diagnostics.</summary>
    public IReadOnlyList<NamingDiagnostic> Diagnostics { get; init; } = Array.Empty<NamingDiagnostic>();
    /// <summary>Gets project-level report entries.</summary>
    public IReadOnlyList<NamingReportProjectEntry> Projects { get; init; } = Array.Empty<NamingReportProjectEntry>();

    /// <summary>Creates a report projection from a project result.</summary>
    public static NamingAnalysisReport From(NamingProjectAnalysisResult result) => new()
    {
        Target = result.Target,
        Status = result.Status,
        Statistics = result.Statistics,
        Patterns = result.Patterns,
        Violations = result.Violations,
        Diagnostics = result.Diagnostics,
        Projects = new[] { NamingReportProjectEntry.From(result) }
    };

    /// <summary>Creates a report projection from a solution result.</summary>
    public static NamingAnalysisReport From(NamingSolutionAnalysisResult result) => new()
    {
        Target = result.Target,
        Status = result.Status,
        Statistics = result.Statistics,
        Patterns = result.Patterns,
        Violations = result.Violations,
        Diagnostics = result.Diagnostics,
        Projects = result.Projects.Where(item => item.Result is not null).Select(item => NamingReportProjectEntry.From(item.Result!)).ToArray()
    };
}

/// <summary>Provides project summary data for a report.</summary>
public sealed class NamingReportProjectEntry
{
    /// <summary>Gets the project name.</summary>
    public string ProjectName { get; init; } = string.Empty;
    /// <summary>Gets the project path.</summary>
    public string? ProjectPath { get; init; }
    /// <summary>Gets the project status.</summary>
    public NamingAnalysisStatus Status { get; init; }
    /// <summary>Gets project statistics.</summary>
    public NamingStatistics Statistics { get; init; } = new();
    /// <summary>Gets the project violation count.</summary>
    public int ViolationCount { get; init; }
    /// <summary>Gets the project pattern count.</summary>
    public int PatternCount { get; init; }

    /// <summary>Creates a project report entry.</summary>
    public static NamingReportProjectEntry From(NamingProjectAnalysisResult result) => new()
    {
        ProjectName = result.Project.Name,
        ProjectPath = result.Project.FilePath,
        Status = result.Status,
        Statistics = result.Statistics,
        ViolationCount = result.Violations.Count,
        PatternCount = result.Patterns.Count
    };
}

/// <summary>Serializes naming-analysis reports to portable formats.</summary>
public static class NamingReportWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>Serializes a report as indented JSON.</summary>
    public static string ToJson(NamingAnalysisReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        return JsonSerializer.Serialize(report, JsonOptions);
    }

    /// <summary>Serializes report violations as CSV.</summary>
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
