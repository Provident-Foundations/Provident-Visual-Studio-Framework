namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>
/// Defines the status of an analysis operation.
/// </summary>
public enum NamingAnalysisStatus
{
    NotStarted,
    Running,
    Completed,
    Partial,
    Failed,
    Canceled
}
