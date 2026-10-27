namespace Provident.Foundations.VisualStudio.Model;

/// <summary>
/// Represents an MSBuild item.
/// </summary>
public class ProjectItemModel
{
    /// <summary>
    /// Gets or initializes the item type.
    /// </summary>
    public string? ItemType { get; init; }

    /// <summary>
    /// Gets or initializes the item include path.
    /// </summary>
    public string? Include { get; init; }

    /// <summary>
    /// Gets or initializes the item update path.
    /// </summary>
    public string? Update { get; init; }

    /// <summary>
    /// Gets or initializes the item remove path.
    /// </summary>
    public string? Remove { get; init; }

    /// <summary>
    /// Gets the item metadata.
    /// </summary>
    public Dictionary<string, string> Metadata { get; } = new(StringComparer.OrdinalIgnoreCase);
}

