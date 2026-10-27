namespace Provident.Foundations.VisualStudio.Model;

/// <summary>
/// Represents a XAML style definition.
/// </summary>
public sealed class StyleDefinitionModel
{
    /// <summary>
    /// Gets or initializes the style key.
    /// </summary>
    public string? Key { get; init; }

    /// <summary>
    /// Gets or initializes the target type.
    /// </summary>
    public string? TargetType { get; init; }

    /// <summary>
    /// Gets or initializes the base style key.
    /// </summary>
    public string? BasedOn { get; init; }

    /// <summary>
    /// Gets or initializes the source file.
    /// </summary>
    public string? File { get; init; }
}
