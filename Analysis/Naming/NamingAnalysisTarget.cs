using Provident.Foundations.VisualStudio.Analysis.Naming.Analysis;

namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>
/// Identifies the scope being analyzed.
/// </summary>
public sealed class NamingAnalysisTarget : Provident.Foundations.Abstractions.DisposableObjectBase
{
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    /// <summary>
    /// Gets the target scope.
    /// </summary>
    public NamingAnalysisScope Scope { get; init; }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the target path.
    /// </summary>
    public string? Path { get; init; }

    /// <summary>
    /// Gets the solution path, when applicable.
    /// </summary>
    public string? SolutionPath { get; init; }

    /// <summary>
    /// Gets the project path, when applicable.
    /// </summary>
    public string? ProjectPath { get; init; }
}
