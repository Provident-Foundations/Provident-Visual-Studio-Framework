namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>
/// Configures naming analysis behavior.
/// </summary>
public sealed class NamingAnalysisConfiguration : Provident.Foundations.Abstractions.DisposableObjectBase
{
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    /// <summary>
    /// Gets the default configuration.
    /// </summary>
    public static NamingAnalysisConfiguration Default { get; } = new();
    /// <summary>
    /// Gets the minimum observations required to infer a pattern.
    /// </summary>
    public int MinimumObservationsPerPattern { get; init; } = 3;
    /// <summary>
    /// Gets the minimum dominance percentage for a common pattern.
    /// </summary>
    public double MinimumDominancePercentage { get; init; } = 0.6;
    /// <summary>
    /// Gets the minimum confidence required for a violation.
    /// </summary>
    public double MinimumViolationConfidence { get; init; } = 0.7;
    /// <summary>
    /// Gets whether references are collected.
    /// </summary>
    public bool IncludeReferences { get; init; } = true;
    /// <summary>
    /// Gets whether generated files are analyzed.
    /// </summary>
    public bool IncludeGeneratedFiles { get; init; }
    /// <summary>
    /// Gets whether XAML identifiers are analyzed.
    /// </summary>
    public bool IncludeXaml { get; init; } = true;
    /// <summary>
    /// Gets the solution pattern weighting.
    /// </summary>
    public NamingPatternWeighting PatternWeighting { get; init; } = NamingPatternWeighting.Both;
    /// <summary>
    /// Gets excluded file patterns.
    /// </summary>
    public IReadOnlyList<string> ExcludedFilePatterns { get; init; } = Array.Empty<string>();
    /// <summary>
    /// Gets excluded namespace prefixes.
    /// </summary>
    public IReadOnlyList<string> ExcludedNamespacePrefixes { get; init; } = Array.Empty<string>();
    /// <summary>
    /// Gets known framework or generated name prefixes.
    /// </summary>
    public IReadOnlyList<string> KnownPrefixes { get; init; } = Array.Empty<string>();
    /// <summary>
    /// Gets known framework or generated name suffixes.
    /// </summary>
    public IReadOnlyList<string> KnownSuffixes { get; init; } = Array.Empty<string>();
}
