namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>
/// Defines the symbol categories recognized by the analyzer.
/// </summary>
public enum NamingSymbolKind
{
    Namespace,
    Class,
    Record,
    Struct,
    Interface,
    Enum,
    Delegate,
    Method,
    Constructor,
    Operator,
    Property,
    Indexer,
    Field,
    Constant,
    Event,
    Parameter,
    Local,
    LocalFunction,
    TypeParameter,
    EnumMember,
    XamlIdentifier,
    ResourceKey,
    Binding,
    Other
}
