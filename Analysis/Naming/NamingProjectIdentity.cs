using Provident.Foundations.Abstractions;
using Provident.Foundations.VisualStudio.Model;

namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>
/// Identifies a project participating in analysis.
/// </summary>
public sealed class NamingProjectIdentity : DisposableObjectBase
{
    /// <summary>
    /// Initializes a new instanc of the <see cref="NamingProjectIdentity"/> instance.
    /// </summary>
    public NamingProjectIdentity()
    {
        TargetFrameworks = [];
    }

    /// <summary>
    /// Cleans up resources used by the object.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release managed resources; otherwise, <see langword="false"/>.
    /// </param>
    protected override void Dispose(bool disposing)
    {
        if (!IsDisposed && disposing)
        {
            TargetFrameworks?.Clear();
        }

        TargetFrameworks = null;
        Name = string.Empty;
        FilePath = null;
        RelativePath = null;
        ProjectGuid = null;
        RootNamespace = null;

        base.Dispose(disposing);
    }
    /// <summary>
    /// Gets the project name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets the absolute project path.
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Gets the project path relative to the solution.
    /// </summary>
    public string? RelativePath { get; set  ; }

    /// <summary>
    /// Gets the project identifier.
    /// </summary>
    public string? ProjectGuid { get; set; }

    /// <summary>
    /// Gets the root namespace.
    /// </summary>
    public string? RootNamespace { get; set; }

    /// <summary>
    /// Gets the target frameworks.
    /// </summary>
    public List<string>? TargetFrameworks { get; set; } 

    /// <summary>
    /// Creates an identity from the existing project models.
    /// </summary>
    public static NamingProjectIdentity From(SolutionProjectModel project)
    {
        ArgumentNullException.ThrowIfNull(project);

        NamingProjectIdentity identity = new NamingProjectIdentity();

        identity.Name = project.Name ?? project.Project?.Name ?? string.Empty;
        identity.FilePath = project.Project?.FilePath;
        identity.RelativePath = project.RelativePath;
        identity.ProjectGuid = project.ProjectGuid ?? project.Project?.ProjectGuid;
        identity.RootNamespace = project.GetRootNamespace();
        identity.TargetFrameworks = project.GetTargetFrameworkList() ?? [];

        return identity;
    }
}
