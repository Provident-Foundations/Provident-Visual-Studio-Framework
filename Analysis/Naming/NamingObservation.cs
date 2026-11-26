namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>
/// Represents one extracted name or name usage.
/// </summary>
public sealed class NamingObservation : Provident.Foundations.Abstractions.DisposableObjectBase
{
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    /// <summary>
    /// Gets the stable observation identifier.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the original name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the normalized comparison name.
    /// </summary>
    public string NormalizedName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the name role.
    /// </summary>
    public NamingNameRole Role { get; init; }

    /// <summary>
    /// Gets the symbol kind.
    /// </summary>
    public NamingSymbolKind Kind { get; init; }

    /// <summary>
    /// Gets the source language.
    /// </summary>
    public NamingSourceLanguage Language { get; init; }

    /// <summary>
    /// Gets the source file.
    /// </summary>
    public NamingSourceFileIdentity? SourceFile { get; init; }

    /// <summary>
    /// Gets the source span.
    /// </summary>
    public NamingSourceSpan? Location { get; init; }

    /// <summary>
    /// Gets the resolved symbol identity.
    /// </summary>
    public NamingSymbolIdentity? Symbol { get; init; }

    /// <summary>
    /// Gets the containing namespace.
    /// </summary>
    public string? Namespace { get; init; }

    /// <summary>
    /// Gets the containing type.
    /// </summary>
    public string? ContainingType { get; init; }

    /// <summary>
    /// Gets whether the observation is excluded from inference.
    /// </summary>
    public bool IsExcluded { get; init; }

    /// <summary>
    /// Gets the exclusion reason.
    /// </summary>
    public string? ExclusionReason { get; init; }

    /// <summary>
    /// Gets the naming classification.
    /// </summary>
    public NamingClassification Classification { get; init; } = new();
}
