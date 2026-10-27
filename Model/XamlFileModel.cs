namespace Provident.Foundations.VisualStudio.Model;

/// <summary>
/// Represents a XAML file in a project.
/// </summary>
public sealed class XamlFileModel : ProjectItemModel
{
    /// <summary>
    /// Gets or initializes the parsed XAML document.
    /// </summary>
    public XamlDocumentModel? ParsedDocument { get; init; }
}
