namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>
/// Represents a naming convention deviation.
/// </summary>
public sealed class NamingViolation : Provident.Foundations.Abstractions.DisposableObjectBase
{
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    /// <summary>
    /// Gets the stable violation identifier.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the observation identifier.
    /// </summary>
    public string ObservationId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the pattern identifier.
    /// </summary>
    public string PatternId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the observed name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the expected style.
    /// </summary>
    public NamingStyle ExpectedStyle { get; init; }

    /// <summary>
    /// Gets the actual style.
    /// </summary>
    public NamingStyle ActualStyle { get; init; }

    /// <summary>
    /// Gets the violation severity.
    /// </summary>
    public NamingViolationSeverity Severity { get; init; }

    /// <summary>
    /// Gets the confidence score.
    /// </summary>
    public double Confidence { get; init; }

    /// <summary>
    /// Gets the explanation.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Gets an optional suggested replacement.
    /// </summary>
    public string? SuggestedName { get; init; }

    /// <summary>
    /// Gets the source location.
    /// </summary>
    public NamingSourceSpan? Location { get; init; }

    /// <summary>
    /// Gets the project identity.
    /// </summary>
    public NamingProjectIdentity? Project { get; init; }

    /// <summary>
    /// Gets whether the violation is suppressed.
    /// </summary>
    public bool IsSuppressed { get; init; }
}
