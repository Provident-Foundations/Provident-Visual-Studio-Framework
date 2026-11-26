namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>
/// Provides a stable report projection for naming-analysis results.
/// </summary>
public sealed class NamingAnalysisReport : Provident.Foundations.Abstractions.DisposableObjectBase
{
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    /// <summary>
    /// Gets the report schema version.
    /// </summary>
    public string SchemaVersion { get; init; } = "1.0";

    /// <summary>
    /// Gets the report generation time.
    /// </summary>
    public DateTimeOffset GeneratedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the analysis target.
    /// </summary>
    public NamingAnalysisTarget Target { get; init; } = new();

    /// <summary>
    /// Gets the completion status.
    /// </summary>
    public NamingAnalysisStatus Status { get; init; }

    /// <summary>
    /// Gets aggregate statistics.
    /// </summary>
    public NamingStatistics Statistics { get; init; } = new();

    /// <summary>
    /// Gets inferred patterns.
    /// </summary>
    public IReadOnlyList<NamingPattern> Patterns { get; init; } = Array.Empty<NamingPattern>();

    /// <summary>
    /// Gets naming violations.
    /// </summary>
    public IReadOnlyList<NamingViolation> Violations { get; init; } = Array.Empty<NamingViolation>();

    /// <summary>
    /// Gets diagnostics.
    /// </summary>
    public IReadOnlyList<NamingDiagnostic> Diagnostics { get; init; } = Array.Empty<NamingDiagnostic>();

    /// <summary>
    /// Gets project-level report entries.
    /// </summary>
    public IReadOnlyList<NamingReportProjectEntry> Projects { get; init; } = Array.Empty<NamingReportProjectEntry>();

    /// <summary>
    /// Creates a report projection from a project result.
    /// </summary>
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

    /// <summary>
    /// Creates a report projection from a solution result.
    /// </summary>
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
