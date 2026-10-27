namespace Provident.Foundations.VisualStudio.Model;

/// <summary>
/// Represents a solution folder.
/// </summary>
public sealed class SolutionFolderModel
{
    /// <summary>
    /// Gets or initializes the folder name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets or initializes the folder path.
    /// </summary>
    public string? RelativePath { get; init; }

    /// <summary>
    /// Gets the child project and folder identifiers.
    /// </summary>
    public List<string> Children { get; } = [];
}
