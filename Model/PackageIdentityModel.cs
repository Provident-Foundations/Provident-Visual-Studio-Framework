namespace Provident.Foundations.VisualStudio.Model;

/// <summary>
/// Represents a package identity.
/// </summary>
public sealed class PackageIdentityModel
{
    /// <summary>
    /// Gets or initializes the package name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets or initializes the publisher.
    /// </summary>
    public string? Publisher { get; init; }

    /// <summary>
    /// Gets or initializes the version.
    /// </summary>
    public string? Version { get; init; }

    /// <summary>
    /// Gets or initializes the processor architecture.
    /// </summary>
    public string? Architecture { get; init; }
}
