using System.Xml.Linq;
using System.Xml;

namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>Extracts naming observations from XAML documents.</summary>
public sealed class XamlNameExtractor
{
    private static readonly XNamespace XamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
    private readonly INamingClassifier _classifier;

    /// <summary>Initializes a new XAML name extractor.</summary>
    public XamlNameExtractor(INamingClassifier? classifier = null)
    {
        _classifier = classifier ?? new NamingClassifier();
    }

    /// <summary>Extracts identifiers, keys, bindings, and event handlers from XAML.</summary>
    public IReadOnlyList<NamingObservation> Extract(
        string source,
        NamingSourceFileIdentity sourceFile,
        NamingProjectIdentity project,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(sourceFile);
        ArgumentNullException.ThrowIfNull(project);
        cancellationToken.ThrowIfCancellationRequested();

        XDocument document = XDocument.Parse(source, LoadOptions.SetLineInfo | LoadOptions.PreserveWhitespace);
        List<NamingObservation> observations = new();
        foreach (XElement element in document.Descendants())
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (XAttribute attribute in element.Attributes())
            {
                string localName = attribute.Name.LocalName;
                if (attribute.Name == XName.Get("Name") || attribute.Name == XamlNamespace + "Name")
                    AddObservation(observations, attribute.Value, NamingSymbolKind.XamlIdentifier, NamingNameRole.XamlIdentifier, sourceFile, project, attribute);
                else if (attribute.Name.Namespace == XamlNamespace && localName == "Key")
                    AddObservation(observations, attribute.Value, NamingSymbolKind.ResourceKey, NamingNameRole.ResourceKey, sourceFile, project, attribute);
                else if (IsEventAttribute(attribute))
                    AddObservation(observations, attribute.Value, NamingSymbolKind.Method, NamingNameRole.EventHandler, sourceFile, project, attribute);
                else if (attribute.Value.Contains("{Binding", StringComparison.OrdinalIgnoreCase))
                    AddBindingObservations(observations, attribute.Value, sourceFile, project, attribute);
                else if (attribute.Name.LocalName.Contains('.', StringComparison.Ordinal))
                    AddObservation(observations, attribute.Name.LocalName, NamingSymbolKind.XamlIdentifier, NamingNameRole.AttachedProperty, sourceFile, project, attribute);
            }
        }
        return observations;
    }

    private void AddBindingObservations(List<NamingObservation> observations, string value, NamingSourceFileIdentity file, NamingProjectIdentity project, XAttribute attribute)
    {
        int start = value.IndexOf("{Binding", StringComparison.OrdinalIgnoreCase) + 8;
        int end = value.IndexOf('}', start);
        string binding = (end < 0 ? value[start..] : value[start..end]).Trim();
        string path = binding.Split(',', 2)[0].Trim();
        if (path.StartsWith("Path=", StringComparison.OrdinalIgnoreCase))
            path = path[5..].Trim();
        else if (path.Contains('='))
            return;
        if (!string.IsNullOrEmpty(path) && !string.Equals(path, ".", StringComparison.Ordinal))
            AddObservation(observations, path, NamingSymbolKind.Binding, NamingNameRole.BindingPath, file, project, attribute);
    }

    private void AddObservation(List<NamingObservation> observations, string name, NamingSymbolKind kind, NamingNameRole role, NamingSourceFileIdentity file, NamingProjectIdentity project, XObject source)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;
        IXmlLineInfo lineInfo = source as IXmlLineInfo ?? new XObjectLineInfo();
        int line = lineInfo.HasLineInfo() ? lineInfo.LineNumber : 1;
        int column = lineInfo.HasLineInfo() ? lineInfo.LinePosition : 1;
        NamingClassification classification = _classifier.Classify(name);
        observations.Add(new NamingObservation
        {
            Id = $"{file.Path}:{line}:{column}:{role}:{name}",
            Name = name,
            NormalizedName = name.TrimStart('_').ToLowerInvariant(),
            Role = role,
            Kind = kind,
            Language = NamingSourceLanguage.Xaml,
            SourceFile = file,
            Location = new NamingSourceSpan { FilePath = file.Path, StartLine = line, StartColumn = column, EndLine = line, EndColumn = column + name.Length, Text = name },
            Symbol = new NamingSymbolIdentity { Key = $"{file.Path}:{role}:{name}", Name = name, FullyQualifiedName = name, Kind = kind, Project = project },
            Classification = classification
        });
    }

    private static bool IsEventAttribute(XAttribute attribute) =>
        attribute.Name.Namespace == XNamespace.None && attribute.Name.LocalName.Length > 1 &&
        char.IsUpper(attribute.Name.LocalName[0]) && attribute.Name.LocalName.EndsWith("Click", StringComparison.Ordinal) ||
        attribute.Name.LocalName.EndsWith("Changed", StringComparison.Ordinal) ||
        attribute.Name.LocalName.EndsWith("Loaded", StringComparison.Ordinal);

    private sealed class XObjectLineInfo : IXmlLineInfo
    {
        public bool HasLineInfo() => false;
        public int LineNumber => 1;
        public int LinePosition => 1;
    }
}
