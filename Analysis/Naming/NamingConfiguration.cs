namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>Configures naming analysis behavior.</summary>
public sealed class NamingAnalysisConfiguration
{
    /// <summary>Gets the default configuration.</summary>
    public static NamingAnalysisConfiguration Default { get; } = new();
    /// <summary>Gets the minimum observations required to infer a pattern.</summary>
    public int MinimumObservationsPerPattern { get; init; } = 3;
    /// <summary>Gets the minimum dominance percentage for a common pattern.</summary>
    public double MinimumDominancePercentage { get; init; } = 0.6;
    /// <summary>Gets the minimum confidence required for a violation.</summary>
    public double MinimumViolationConfidence { get; init; } = 0.7;
    /// <summary>Gets whether references are collected.</summary>
    public bool IncludeReferences { get; init; } = true;
    /// <summary>Gets whether generated files are analyzed.</summary>
    public bool IncludeGeneratedFiles { get; init; }
    /// <summary>Gets whether XAML identifiers are analyzed.</summary>
    public bool IncludeXaml { get; init; } = true;
    /// <summary>Gets the solution pattern weighting.</summary>
    public NamingPatternWeighting PatternWeighting { get; init; } = NamingPatternWeighting.Both;
    /// <summary>Gets excluded file patterns.</summary>
    public IReadOnlyList<string> ExcludedFilePatterns { get; init; } = Array.Empty<string>();
    /// <summary>Gets excluded namespace prefixes.</summary>
    public IReadOnlyList<string> ExcludedNamespacePrefixes { get; init; } = Array.Empty<string>();
    /// <summary>Gets known framework or generated name prefixes.</summary>
    public IReadOnlyList<string> KnownPrefixes { get; init; } = Array.Empty<string>();
    /// <summary>Gets known framework or generated name suffixes.</summary>
    public IReadOnlyList<string> KnownSuffixes { get; init; } = Array.Empty<string>();
}

/// <summary>Defines an intentional naming-analysis exception.</summary>
public sealed class NamingSuppression
{
    /// <summary>Gets the suppression key.</summary>
    public string Key { get; init; } = string.Empty;
    /// <summary>Gets the suppression scope.</summary>
    public NamingAnalysisScope Scope { get; init; }
    /// <summary>Gets the matching project, namespace, file, or symbol.</summary>
    public string Match { get; init; } = string.Empty;
    /// <summary>Gets the reason for the suppression.</summary>
    public string Reason { get; init; } = string.Empty;
    /// <summary>Gets the author of the suppression.</summary>
    public string? Author { get; init; }
    /// <summary>Gets the optional expiration date.</summary>
    public DateTimeOffset? Expires { get; init; }
}
