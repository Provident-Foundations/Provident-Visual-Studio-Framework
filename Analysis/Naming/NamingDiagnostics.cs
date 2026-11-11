namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>Describes a naming analysis diagnostic.</summary>
public sealed class NamingDiagnostic
{
    /// <summary>Gets the diagnostic code.</summary>
    public string Code { get; init; } = string.Empty;
    /// <summary>Gets the diagnostic message.</summary>
    public string Message { get; init; } = string.Empty;
    /// <summary>Gets the severity.</summary>
    public NamingDiagnosticSeverity Severity { get; init; }
    /// <summary>Gets the related source location.</summary>
    public NamingSourceSpan? Location { get; init; }
    /// <summary>Gets the related project path.</summary>
    public string? ProjectPath { get; init; }
    /// <summary>Gets the related exception type or message.</summary>
    public string? Detail { get; init; }
}

/// <summary>Describes progress for an analysis operation.</summary>
public sealed class NamingAnalysisProgress
{
    /// <summary>Gets the current analysis stage.</summary>
    public string Stage { get; init; } = string.Empty;
    /// <summary>Gets the current project name.</summary>
    public string? ProjectName { get; init; }
    /// <summary>Gets the current file path.</summary>
    public string? FilePath { get; init; }
    /// <summary>Gets the completed item count.</summary>
    public int CompletedItems { get; init; }
    /// <summary>Gets the total item count.</summary>
    public int TotalItems { get; init; }
    /// <summary>Gets the progress percentage.</summary>
    public double Percentage => TotalItems <= 0 ? 0 : Math.Clamp((double)CompletedItems / TotalItems * 100, 0, 100);
}
