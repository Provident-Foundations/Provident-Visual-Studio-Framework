using Provident.Foundations.Abstractions;

namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>
/// Contains the result of analyzing naming conventions across a solution.
/// </summary>
public sealed class NamingSolutionAnalysisResult : DisposableObjectBase
{
    /// <summary>
    /// Cleans up the resources used by the object.
    /// </summary>
    /// <param name="disposing">
    /// A value indicating whether to release managed resources.
    /// </param>
    protected override void Dispose(bool disposing)
    {
        if (!IsDisposed && disposing)
        {
            Target.Dispose();
            foreach (NamingSolutionProjectResult project in Projects)
            {
                project.Dispose();
            }

            foreach (NamingPattern pattern in Patterns)
            {
                pattern.Dispose();
            }

            foreach (NamingViolation violation in Violations)
            {
                violation.Dispose();
            }

            Statistics.Dispose();
            Index.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// Gets the solution analysis target.
    /// </summary>
    public NamingAnalysisTarget Target { get; init; } = new();

    /// <summary>
    /// Gets the results for all projects in the solution.
    /// </summary>
    public IReadOnlyList<NamingSolutionProjectResult> Projects { get; init; } = Array.Empty<NamingSolutionProjectResult>();

    /// <summary>
    /// Gets observations collected from analyzed projects.
    /// </summary>
    public IReadOnlyList<NamingObservation> Observations { get; init; } = Array.Empty<NamingObservation>();

    /// <summary>
    /// Gets naming patterns inferred from the collected observations.
    /// </summary>
    public IReadOnlyList<NamingPattern> Patterns { get; init; } = Array.Empty<NamingPattern>();

    /// <summary>
    /// Gets naming violations found in the collected observations.
    /// </summary>
    public IReadOnlyList<NamingViolation> Violations { get; init; } = Array.Empty<NamingViolation>();

    /// <summary>
    /// Gets statistics aggregated across the solution.
    /// </summary>
    public NamingStatistics Statistics { get; init; } = new();

    /// <summary>
    /// Gets lookup indexes for the collected observations.
    /// </summary>
    public NamingAnalysisIndex Index { get; init; } = new();

    /// <summary>
    /// Gets diagnostics reported during solution analysis.
    /// </summary>
    public IReadOnlyList<NamingDiagnostic> Diagnostics { get; init; } = Array.Empty<NamingDiagnostic>();

    /// <summary>
    /// Gets the overall analysis status.
    /// </summary>
    public NamingAnalysisStatus Status { get; init; }

    /// <summary>
    /// Gets the elapsed time for solution analysis.
    /// </summary>
    public TimeSpan Duration { get; init; }
}
