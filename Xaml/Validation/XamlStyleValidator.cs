using Provident.Foundations.Abstractions;
using Provident.Foundations.Common;
using Provident.Foundations.VisualStudio.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Provident.Foundations.VisualStudio.Xaml.Validation;

/// <summary>
/// Compares XAML files to find resource definitions, missing references, duplicates, and cycles.
/// </summary>
public sealed class XamlStyleValidator : DisposableObjectBase
{
    #region Public Events
    /// <summary>
    /// Occurs when updating the status (usually for a UI display).
    /// </summary>
    public event EventHandler<string>? StatusUpdate;
    #endregion

    #region Private Member Declarations

    /// <summary>
    /// The name of the XAML attribute that contains the resource ID.
    /// </summary>
    private const string KeyAttributeName = "Key";
    /// <summary>
    /// The name of the XAML attribute that references a resource ID.
    /// </summary>
    private const string BasedOnAttributeName = "BasedOn";

    /// <summary>
    /// The list of XAML definitions of resources, valid, missing, and duplicates.
    /// </summary>
    private XamlDefinitionsContainer? _xamlDefs;

    /// <summary>
    /// The list of files to be processed.
    /// </summary>
    private List<string>? _xamlFiles;

    /// <summary>
    /// The log for the errors that occur only in this instance.
    /// </summary>
    private OperationLog? _errorLog;

    /// <summary>
    /// The log for the informational items, warnings, and errors in the operation.
    /// </summary>
    private OperationLog? _operationLog;

    /// <summary>
    /// The list of detected circular references.
    /// </summary>
    private XamlCircularReferenceInfo? _circularRefsList;
    #endregion

    #region Constructor / Dispose Methods
    /// <summary>
    /// Initializes a new instance of the <see cref="XamlStyleValidator"/> class.
    /// </summary>
    /// <remarks>
    /// This is the default constructor.
    /// </remarks>
    public XamlStyleValidator()
    {
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="XamlStyleValidator"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
        if (!IsDisposed && disposing)
        {
            _xamlDefs?.Dispose();
            _circularRefsList?.Dispose();
            _errorLog?.Dispose();
            _operationLog?.Dispose();
        }
        
        _xamlDefs = null;
        _circularRefsList = null;
        _errorLog = null;
        _operationLog = null;
        base.Dispose(disposing);
    }
    #endregion

    #region Public Properties
    /// <summary>
    /// Gets the reference to the collected definitions and references.
    /// </summary>
    /// <value>
    /// A <see cref="XamlDefinitionsContainer"/> instance.
    /// </value>
    public XamlDefinitionsContainer? Data => _xamlDefs;
    #endregion

    #region Private Event Methods
    /// <summary>
    /// Raises the <see cref="StatusUpdate"/> event.
    /// </summary>
    /// <param name="status">
    /// A string containing the status message to be displayed.
    /// </param>
    private void UpdateStatus(string status)
    {
        StatusUpdate?.Invoke(this, status);
    }
    #endregion

    #region Public Methods / Functions
    /// <summary>
    /// Analyzes the list of XAML files for style ID inconsistences - missing ID values, duplicates,
    /// and circular references.
    /// </summary>
    /// <param name="xamlFiles">
    /// A <see cref="Dictionary{TKey, TValue}"/> of string keys that are the file names of the 
    /// XAML files, and the accompanying <see cref="XamlFile"/> values that contain the loaded
    /// and parsed XAML content.
    /// </param>
    /// <returns>
    /// A <see cref="XamlStyleIdValidationReport"/> instance describing the result of the analysis.
    /// </returns>
    public async Task<XamlStyleIdValidationReport> AnalyzeAsync(Dictionary<string, XamlFile> xamlFiles)
    {
        Reset();

        await ReadAndValidateXamlFilesAsync(xamlFiles);

        await CalculateProjectIssuesAsync();

        return GenerateReport();
    }
    #endregion

    #region Private Methods / Functions
    /// <summary>
    /// Reads the XAML code for each of the specified files, and isolates and locates the 
    /// ID values for the XAML styles.
    /// </summary>
    /// <param name="xamlFiles">
    /// A <see cref="Dictionary{TKey, TValue}"/> of string keys that are the file names of the 
    /// XAML files, and the accompanying <see cref="XamlFile"/> values that contain the loaded
    /// and parsed XAML content.
    /// </param>
    private async Task ReadAndValidateXamlFilesAsync(Dictionary<string, XamlFile> xamlFiles)
    {
        foreach (string fileName in xamlFiles.Keys)
        {
            // Log the operation at the start of the file.
            _xamlFiles?.Add(fileName);
            string message = $"Scanning File: '{fileName}'";
            UpdateStatus(message);
            _operationLog?.AddInfo(message);

            // Scan the XAML definition for resources and identifiers.
            using OperationResult scanResult = ScanDefinitions(fileName, xamlFiles[fileName]);
            if (!scanResult.Success)
            {
                _errorLog?.AddFailure(scanResult);
            }

            // Scan the XAML definition for external resource reference identifiers.
            using OperationResult scanRefsResult = ScanReferences(fileName, xamlFiles[fileName]);
            if (!scanRefsResult.Success)
            {
                _errorLog?.AddFailure(scanRefsResult);
            }
        }
    }

    /// <summary>
    /// Analyzes all the loaded content for errors.
    /// </summary>
    private async Task CalculateProjectIssuesAsync()
    {
        await Task.Yield();

        // Look for resource IDs that were specified, but do not exist.
        AnalyzeProjectForMissingResources();

        // Look for circular dependencies (resources that point at resources that point back to the original.)
        AnalyzeProjectForCircularDependencies();

    }

    /// <summary>
    /// Looks for style iD values that are referenced, but do not exist or are not defined.
    /// </summary>
    private void AnalyzeProjectForMissingResources()
    {
        UpdateStatus("Looking For Missing Resources...");

        // Look for resource IDs that were specified, but do not exist.
        int missing = FindMissingResources();
        if (missing > 0)
        {
            _operationLog?.AddFailure($"{missing} missing resource(s) found.");
        }
        else
        {
            _operationLog?.AddInfo($"No missing resources were found.");
        }
    }

    /// <summary>
    /// Looks for circular dependencies in the style references; seeking references that refer
    /// to themselves or where a pair each refers to the same file(s) - causing an infinit loop
    /// or stack overflow.
    /// </summary>
    private void AnalyzeProjectForCircularDependencies()
    {
        UpdateStatus("Looking For Circular dependencies ...");

        // Look for circular dependencies (resources that point at resources that point back to the original.)
        _circularRefsList = DetectCircularDependencies();
        if (_circularRefsList.Cycles?.Count > 0)
        {
            _operationLog?.AddFailure($"{_circularRefsList.CycleCount} circular dependency(ies) found.");
        }
        else
        {
            _operationLog?.AddInfo($"No circular dependencies were found.");
        }
    }

    /// <summary>
    /// Generates the report content.
    /// </summary>
    private XamlStyleIdValidationReport GenerateReport()
    {
        var report = new XamlStyleIdValidationReport();
        report.Generate(_xamlDefs, _circularRefsList);
        return report;
    }

    /// <summary>
    /// Scans an XAML document for keyed definitions and style dependencies.
    /// </summary>
    /// <param name="fileName">
    /// A string containing the source file path and name.
    /// </param>
    /// <param name="xamlDocument">
    /// A <see cref="XamlFile"/> instance containing the loaded XAML document.
    /// </param>
    /// <returns>
    /// An <see cref="OperationResult"/> instance indicating the success or failure of the operation.
    /// </returns>
    private OperationResult ScanDefinitions(string fileName, XamlFile xamlDocument)
    {
        OperationResult result = new OperationResult();

        if (xamlDocument is null || xamlDocument.Document is null)
        {
            result.SetFailureMessage($"The XAML file: {fileName} could not be parsed as an XML document.");
        }
        else if (_xamlDefs is null)
        {
            result.SetFailureMessage($"The XAML definitions were not loaded into memory.");
        }
        else
        { 
            XamlDefinitionsContainer definitions = _xamlDefs;
            var descendants = xamlDocument.Document.Descendants();

            foreach (XElement element in descendants)
            {
                // Look for a "Key" attribute on the element, and if present, read
                // the XAML resource definition.
                XamlResourceDefinition? definition = ReadKeyedResource(fileName, element);
                if (definition is not null)
                {
                    AddToResourceLists(definitions, definition);
                }
            
                // Look for the "based On" attribute.
                XAttribute? basedOnAttribute = XamlFunctions.ReadAttributeFromElement(element, BasedOnAttributeName);
                if (basedOnAttribute is not null)
                {
                    definitions.UpdateStyleDependencies(basedOnAttribute.Value);
                }
            }
            result.Success = true;
        }
        return result;
    }

    /// <summary>
    /// Scans source lines for StaticResource and ThemeResource references.
    /// </summary>
    /// <param name="fileName">The source file path.</param>
    /// <param name="xamlDocument">The loaded XAML file.</param>
    private OperationResult ScanReferences(string fileName, XamlFile xamlDocument)
    {
        OperationResult result = new OperationResult();

        if (_xamlDefs is null)
        {
            result.SetFailureMessage($"The XAML definitions were not loaded into memory.");
        }
        else if (xamlDocument.Lines is null)
        {
            result.SetFailureMessage($"The XAML content has no code.");
        }
        else
        {
            XamlDefinitionsContainer definitions = _xamlDefs;
            bool inXmlComment = false;
            for (int lineNumber = 0; lineNumber < xamlDocument.Lines.Count; lineNumber++)
            {
                string lineText = RemoveXmlComments(xamlDocument.Lines[lineNumber], ref inXmlComment);
                foreach (Match match in XamlRegEx.ResourceReferenceRegex.Matches(lineText))
                {
                    definitions.References!.Add(new XamlResourceReference
                    {
                        Key = match.Groups[1].Value.Trim(),
                        File = fileName,
                        Line = lineNumber + 1
                    });
                }
            }
            result.Success = true;
        }
        return result;
    }

    /// <summary>
    /// Finds references whose keys are not defined.
    /// </summary>
    /// <returns>The number of unique missing keys.</returns>
    private int FindMissingResources()
    {
        XamlDefinitionsContainer definitions = _xamlDefs ?? throw new ObjectDisposedException(nameof(XamlStyleValidator));
        HashSet<string> missingKeys = new(StringComparer.Ordinal);
        definitions.MissingResources!.Clear();

        foreach (XamlResourceReference reference in definitions.References!)
        {
            if (!definitions.ResourceDefinitions!.ContainsKey(reference.Key))
            {
                missingKeys.Add(reference.Key);
                definitions.AddMissingResource(reference.Key, reference.File, reference.Line);
            }
        }

        return missingKeys.Count;
    }

    /// <summary>
    /// Detects cycles in the style dependency graph.
    /// </summary>
    /// <returns>The detected circular dependencies.</returns>
    private XamlCircularReferenceInfo DetectCircularDependencies()
    {
        XamlDefinitionsContainer definitions = _xamlDefs ?? throw new ObjectDisposedException(nameof(XamlStyleValidator));
        XamlCircularReferenceInfo info = new();
        HashSet<string> visited = new(StringComparer.Ordinal);
        HashSet<string> active = new(StringComparer.Ordinal);
        List<string> path = [];
        HashSet<string> cycleKeys = new(StringComparer.Ordinal);

        foreach (string node in definitions.StyleDependencies.Keys)
        {
            Visit(node, definitions.StyleDependencies, visited, active, path, cycleKeys, info);
        }

        return info;
    }

    private static void Visit(
        string current,
        Dictionary<string, List<string>?> graph,
        HashSet<string> visited,
        HashSet<string> active,
        List<string> path,
        HashSet<string> cycleKeys,
        XamlCircularReferenceInfo info)
    {
        if (active.Contains(current))
        {
            int cycleStart = path.IndexOf(current);
            List<string> cycle = path.Skip(cycleStart).Append(current).ToList();
            if (cycleKeys.Add(string.Join("\u001F", cycle)))
            {
                info.Cycles!.Add(cycle);
            }

            return;
        }

        if (!visited.Add(current))
        {
            return;
        }

        active.Add(current);
        path.Add(current);
        if (graph.TryGetValue(current, out List<string>? children))
        {
            foreach (string child in children ?? [])
            {
                Visit(child, graph, visited, active, path, cycleKeys, info);
            }
        }

        path.RemoveAt(path.Count - 1);
        active.Remove(current);
    }

    private static string RemoveXmlComments(string line, ref bool inXmlComment)
    {
        while (true)
        {
            if (inXmlComment)
            {
                int commentEnd = line.IndexOf("-->", StringComparison.Ordinal);
                if (commentEnd < 0)
                {
                    return string.Empty;
                }

                line = line[(commentEnd + 3)..];
                inXmlComment = false;
            }

            int commentStart = line.IndexOf("<!--", StringComparison.Ordinal);
            if (commentStart < 0)
            {
                return line;
            }

            int commentEndOnLine = line.IndexOf("-->", commentStart + 4, StringComparison.Ordinal);
            if (commentEndOnLine >= 0)
            {
                line = line.Remove(commentStart, commentEndOnLine + 3 - commentStart);
            }
            else
            {
                inXmlComment = true;
                return line[..commentStart];
            }
        }
    }
    private XamlResourceDefinition? ReadKeyedResource(string fileName, XElement element)
    {
        XamlResourceDefinition? definition = null;

        // Look for a "Key" attribute on the element.
        string? keyName = XamlFunctions.ReadAttributeValueFromElement(element, KeyAttributeName);
        if (!string.IsNullOrEmpty(keyName))
        {
            definition = new()
            {
                Key = keyName,
                File = fileName,
                ElementType = element.Name.LocalName
            };
        }
        return definition;
    }
    private void AddToResourceLists(XamlDefinitionsContainer definitions, XamlResourceDefinition definition)
    {
        // If the key has not yet been used, add it to the current resource definitions list.
        string key = definition.Key;
        if (!definitions.ResourceDefinitions!.TryAdd(key, definition))
        {
            // Otherwise, add it to the duplicated resources list.
            definitions.DuplicateDefinitions!.Add(
                new XamlDuplicateResource
                {
                    Key = key,
                    ExistingFile = definitions.ResourceDefinitions[key].File,
                    DuplicateFile = definition.File
                });
        }
    }
    private void Reset()
    {
        if (_xamlDefs is null)
        {
            _xamlDefs = new();
        }
        if (_xamlFiles is null)
        {
            _xamlFiles = [];
        }
        if (_errorLog is null)
        {
            _errorLog = new();
        }
        if (_operationLog is null)
        {
            _operationLog = new();
        }
        if (_circularRefsList is null)
        {
            _circularRefsList = new();
        }

        _xamlDefs.Clear();
        _xamlFiles.Clear();
        
    }
    #endregion
}

