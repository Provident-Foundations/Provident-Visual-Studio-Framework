namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>
/// Identifies a group used to infer a naming pattern.
/// </summary>
public sealed class NamingPatternScope : Provident.Foundations.Abstractions.DisposableObjectBase
{
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    /// <summary>
    /// Gets the symbol kind.
    /// </summary>
    public NamingSymbolKind? SymbolKind { get; init; }

    /// <summary>
    /// Gets the source language.
    /// </summary>
    public NamingSourceLanguage? Language { get; init; }

    /// <summary>
    /// Gets the project name.
    /// </summary>
    public string? ProjectName { get; init; }

    /// <summary>
    /// Gets the namespace.
    /// </summary>
    public string? Namespace { get; init; }

    /// <summary>
    /// Gets the file extension.
    /// </summary>
    public string? FileExtension { get; init; }

    /// <summary>
    /// Gets the stable scope key.
    /// </summary>
    public string Key => string.Join("|", SymbolKind?.ToString() ?? "*", Language?.ToString() ?? "*", ProjectName ?? "*", Namespace ?? "*", FileExtension ?? "*");
}
