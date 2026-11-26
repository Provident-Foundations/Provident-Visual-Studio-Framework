namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>
/// Describes the naming characteristics of one identifier.
/// </summary>
public sealed class NamingClassification : Provident.Foundations.Abstractions.DisposableObjectBase
{
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    /// <summary>
    /// Gets the primary naming style.
    /// </summary>
    public NamingStyle Style { get; init; }

    /// <summary>
    /// Gets classification confidence from zero through one.
    /// </summary>
    public double Confidence { get; init; }

    /// <summary>
    /// Gets the tokenization result.
    /// </summary>
    public NamingTokenization Tokenization { get; init; } = new();

    /// <summary>
    /// Gets whether the identifier has a leading underscore.
    /// </summary>
    public bool HasLeadingUnderscore { get; init; }

    /// <summary>
    /// Gets whether the identifier has a trailing asynchronous suffix.
    /// </summary>
    public bool HasAsyncSuffix { get; init; }

    /// <summary>
    /// Gets whether the identifier appears abbreviated.
    /// </summary>
    public bool AppearsAbbreviated { get; init; }

    /// <summary>
    /// Gets the identifier's length.
    /// </summary>
    public int Length { get; init; }
}
