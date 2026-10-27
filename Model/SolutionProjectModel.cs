namespace Provident.Foundations.VisualStudio.Model;

/// <summary>
/// Represents a solution project entry.
/// </summary>
public sealed class SolutionProjectModel
{
    /// <summary>
    /// Gets or initializes the project name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets or initializes the project path.
    /// </summary>
    public string? RelativePath { get; init; }

    /// <summary>
    /// Gets or initializes the project GUID.
    /// </summary>
    public string? ProjectGuid { get; init; }

    /// <summary>
    /// Gets or initializes the project model.
    /// </summary>
    public ProjectModel? Project { get; init; }
}
