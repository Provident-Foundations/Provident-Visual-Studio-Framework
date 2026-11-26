using Provident.Foundations.Abstractions;

namespace Provident.Foundations.VisualStudio.Analysis.Naming.Analysis.Project;

/// <summary>
/// Represents the scope and location of a naming analysis.
/// </summary>
public class NamingAnalysisTarget : DisposableObjectBase
{
    /// <summary>
    /// Cleans up resources used by the object.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release managed resources; otherwise, <see langword="false"/>.
    /// </param>
    protected override void Dispose(bool disposing)
    {
        Name = string.Empty;
        Path = null;
        SolutionPath = null;
        ProjectPath = null;
        base.Dispose(disposing);
    }
    /// <summary>
    /// Gets the scope of the analysis.
    /// </summary>
    /// <value>
    /// The project, solution, or file-set scope being analyzed.
    /// </value>
    public NamingAnalysisScope Scope { get; init; }

    /// <summary>
    /// Gets the display name of the analysis target.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets the path of the analysis target, when available.
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// Gets the path of the containing solution, when applicable.
    /// </summary>
    public string? SolutionPath { get; set; }

    /// <summary>
    /// Gets the path of the analyzed project, when applicable.
    /// </summary>
    public string? ProjectPath { get; set; }
}
