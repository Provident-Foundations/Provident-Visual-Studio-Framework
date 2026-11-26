namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>
/// Contains aggregate counts for an analysis scope.
/// </summary>
public sealed class NamingStatistics : Provident.Foundations.Abstractions.DisposableObjectBase
{
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    /// <summary>
    /// Gets the file count.
    /// </summary>
    public int FileCount { get; init; }

    /// <summary>
    /// Gets the parsed file count.
    /// </summary>
    public int ParsedFileCount { get; init; }

    /// <summary>
    /// Gets the identifier count.
    /// </summary>
    public int IdentifierCount { get; init; }

    /// <summary>
    /// Gets the declaration count.
    /// </summary>
    public int DeclarationCount { get; init; }

    /// <summary>
    /// Gets the reference count.
    /// </summary>
    public int ReferenceCount { get; init; }

    /// <summary>
    /// Gets the violation count.
    /// </summary>
    public int ViolationCount { get; init; }

    /// <summary>
    /// Gets the excluded observation count.
    /// </summary>
    public int ExcludedCount { get; init; }

    /// <summary>
    /// Gets counts by symbol kind.
    /// </summary>
    public IReadOnlyDictionary<NamingSymbolKind, int> BySymbolKind { get; init; } = new Dictionary<NamingSymbolKind, int>();

    /// <summary>
    /// Gets counts by name role.
    /// </summary>
    public IReadOnlyDictionary<NamingNameRole, int> ByRole { get; init; } = new Dictionary<NamingNameRole, int>();

    /// <summary>
    /// Gets counts by style.
    /// </summary>
    public IReadOnlyDictionary<NamingStyle, int> ByStyle { get; init; } = new Dictionary<NamingStyle, int>();

    /// <summary>
    /// Gets the most common tokens.
    /// </summary>
    public IReadOnlyDictionary<string, int> CommonTokens { get; init; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the most common prefixes.
    /// </summary>
    public IReadOnlyDictionary<string, int> CommonPrefixes { get; init; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the most common suffixes.
    /// </summary>
    public IReadOnlyDictionary<string, int> CommonSuffixes { get; init; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
}
