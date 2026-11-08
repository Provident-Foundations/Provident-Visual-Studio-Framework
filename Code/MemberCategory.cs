namespace Provident.Foundations.VisualStudio.Code;

/// <summary>
/// Lists the types of code members that are currently supported.
/// </summary>
public enum MemberCategory
{
    /// <summary>
    /// Indicates no member category was specified.
    /// </summary>
    None = 0,

    /// <summary>
    /// Indicates public event declarations.
    /// </summary>
    PublicEvents,

    /// <summary>
    /// Indicates private field and member declarations.
    /// </summary>
    PrivateFields,

    /// <summary>
    /// Indicates constructors, dispose methods, and destructors.
    /// </summary>
    ConstructorsAndDispose,

    /// <summary>
    /// Indicates public property definitions.
    /// </summary>
    PublicProperties,

    /// <summary>
    /// Indicates public method and function definitions.
    /// </summary>
    PublicMethods,

    /// <summary>
    /// Indicates private methods and function definitions.
    /// </summary>
    PrivateMethods,

    /// <summary>
    /// Indicates members that do not match one of the specialized layout categories.
    /// </summary>
    OtherMembers
}
