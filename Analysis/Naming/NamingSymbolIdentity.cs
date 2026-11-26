namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>
/// Identifies a resolved source symbol.
/// </summary>
public sealed class NamingSymbolIdentity : Provident.Foundations.Abstractions.DisposableObjectBase
{
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    /// <summary>
    /// Gets the stable Roslyn symbol key or fallback key.
    /// </summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>
    /// Gets the simple symbol name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the fully qualified name.
    /// </summary>
    public string FullyQualifiedName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the symbol kind.
    /// </summary>
    public NamingSymbolKind Kind { get; init; }

    /// <summary>
    /// Gets the containing namespace.
    /// </summary>
    public string? Namespace { get; init; }

    /// <summary>
    /// Gets the containing symbol key.
    /// </summary>
    public string? ContainingSymbolKey { get; init; }

    /// <summary>
    /// Gets the owning project.
    /// </summary>
    public NamingProjectIdentity? Project { get; init; }

    /// <summary>
    /// Gets the accessibility text.
    /// </summary>
    public string? Accessibility { get; init; }

    /// <summary>
    /// Gets whether the symbol is static.
    /// </summary>
    public bool IsStatic { get; init; }

    /// <summary>
    /// Gets whether the symbol has multiple declarations.
    /// </summary>
    public bool IsPartial { get; init; }
}
