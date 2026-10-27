namespace Provident.Foundations.VisualStudio.Model;

/// <summary>
/// Represents a package reference.
/// </summary>
public sealed class PackageReferenceModel
{
    /// <summary>
    /// Gets or initializes the package identifier.
    /// </summary>
    public string? Include { get; init; }

    /// <summary>
    /// Gets or initializes the package version.
    /// </summary>
    public string? Version { get; init; }

    /// <summary>
    /// Gets or initializes whether assets are private.
    /// </summary>
    public bool PrivateAssets { get; init; }
}
