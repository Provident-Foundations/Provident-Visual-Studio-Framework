namespace Provident.Foundations.VisualStudio.Model;

/// <summary>
/// Represents a project SDK item with metadata.
/// </summary>
public sealed class CodeFileModel : ProjectItemModel
{
    /// <summary>
    /// Gets or initializes the language.
    /// </summary>
    public string? Language { get; init; }
}
