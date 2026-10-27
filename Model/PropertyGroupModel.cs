namespace Provident.Foundations.VisualStudio.Model;

/// <summary>
/// Represents an MSBuild property group.
/// </summary>
public sealed class PropertyGroupModel
{
    /// <summary>
    /// Gets or initializes the condition.
    /// </summary>
    public string? Condition { get; init; }

    /// <summary>
    /// Gets the properties in the group.
    /// </summary>
    public Dictionary<string, string> Properties { get; } = new(StringComparer.OrdinalIgnoreCase);
}

