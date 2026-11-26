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

    /// <summary>
    /// Attempts to get the root namepsace of the project.
    /// </summary>
    /// <returns>
    /// A string containing the root namespace.
    /// </returns>
    public string? GetRootNamespace()
    {
        if (Project is null)
            return null;
        else
            return Project.GetRootNamespace();
    }

    /// <summary>
    /// Gets the list of target framework names.
    /// </summary>
    /// <returns>
    /// A <see cref="List{T}"/> of <see cref="string"/> values containing the framework
    /// names, or <b>null</b> if the property is not present.
    /// </returns>
    public List<string>? GetTargetFrameworkList()
    {
        if (Project is null)
            return null;
        else
            return Project.GetTargetFrameworkList();
    }
}
