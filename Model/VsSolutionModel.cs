using Provident.Foundations.Abstractions;

namespace Provident.Foundations.VisualStudio.Model;

/// <summary>
/// Represents a Visual Studio solution and its projects.
/// </summary>
public class VsSolutionModel
{
    /// <summary>
    /// Gets or initializes the solution path.
    /// </summary>
    public string? FilePath { get; init; }

    /// <summary>
    /// Gets or initializes the solution name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets or initializes the solution format.
    /// </summary>
    public SolutionFormat Format { get; init; }

    /// <summary>
    /// Gets the solution configurations.
    /// </summary>
    public List<SolutionConfigurationModel> Configurations { get; } = [];

    /// <summary>
    /// Gets the projects in the solution.
    /// </summary>
    public List<SolutionProjectModel> Projects { get; } = [];

    /// <summary>
    /// Gets the solution folders.
    /// </summary>
    public List<SolutionFolderModel> Folders { get; } = [];
}
