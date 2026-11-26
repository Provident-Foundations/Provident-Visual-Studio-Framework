namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>
/// Describes a source location.
/// </summary>
public sealed class NamingSourceSpan : Provident.Foundations.Abstractions.DisposableObjectBase
{
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    /// <summary>
    /// Gets the source file path.
    /// </summary>
    public string FilePath { get; init; } = string.Empty;

    /// <summary>
    /// Gets the one-based starting line.
    /// </summary>
    public int StartLine { get; init; }

    /// <summary>
    /// Gets the one-based starting column.
    /// </summary>
    public int StartColumn { get; init; }

    /// <summary>
    /// Gets the one-based ending line.
    /// </summary>
    public int EndLine { get; init; }

    /// <summary>
    /// Gets the one-based ending column.
    /// </summary>
    public int EndColumn { get; init; }

    /// <summary>
    /// Gets the zero-based character offset.
    /// </summary>
    public int CharacterOffset { get; init; }

    /// <summary>
    /// Gets the character length.
    /// </summary>
    public int CharacterLength { get; init; }

    /// <summary>
    /// Gets an optional source excerpt.
    /// </summary>
    public string? Text { get; init; }
}
