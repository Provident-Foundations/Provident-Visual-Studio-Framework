namespace Provident.Foundations.VisualStudio.Model;

/// <summary>
/// Represents a resource reference in the semantic model.
/// </summary>
public sealed class ResourceReferenceModel
{
    /// <summary>
    /// Gets or initializes the referenced key.
    /// </summary>
    public string? Key { get; init; }

    /// <summary>
    /// Gets or initializes the source file.
    /// </summary>
    public string? File { get; init; }

    /// <summary>
    /// Gets or initializes the source line.
    /// </summary>
    public int LineNumber { get; init; }

    /// <summary>
    /// Gets or initializes the reference type.
    /// </summary>
    public ResourceReferenceType ReferenceType { get; init; }
}
