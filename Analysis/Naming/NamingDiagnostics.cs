namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>
/// Describes a naming analysis diagnostic.
/// </summary>
public sealed class NamingDiagnostic : Provident.Foundations.Abstractions.DisposableObjectBase
{
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    /// <summary>
    /// Gets the diagnostic code.
    /// </summary>
    public string Code { get; init; } = string.Empty;
    /// <summary>
    /// Gets the diagnostic message.
    /// </summary>
    public string Message { get; init; } = string.Empty;
    /// <summary>
    /// Gets the severity.
    /// </summary>
    public NamingDiagnosticSeverity Severity { get; init; }
    /// <summary>
    /// Gets the related source location.
    /// </summary>
    public NamingSourceSpan? Location { get; init; }
    /// <summary>
    /// Gets the related project path.
    /// </summary>
    public string? ProjectPath { get; init; }
    /// <summary>
    /// Gets the related exception type or message.
    /// </summary>
    public string? Detail { get; init; }
}
