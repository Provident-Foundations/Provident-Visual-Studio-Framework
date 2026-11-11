namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>Represents the tokenized form of an identifier.</summary>
public sealed class NamingTokenization
{
    /// <summary>Gets the original identifier.</summary>
    public string Original { get; init; } = string.Empty;
    /// <summary>Gets the ordered identifier tokens.</summary>
    public IReadOnlyList<string> Tokens { get; init; } = Array.Empty<string>();
    /// <summary>Gets the leading prefix.</summary>
    public string? Prefix { get; init; }
    /// <summary>Gets the trailing suffix.</summary>
    public string? Suffix { get; init; }
    /// <summary>Gets whether acronym tokens were detected.</summary>
    public bool ContainsAcronym { get; init; }
    /// <summary>Gets whether numeric tokens were detected.</summary>
    public bool ContainsNumericToken { get; init; }
    /// <summary>Gets separator characters encountered in the identifier.</summary>
    public IReadOnlyList<char> Separators { get; init; } = Array.Empty<char>();
}

/// <summary>Describes the naming characteristics of one identifier.</summary>
public sealed class NamingClassification
{
    /// <summary>Gets the primary naming style.</summary>
    public NamingStyle Style { get; init; }
    /// <summary>Gets classification confidence from zero through one.</summary>
    public double Confidence { get; init; }
    /// <summary>Gets the tokenization result.</summary>
    public NamingTokenization Tokenization { get; init; } = new();
    /// <summary>Gets whether the identifier has a leading underscore.</summary>
    public bool HasLeadingUnderscore { get; init; }
    /// <summary>Gets whether the identifier has a trailing asynchronous suffix.</summary>
    public bool HasAsyncSuffix { get; init; }
    /// <summary>Gets whether the identifier appears abbreviated.</summary>
    public bool AppearsAbbreviated { get; init; }
    /// <summary>Gets the identifier's length.</summary>
    public int Length { get; init; }
}

/// <summary>Represents one extracted name or name usage.</summary>
public sealed class NamingObservation
{
    /// <summary>Gets the stable observation identifier.</summary>
    public string Id { get; init; } = string.Empty;
    /// <summary>Gets the original name.</summary>
    public string Name { get; init; } = string.Empty;
    /// <summary>Gets the normalized comparison name.</summary>
    public string NormalizedName { get; init; } = string.Empty;
    /// <summary>Gets the name role.</summary>
    public NamingNameRole Role { get; init; }
    /// <summary>Gets the symbol kind.</summary>
    public NamingSymbolKind Kind { get; init; }
    /// <summary>Gets the source language.</summary>
    public NamingSourceLanguage Language { get; init; }
    /// <summary>Gets the source file.</summary>
    public NamingSourceFileIdentity? SourceFile { get; init; }
    /// <summary>Gets the source span.</summary>
    public NamingSourceSpan? Location { get; init; }
    /// <summary>Gets the resolved symbol identity.</summary>
    public NamingSymbolIdentity? Symbol { get; init; }
    /// <summary>Gets the containing namespace.</summary>
    public string? Namespace { get; init; }
    /// <summary>Gets the containing type.</summary>
    public string? ContainingType { get; init; }
    /// <summary>Gets whether the observation is excluded from inference.</summary>
    public bool IsExcluded { get; init; }
    /// <summary>Gets the exclusion reason.</summary>
    public string? ExclusionReason { get; init; }
    /// <summary>Gets the naming classification.</summary>
    public NamingClassification Classification { get; init; } = new();
}

/// <summary>Identifies a group used to infer a naming pattern.</summary>
public sealed class NamingPatternScope
{
    /// <summary>Gets the symbol kind.</summary>
    public NamingSymbolKind? SymbolKind { get; init; }
    /// <summary>Gets the source language.</summary>
    public NamingSourceLanguage? Language { get; init; }
    /// <summary>Gets the project name.</summary>
    public string? ProjectName { get; init; }
    /// <summary>Gets the namespace.</summary>
    public string? Namespace { get; init; }
    /// <summary>Gets the file extension.</summary>
    public string? FileExtension { get; init; }
    /// <summary>Gets the stable scope key.</summary>
    public string Key => string.Join("|", SymbolKind?.ToString() ?? "*", Language?.ToString() ?? "*", ProjectName ?? "*", Namespace ?? "*", FileExtension ?? "*");
}

/// <summary>Represents an inferred naming convention.</summary>
public sealed class NamingPattern
{
    /// <summary>Gets the stable pattern identifier.</summary>
    public string Id { get; init; } = string.Empty;
    /// <summary>Gets the pattern scope.</summary>
    public NamingPatternScope Scope { get; init; } = new();
    /// <summary>Gets the dominant style.</summary>
    public NamingStyle DominantStyle { get; init; }
    /// <summary>Gets the common prefixes.</summary>
    public IReadOnlyList<string> CommonPrefixes { get; init; } = Array.Empty<string>();
    /// <summary>Gets the common suffixes.</summary>
    public IReadOnlyList<string> CommonSuffixes { get; init; } = Array.Empty<string>();
    /// <summary>Gets the supporting observations.</summary>
    public IReadOnlyList<string> ObservationIds { get; init; } = Array.Empty<string>();
    /// <summary>Gets the number of applicable observations.</summary>
    public int ObservationCount { get; init; }
    /// <summary>Gets the number matching the dominant pattern.</summary>
    public int MatchingCount { get; init; }
    /// <summary>Gets the number not matching the dominant pattern.</summary>
    public int NonMatchingCount { get; init; }
    /// <summary>Gets the dominance percentage.</summary>
    public double DominancePercentage { get; init; }
    /// <summary>Gets the confidence score.</summary>
    public double Confidence { get; init; }
    /// <summary>Gets whether multiple styles are materially represented.</summary>
    public bool IsMixed { get; init; }
    /// <summary>Gets whether there was insufficient evidence.</summary>
    public bool HasInsufficientEvidence { get; init; }
}

/// <summary>Contains aggregate counts for an analysis scope.</summary>
public sealed class NamingStatistics
{
    /// <summary>Gets the file count.</summary>
    public int FileCount { get; init; }
    /// <summary>Gets the parsed file count.</summary>
    public int ParsedFileCount { get; init; }
    /// <summary>Gets the identifier count.</summary>
    public int IdentifierCount { get; init; }
    /// <summary>Gets the declaration count.</summary>
    public int DeclarationCount { get; init; }
    /// <summary>Gets the reference count.</summary>
    public int ReferenceCount { get; init; }
    /// <summary>Gets the violation count.</summary>
    public int ViolationCount { get; init; }
    /// <summary>Gets the excluded observation count.</summary>
    public int ExcludedCount { get; init; }
    /// <summary>Gets counts by symbol kind.</summary>
    public IReadOnlyDictionary<NamingSymbolKind, int> BySymbolKind { get; init; } = new Dictionary<NamingSymbolKind, int>();
    /// <summary>Gets counts by name role.</summary>
    public IReadOnlyDictionary<NamingNameRole, int> ByRole { get; init; } = new Dictionary<NamingNameRole, int>();
    /// <summary>Gets counts by style.</summary>
    public IReadOnlyDictionary<NamingStyle, int> ByStyle { get; init; } = new Dictionary<NamingStyle, int>();
    /// <summary>Gets the most common tokens.</summary>
    public IReadOnlyDictionary<string, int> CommonTokens { get; init; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    /// <summary>Gets the most common prefixes.</summary>
    public IReadOnlyDictionary<string, int> CommonPrefixes { get; init; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    /// <summary>Gets the most common suffixes.</summary>
    public IReadOnlyDictionary<string, int> CommonSuffixes { get; init; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
}

/// <summary>Represents a naming convention deviation.</summary>
public sealed class NamingViolation
{
    /// <summary>Gets the stable violation identifier.</summary>
    public string Id { get; init; } = string.Empty;
    /// <summary>Gets the observation identifier.</summary>
    public string ObservationId { get; init; } = string.Empty;
    /// <summary>Gets the pattern identifier.</summary>
    public string PatternId { get; init; } = string.Empty;
    /// <summary>Gets the observed name.</summary>
    public string Name { get; init; } = string.Empty;
    /// <summary>Gets the expected style.</summary>
    public NamingStyle ExpectedStyle { get; init; }
    /// <summary>Gets the actual style.</summary>
    public NamingStyle ActualStyle { get; init; }
    /// <summary>Gets the violation severity.</summary>
    public NamingViolationSeverity Severity { get; init; }
    /// <summary>Gets the confidence score.</summary>
    public double Confidence { get; init; }
    /// <summary>Gets the explanation.</summary>
    public string Message { get; init; } = string.Empty;
    /// <summary>Gets an optional suggested replacement.</summary>
    public string? SuggestedName { get; init; }
    /// <summary>Gets the source location.</summary>
    public NamingSourceSpan? Location { get; init; }
    /// <summary>Gets the project identity.</summary>
    public NamingProjectIdentity? Project { get; init; }
    /// <summary>Gets whether the violation is suppressed.</summary>
    public bool IsSuppressed { get; init; }
}

/// <summary>Provides lookup indexes for an analysis result.</summary>
public sealed class NamingAnalysisIndex
{
    /// <summary>Gets observations by identifier.</summary>
    public IReadOnlyDictionary<string, NamingObservation> ObservationsById { get; init; } = new Dictionary<string, NamingObservation>();
    /// <summary>Gets symbols by stable key.</summary>
    public IReadOnlyDictionary<string, NamingSymbolIdentity> SymbolsByKey { get; init; } = new Dictionary<string, NamingSymbolIdentity>();
    /// <summary>Gets observations by file path.</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<NamingObservation>> ObservationsByFile { get; init; } = new Dictionary<string, IReadOnlyList<NamingObservation>>();
    /// <summary>Gets observations by normalized name.</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<NamingObservation>> ObservationsByName { get; init; } = new Dictionary<string, IReadOnlyList<NamingObservation>>();
}

/// <summary>Contains the result of analyzing one project.</summary>
public sealed class NamingProjectAnalysisResult
{
    /// <summary>Gets the project identity.</summary>
    public NamingProjectIdentity Project { get; init; } = new();
    /// <summary>Gets the analysis target.</summary>
    public NamingAnalysisTarget Target { get; init; } = new();
    /// <summary>Gets the completion status.</summary>
    public NamingAnalysisStatus Status { get; init; }
    /// <summary>Gets source files.</summary>
    public IReadOnlyList<NamingSourceFileIdentity> Files { get; init; } = Array.Empty<NamingSourceFileIdentity>();
    /// <summary>Gets name observations.</summary>
    public IReadOnlyList<NamingObservation> Observations { get; init; } = Array.Empty<NamingObservation>();
    /// <summary>Gets inferred patterns.</summary>
    public IReadOnlyList<NamingPattern> Patterns { get; init; } = Array.Empty<NamingPattern>();
    /// <summary>Gets violations.</summary>
    public IReadOnlyList<NamingViolation> Violations { get; init; } = Array.Empty<NamingViolation>();
    /// <summary>Gets aggregate statistics.</summary>
    public NamingStatistics Statistics { get; init; } = new();
    /// <summary>Gets lookup indexes.</summary>
    public NamingAnalysisIndex Index { get; init; } = new();
    /// <summary>Gets diagnostics.</summary>
    public IReadOnlyList<NamingDiagnostic> Diagnostics { get; init; } = Array.Empty<NamingDiagnostic>();
    /// <summary>Gets the analysis duration.</summary>
    public TimeSpan Duration { get; init; }
}

/// <summary>Describes the result for one project in a solution analysis.</summary>
public sealed class NamingSolutionProjectResult
{
    /// <summary>Gets the project identity.</summary>
    public NamingProjectIdentity Project { get; init; } = new();
    /// <summary>Gets dependency project keys.</summary>
    public IReadOnlyList<string> Dependencies { get; init; } = Array.Empty<string>();
    /// <summary>Gets the project status.</summary>
    public NamingAnalysisStatus Status { get; init; }
    /// <summary>Gets the project result.</summary>
    public NamingProjectAnalysisResult? Result { get; init; }
    /// <summary>Gets failure diagnostics.</summary>
    public IReadOnlyList<NamingDiagnostic> Diagnostics { get; init; } = Array.Empty<NamingDiagnostic>();
}

/// <summary>Contains the aggregate result for a solution analysis.</summary>
public sealed class NamingSolutionAnalysisResult
{
    /// <summary>Gets the analysis target.</summary>
    public NamingAnalysisTarget Target { get; init; } = new();
    /// <summary>Gets project results.</summary>
    public IReadOnlyList<NamingSolutionProjectResult> Projects { get; init; } = Array.Empty<NamingSolutionProjectResult>();
    /// <summary>Gets all solution observations.</summary>
    public IReadOnlyList<NamingObservation> Observations { get; init; } = Array.Empty<NamingObservation>();
    /// <summary>Gets solution patterns.</summary>
    public IReadOnlyList<NamingPattern> Patterns { get; init; } = Array.Empty<NamingPattern>();
    /// <summary>Gets solution violations.</summary>
    public IReadOnlyList<NamingViolation> Violations { get; init; } = Array.Empty<NamingViolation>();
    /// <summary>Gets aggregate statistics.</summary>
    public NamingStatistics Statistics { get; init; } = new();
    /// <summary>Gets solution indexes.</summary>
    public NamingAnalysisIndex Index { get; init; } = new();
    /// <summary>Gets solution diagnostics.</summary>
    public IReadOnlyList<NamingDiagnostic> Diagnostics { get; init; } = Array.Empty<NamingDiagnostic>();
    /// <summary>Gets completion status.</summary>
    public NamingAnalysisStatus Status { get; init; }
    /// <summary>Gets the analysis duration.</summary>
    public TimeSpan Duration { get; init; }
}
