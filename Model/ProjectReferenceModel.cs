namespace Provident.Foundations.VisualStudio.Model;

/// <summary>
/// Represents a project-to-project reference.
/// </summary>
public sealed class ProjectReferenceModel
{
    /// <summary>
    /// Gets or initializes the referenced project path.
    /// </summary>
    public string? Include { get; init; }

    /// <summary>
    /// Gets or initializes the referenced project GUID.
    /// </summary>
    public string? ProjectGuid { get; init; }

    /// <summary>
    /// Gets or initializes the reference name.
    /// </summary>
    public string? Name { get; init; }
}
