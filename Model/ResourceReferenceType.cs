namespace Provident.Foundations.VisualStudio.Model;

/// <summary>
/// Identifies a XAML resource reference kind.
/// </summary>
public enum ResourceReferenceType
{
    /// <summary>
    /// StaticResource reference.
    /// </summary>
    StaticResource,

    /// <summary>
    /// ThemeResource reference.
    /// </summary>
    ThemeResource,

    /// <summary>
    /// BasedOn style reference.
    /// </summary>
    BasedOn,

    /// <summary>
    /// Unknown reference.
    /// </summary>
    Unknown,
}
