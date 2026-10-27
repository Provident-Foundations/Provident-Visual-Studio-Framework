namespace Provident.Foundations.VisualStudio.Model;

/// <summary>
/// Represents an application declaration in a package manifest.
/// </summary>
public sealed class PackageApplicationModel
{
    /// <summary>
    /// Gets or initializes the application identifier.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// Gets or initializes the executable.
    /// </summary>
    public string? Executable { get; init; }

    /// <summary>
    /// Gets or initializes the entry point.
    /// </summary>
    public string? EntryPoint { get; init; }

    /// <summary>
    /// Gets package extensions.
    /// </summary>
    public List<PackageExtensionModel> Extensions { get; } = [];
}
