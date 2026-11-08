using Provident.Foundations.Abstractions;

namespace Provident.Foundations.VisualStudio.Code;

/// <summary>
/// Represents and manages a region definition in code.
/// </summary>
public sealed class RegionDefinition : DisposableObjectBase
{
    /// <summary>
    /// Gets or sets the category of the region definition.
    /// </summary>
    /// <value>
    /// A <see cref="MemberCategory"/> enumerated value.
    /// </value>
    public MemberCategory Category { get; init; }

    /// <summary>
    /// Gets or sets the name for the region.
    /// </summary>
    /// <value>
    /// A string containing the region name.
    /// </value>
    public string Name { get; init; } = string.Empty;



}
