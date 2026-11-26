namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>
/// Represents an inferred naming convention.
/// </summary>
public sealed class NamingPattern : Provident.Foundations.Abstractions.DisposableObjectBase
{
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    /// <summary>
    /// Gets the stable pattern identifier.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the pattern scope.
    /// </summary>
    public NamingPatternScope Scope { get; init; } = new();

    /// <summary>
    /// Gets the dominant style.
    /// </summary>
    public NamingStyle DominantStyle { get; init; }

    /// <summary>
    /// Gets the common prefixes.
    /// </summary>
    public IReadOnlyList<string> CommonPrefixes { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the common suffixes.
    /// </summary>
    public IReadOnlyList<string> CommonSuffixes { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the supporting observations.
    /// </summary>
    public IReadOnlyList<string> ObservationIds { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the number of applicable observations.
    /// </summary>
    public int ObservationCount { get; init; }

    /// <summary>
    /// Gets the number matching the dominant pattern.
    /// </summary>
    public int MatchingCount { get; init; }

    /// <summary>
    /// Gets the number not matching the dominant pattern.
    /// </summary>
    public int NonMatchingCount { get; init; }

    /// <summary>
    /// Gets the dominance percentage.
    /// </summary>
    public double DominancePercentage { get; init; }

    /// <summary>
    /// Gets the confidence score.
    /// </summary>
    public double Confidence { get; init; }

    /// <summary>
    /// Gets whether multiple styles are materially represented.
    /// </summary>
    public bool IsMixed { get; init; }

    /// <summary>
    /// Gets whether there was insufficient evidence.
    /// </summary>
    public bool HasInsufficientEvidence { get; init; }
}
