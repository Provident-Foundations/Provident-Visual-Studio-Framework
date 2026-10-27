namespace Provident.Foundations.VisualStudio.Model;

/// <summary>
/// Represents a package capability.
/// </summary>
public sealed class PackageCapabilityModel
{
    /// <summary>
    /// Gets or initializes the capability name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets or initializes the namespace.
    /// </summary>
    public string? Namespace { get; init; }
}
