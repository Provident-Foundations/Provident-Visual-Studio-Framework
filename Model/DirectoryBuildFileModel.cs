namespace Provident.Foundations.VisualStudio.Model;

/// <summary>
/// Represents a Directory.Build.props or Directory.Build.targets file.
/// </summary>
public sealed class DirectoryBuildFileModel
{
    /// <summary>
    /// Gets or initializes the file path.
    /// </summary>
    public string? FilePath { get; init; }

    /// <summary>
    /// Gets or initializes whether this is a targets file.
    /// </summary>
    public bool IsTargetsFile { get; init; }

    /// <summary>
    /// Gets the declared properties and items.
    /// </summary>
    /// <value>
    /// Gets the value represented by this property.
    /// </value>
    public List<ProjectItemModel> Items { get; } = [];
}
