namespace Provident.Foundations.VisualStudio.Model;

/// <summary>
/// Represents workspace-level MSBuild files.
/// </summary>
public sealed class WorkspaceModel
{
    /// <summary>
    /// Gets or initializes the workspace path.
    /// </summary>
    public string? RootPath { get; init; }

    /// <summary>
    /// Gets directory-level MSBuild files.
    /// </summary>
    public List<DirectoryBuildFileModel> BuildFiles { get; } = [];

    /// <summary>
    /// Gets solutions in the workspace.
    /// </summary>
    public List<VsSolutionModel> Solutions { get; } = [];
}
