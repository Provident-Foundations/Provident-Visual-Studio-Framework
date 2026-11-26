using Provident.Foundations.Abstractions;

namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>
/// Describes the result for one project in a solution analysis.
/// </summary>
public sealed class NamingSolutionProjectResult : DisposableObjectBase
{
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
            if (Result is not null)
            {
                Result.Dispose();
            }
            else if (Diagnostics is not null)
            {
                foreach (NamingDiagnostic diagnostic in Diagnostics)
                {
                    diagnostic.Dispose();
                }
            }

            Project = null;
            Dependencies = null;
            Result = null;
            Diagnostics = null;
        }

        base.Dispose(disposing);
    }
    /// <summary>
    /// Gets the project identity.
    /// </summary>
    public NamingProjectIdentity? Project { get; set; } = new();

    /// <summary>
    /// Gets dependency project keys.
    /// </summary>
    public List<string>? Dependencies { get; set; } = [];

    /// <summary>
    /// Gets the project status.
    /// </summary>
    public NamingAnalysisStatus Status { get; set; }

    /// <summary>
    /// Gets the project result.
    /// </summary>
    public NamingProjectAnalysisResult? Result { get; set; }

    /// <summary>
    /// Gets failure diagnostics.
    /// </summary>
    public IReadOnlyList<NamingDiagnostic>? Diagnostics { get; set; } = Array.Empty<NamingDiagnostic>();
}
