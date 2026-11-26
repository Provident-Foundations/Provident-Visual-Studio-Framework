namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>
/// Provides project summary data for a report.
/// </summary>
public sealed class NamingReportProjectEntry : Provident.Foundations.Abstractions.DisposableObjectBase
{
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    /// <summary>
    /// Gets the project name.
    /// </summary>
    public string ProjectName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the project path.
    /// </summary>
    public string? ProjectPath { get; init; }

    /// <summary>
    /// Gets the project status.
    /// </summary>
    public NamingAnalysisStatus Status { get; init; }

    /// <summary>
    /// Gets project statistics.
    /// </summary>
    public NamingStatistics Statistics { get; init; } = new();

    /// <summary>
    /// Gets the project violation count.
    /// </summary>
    public int ViolationCount { get; init; }

    /// <summary>
    /// Gets the project pattern count.
    /// </summary>
    public int PatternCount { get; init; }

    /// <summary>
    /// Creates a project report entry.
    /// </summary>
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
