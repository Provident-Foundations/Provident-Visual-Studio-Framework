using Provident.Foundations.VisualStudio.Analysis.Naming.Analysis;

namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>
/// Defines an intentional naming-analysis exception.
/// </summary>
public sealed class NamingSuppression : Provident.Foundations.Abstractions.DisposableObjectBase
{
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    /// <summary>
    /// Gets the suppression key.
    /// </summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>
    /// Gets the suppression scope.
    /// </summary>
    public NamingAnalysisScope Scope { get; init; }

    /// <summary>
    /// Gets the matching project, namespace, file, or symbol.
    /// </summary>
    public string Match { get; init; } = string.Empty;

    /// <summary>
    /// Gets the reason for the suppression.
    /// </summary>
    public string Reason { get; init; } = string.Empty;

    /// <summary>
    /// Gets the author of the suppression.
    /// </summary>
    public string? Author { get; init; }

    /// <summary>
    /// Gets the optional expiration date.
    /// </summary>
    public DateTimeOffset? Expires { get; init; }
}
