namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>Defines the scope of a naming analysis.</summary>
public enum NamingAnalysisScope { Project, Solution, FileSet }

/// <summary>Defines the supported source languages.</summary>
public enum NamingSourceLanguage { CSharp, Xaml, Unknown }

/// <summary>Defines the role of an observed name.</summary>
public enum NamingNameRole { Declaration, Reference, XamlIdentifier, ResourceKey, BindingPath, EventHandler, AttachedProperty, Generated, Unknown }

/// <summary>Defines the symbol categories recognized by the analyzer.</summary>
public enum NamingSymbolKind
{
    Namespace, Class, Record, Struct, Interface, Enum, Delegate, Method, Constructor, Operator,
    Property, Indexer, Field, Constant, Event, Parameter, Local, LocalFunction, TypeParameter,
    EnumMember, XamlIdentifier, ResourceKey, Binding, Other
}

/// <summary>Defines recognized identifier styles.</summary>
public enum NamingStyle { Unknown, PascalCase, CamelCase, SnakeCase, ScreamingSnakeCase, KebabCase, DotSeparated, UnderscorePrefixed, SpaceSeparated, Mixed }

/// <summary>Defines the status of an analysis operation.</summary>
public enum NamingAnalysisStatus { NotStarted, Running, Completed, Partial, Failed, Canceled }

/// <summary>Defines diagnostic severity.</summary>
public enum NamingDiagnosticSeverity { Information, Warning, Error }

/// <summary>Defines naming violation severity.</summary>
public enum NamingViolationSeverity { Information, Warning, Error }

/// <summary>Defines the weighting used for solution pattern aggregation.</summary>
public enum NamingPatternWeighting { ObservationCount, ProjectCount, Both }
