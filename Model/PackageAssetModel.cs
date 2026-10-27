namespace Provident.Foundations.VisualStudio.Model;

/// <summary>
/// Represents a package visual asset.
/// </summary>
public sealed class PackageAssetModel
{
    /// <summary>
    /// Gets or initializes the asset role.
    /// </summary>
    public string? Role { get; init; }

    /// <summary>
    /// Gets or initializes the asset path.
    /// </summary>
    public string? Path { get; init; }
}
