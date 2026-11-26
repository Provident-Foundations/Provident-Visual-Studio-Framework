namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>
/// Contains the result of analyzing one project.
/// </summary>
public sealed class NamingProjectAnalysisResult : Provident.Foundations.Abstractions.DisposableObjectBase
{
    protected override void Dispose(bool disposing)
    {
        if (!IsDisposed && disposing)
        {
            Target.Dispose();
            foreach (NamingSourceFileIdentity file in Files)
            {
                file.Dispose();
            }

            foreach (NamingObservation observation in Observations)
            {
                observation.Dispose();
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
            foreach (NamingDiagnostic diagnostic in Diagnostics)
            {
                diagnostic.Dispose();
            }
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// Gets the project identity.
    /// </summary>
    public NamingProjectIdentity Project { get; init; } = new();

    /// <summary>
    /// Gets the analysis target.
    /// </summary>
    public NamingAnalysisTarget Target { get; init; } = new();

    /// <summary>
    /// Gets the completion status.
    /// </summary>
    public NamingAnalysisStatus Status { get; init; }

    /// <summary>
    /// Gets source files.
    /// </summary>
    public IReadOnlyList<NamingSourceFileIdentity> Files { get; init; } = Array.Empty<NamingSourceFileIdentity>();

    /// <summary>
    /// Gets name observations.
    /// </summary>
    public IReadOnlyList<NamingObservation> Observations { get; init; } = Array.Empty<NamingObservation>();

    /// <summary>
    /// Gets inferred patterns.
    /// </summary>
    public IReadOnlyList<NamingPattern> Patterns { get; init; } = Array.Empty<NamingPattern>();

    /// <summary>
    /// Gets violations.
    /// </summary>
    public IReadOnlyList<NamingViolation> Violations { get; init; } = Array.Empty<NamingViolation>();

    /// <summary>
    /// Gets aggregate statistics.
    /// </summary>
    public NamingStatistics Statistics { get; init; } = new();

    /// <summary>
    /// Gets lookup indexes.
    /// </summary>
    public NamingAnalysisIndex Index { get; init; } = new();

    /// <summary>
    /// Gets diagnostics.
    /// </summary>
    public IReadOnlyList<NamingDiagnostic> Diagnostics { get; init; } = Array.Empty<NamingDiagnostic>();

    /// <summary>
    /// Gets the analysis duration.
    /// </summary>
    public TimeSpan Duration { get; init; }
}
