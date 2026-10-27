namespace Provident.Foundations.VisualStudio.Model;

/// <summary>
/// Represents a resource definition in the semantic model.
/// </summary>
public sealed class ResourceDefinitionModel
{
    /// <summary>
    /// Gets or initializes the resource key.
    /// </summary>
    public string? Key { get; init; }

    /// <summary>
    /// Gets or initializes the source file.
    /// </summary>
    public string? File { get; init; }

    /// <summary>
    /// Gets or initializes the element type.
    /// </summary>
    public string? ElementType { get; init; }

    /// <summary>
    /// Gets or initializes the source line.
    /// </summary>
    public int LineNumber { get; init; }
}
