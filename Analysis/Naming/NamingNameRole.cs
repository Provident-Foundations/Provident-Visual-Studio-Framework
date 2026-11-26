namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>
/// Defines the role of an observed name.
/// </summary>
public enum NamingNameRole
{
    Declaration,
    Reference,
    XamlIdentifier,
    ResourceKey,
    BindingPath,
    EventHandler,
    AttachedProperty,
    Generated,
    Unknown
}
