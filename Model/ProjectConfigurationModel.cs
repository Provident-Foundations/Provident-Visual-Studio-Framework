namespace Provident.Foundations.VisualStudio.Model;

/// <summary>
/// Represents a project build configuration.
/// </summary>
public sealed class ProjectConfigurationModel
{
    /// <summary>
    /// Gets or initializes the configuration name.
    /// </summary>
    public string? Configuration { get; init; }

    /// <summary>
    /// Gets or initializes the target platform.
    /// </summary>
    public string? Platform { get; init; }

    /// <summary>
    /// Gets or initializes the target framework.
    /// </summary>
    public string? TargetFramework { get; init; }

    /// <summary>
    /// Gets or initializes the runtime identifier.
    /// </summary>
    public string? RuntimeIdentifier { get; init; }

    /// <summary>
    /// Gets or initializes whether the project is built.
    /// </summary>
    public bool Build { get; init; } = true;
}
