namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>
/// Provides lookup indexes for an analysis result.
/// </summary>
public sealed class NamingAnalysisIndex : Provident.Foundations.Abstractions.DisposableObjectBase
{
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    /// <summary>
    /// Gets observations by identifier.
    /// </summary>
    public IReadOnlyDictionary<string, NamingObservation> ObservationsById { get; init; } = new Dictionary<string, NamingObservation>();

    /// <summary>
    /// Gets symbols by stable key.
    /// </summary>
    public IReadOnlyDictionary<string, NamingSymbolIdentity> SymbolsByKey { get; init; } = new Dictionary<string, NamingSymbolIdentity>();

    /// <summary>
    /// Gets observations by file path.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<NamingObservation>> ObservationsByFile { get; init; } = new Dictionary<string, IReadOnlyList<NamingObservation>>();

    /// <summary>
    /// Gets observations by normalized name.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<NamingObservation>> ObservationsByName { get; init; } = new Dictionary<string, IReadOnlyList<NamingObservation>>();
}
