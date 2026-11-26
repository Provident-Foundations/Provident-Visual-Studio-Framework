namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>
/// Represents the tokenized form of an identifier.
/// </summary>
public sealed class NamingTokenization : Provident.Foundations.Abstractions.DisposableObjectBase
{
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    /// <summary>
    /// Gets the original identifier.
    /// </summary>
    public string Original { get; init; } = string.Empty;

    /// <summary>
    /// Gets the ordered identifier tokens.
    /// </summary>
    public IReadOnlyList<string> Tokens { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the leading prefix.
    /// </summary>
    public string? Prefix { get; init; }

    /// <summary>
    /// Gets the trailing suffix.
    /// </summary>
    public string? Suffix { get; init; }

    /// <summary>
    /// Gets whether acronym tokens were detected.
    /// </summary>
    public bool ContainsAcronym { get; init; }

    /// <summary>
    /// Gets whether numeric tokens were detected.
    /// </summary>
    public bool ContainsNumericToken { get; init; }

    /// <summary>
    /// Gets separator characters encountered in the identifier.
    /// </summary>
    public IReadOnlyList<char> Separators { get; init; } = Array.Empty<char>();
}
