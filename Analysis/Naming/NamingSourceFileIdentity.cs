namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>
/// Identifies a source file participating in analysis.
/// </summary>
public sealed class NamingSourceFileIdentity : Provident.Foundations.Abstractions.DisposableObjectBase
{
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    /// <summary>
    /// Gets the absolute file path.
    /// </summary>
    public string Path { get; init; } = string.Empty;

    /// <summary>
    /// Gets the project-relative path.
    /// </summary>
    public string? RelativePath { get; init; }

    /// <summary>
    /// Gets the file name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the file extension.
    /// </summary>
    public string Extension { get; init; } = string.Empty;

    /// <summary>
    /// Gets the source language.
    /// </summary>
    public NamingSourceLanguage Language { get; init; }

    /// <summary>
    /// Gets the owning project.
    /// </summary>
    public NamingProjectIdentity? Project { get; init; }

    /// <summary>
    /// Gets whether the file appears to be generated.
    /// </summary>
    public bool IsGenerated { get; init; }

    /// <summary>
    /// Gets the content hash, when calculated.
    /// </summary>
    public string? ContentHash { get; init; }
}
