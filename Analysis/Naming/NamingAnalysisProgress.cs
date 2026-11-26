namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>
/// Describes progress for an analysis operation.
/// </summary>
public sealed class NamingAnalysisProgress : Provident.Foundations.Abstractions.DisposableObjectBase
{
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    /// <summary>
    /// Gets the current analysis stage.
    /// </summary>
    public string Stage { get; init; } = string.Empty;

    /// <summary>
    /// Gets the current project name.
    /// </summary>
    public string? ProjectName { get; init; }

    /// <summary>
    /// Gets the current file path.
    /// </summary>
    public string? FilePath { get; init; }

    /// <summary>
    /// Gets the completed item count.
    /// </summary>
    public int CompletedItems { get; init; }

    /// <summary>
    /// Gets the total item count.
    /// </summary>
    public int TotalItems { get; init; }

    /// <summary>
    /// Gets the progress percentage.
    /// </summary>
    public double Percentage => TotalItems <= 0 ? 0 : System.Math.Clamp((double)CompletedItems / TotalItems * 100, 0, 100);
}
