namespace Provident.Foundations.VisualStudio.Model;

/// <summary>
/// Represents a project SDK declaration.
/// </summary>
public sealed class ProjectSdkModel
{
    /// <summary>
    /// Gets or initializes the SDK name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets or initializes the SDK version.
    /// </summary>
    public string? Version { get; init; }
}
