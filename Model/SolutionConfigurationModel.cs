namespace Provident.Foundations.VisualStudio.Model;

/// <summary>
/// Represents a solution build configuration.
/// </summary>
public sealed class SolutionConfigurationModel
{
    /// <summary>
    /// Gets or initializes the configuration name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets or initializes the target platform.
    /// </summary>
    public string? Platform { get; init; }

    /// <summary>
    /// Gets or initializes whether the configuration is active.
    /// </summary>
    public bool IsActive { get; init; }
}
