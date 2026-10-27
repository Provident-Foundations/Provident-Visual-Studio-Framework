namespace Provident.Foundations.VisualStudio.Model;

/// <summary>
/// Represents a WinUI 3 application and MSIX packaging model.
/// </summary>
public sealed class WinUiPackagingModel
{
    /// <summary>
    /// Gets or initializes the package identity.
    /// </summary>
    public PackageIdentityModel Identity { get; init; } = new();

    /// <summary>
    /// Gets or initializes the application manifest path.
    /// </summary>
    public string? ManifestPath { get; set; }

    /// <summary>
    /// Gets or initializes the Windows App SDK version.
    /// </summary>
    public string? WindowsAppSdkVersion { get; set; }

    /// <summary>
    /// Gets or initializes the Windows App SDK self-contained setting.
    /// </summary>
    public bool SelfContained { get; init; }

    /// <summary>
    /// Gets package capabilities.
    /// </summary>
    public List<PackageCapabilityModel> Capabilities { get; } = [];

    /// <summary>
    /// Gets declared applications.
    /// </summary>
    public List<PackageApplicationModel> Applications { get; } = [];

    /// <summary>
    /// Gets visual assets.
    /// </summary>
    public List<PackageAssetModel> Assets { get; } = [];
}
