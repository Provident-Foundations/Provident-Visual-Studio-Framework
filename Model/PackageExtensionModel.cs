namespace Provident.Foundations.VisualStudio.Model;

/// <summary>
/// Represents an application extension.
/// </summary>
public sealed class PackageExtensionModel
{
    /// <summary>
    /// Gets or initializes the extension category.
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// Gets or initializes the executable.
    /// </summary>
    public string? Executable { get; init; }

    /// <summary>
    /// Gets package extension attributes.
    /// </summary>
    public Dictionary<string, string> Attributes { get; } = new(StringComparer.OrdinalIgnoreCase);
}
