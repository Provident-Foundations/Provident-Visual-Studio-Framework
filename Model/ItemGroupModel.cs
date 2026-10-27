namespace Provident.Foundations.VisualStudio.Model;

/// <summary>
/// Represents an MSBuild item group.
/// </summary>
public sealed class ItemGroupModel
{
    /// <summary>
    /// Gets or initializes the condition.
    /// </summary>
    public string? Condition { get; init; }

    /// <summary>
    /// Gets the items in the group.
    /// </summary>
    public List<ProjectItemModel> Items { get; } = [];
}
