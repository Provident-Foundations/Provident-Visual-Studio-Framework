using System.Xml.Linq;

namespace Provident.Foundations.VisualStudio.Model;

/// <summary>
/// Represents a parsed XAML document.
/// </summary>
public sealed class XamlDocumentModel
{
    /// <summary>
    /// Gets or initializes the document path.
    /// </summary>
    public string? FilePath { get; init; }

    /// <summary>
    /// Gets or initializes the root element name.
    /// </summary>
    public string? RootElement { get; init; }

    /// <summary>
    /// Gets the parsed XML document when available.
    /// </summary>
    public XDocument? Document { get; init; }

    /// <summary>
    /// Gets the resource dictionaries.
    /// </summary>
    public List<ResourceDictionaryNode> Dictionaries { get; } = [];

    /// <summary>
    /// Gets the styles.
    /// </summary>
    public List<StyleDefinitionModel> Styles { get; } = [];

    /// <summary>
    /// Gets the references.
    /// </summary>
    public List<ResourceReferenceModel> References { get; } = [];
}
