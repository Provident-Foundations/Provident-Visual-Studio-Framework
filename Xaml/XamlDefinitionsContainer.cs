using Provident.Foundations.Abstractions;

namespace Provident.Foundations.VisualStudio.Xaml;

/// <summary>
/// Contains resource definitions, references, dependencies, and diagnostics.
/// </summary>
public sealed class XamlDefinitionsContainer : DisposableObjectBase
{
    #region Private Member Declarations
    /// <summary>
    /// The list of XAML resource definitions.
    /// </summary>
    private Dictionary<string, XamlResourceDefinition>? _resourceDefinitions;

    /// <summary>
    /// The list of duplicated resources.
    /// </summary>
    private List<XamlDuplicateResource>? _duplicateDefinitions;

    /// <summary>
    /// The list of references to the resource definitions.
    /// </summary>
    private List<XamlResourceReference>? _validReferences;

    /// <summary>
    /// The list of references to missing XAML resources.
    /// </summary>
    private List<XamlMissingResource>? _missingResources;

    /// <summary>
    /// The list of style dependency definitions.
    /// </summary>
    private Dictionary<string, List<string>?>? _styleDependencies;
    #endregion


    #region Constructor / Dispose Methods
    /// <summary>
    /// Initializes a new instance of the <see cref="XamlDefinitionsContainer"/> class.
    /// </summary>
    /// <remarks>
    /// This is the default constructor.
    /// </remarks>
    public XamlDefinitionsContainer()
    {
        _resourceDefinitions = new Dictionary<string, XamlResourceDefinition>(StringComparer.OrdinalIgnoreCase);
        _duplicateDefinitions = new List<XamlDuplicateResource>();
        _validReferences = new List<XamlResourceReference>();
        _missingResources = new List<XamlMissingResource>();
        _styleDependencies = new Dictionary<string, List<string>?>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="XamlDefinitionsContainer"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _resourceDefinitions?.Clear();
            _duplicateDefinitions?.Clear();
            _validReferences?.Clear();
            _styleDependencies?.Clear();
            _missingResources?.Clear();
        }

        _resourceDefinitions = null;
        _duplicateDefinitions = null;
        _validReferences = null;
        _missingResources = null;
        _styleDependencies = null;
        base.Dispose(disposing);
    }
    #endregion

    #region Public Properties
    /// <summary>
    /// Gets the reference to the list of resource definitions by key value.
    /// </summary>
    /// <value>
    /// A <see cref="Dictionary{TKey, TValue}"/> of <see cref="string"/> keys and
    /// <see cref="XamlResourceDefinition"/> definitions.
    /// </value>
    public Dictionary<string, XamlResourceDefinition>? ResourceDefinitions => _resourceDefinitions;

    /// <summary>
    /// Gets the reference to the list of duplicate resource definitions.
    /// </summary>
    /// <value>
    /// A <see cref="List{T}"/> of <see cref="XamlDuplicateResource"/> instances.
    /// </value>
    public List<XamlDuplicateResource>? DuplicateDefinitions => _duplicateDefinitions;

    /// <summary>
    /// Gets the reference to the list of resource references.
    /// </summary>
    /// <value>
    /// A <see cref="List{T}"/> of <see cref="XamlResourceReference"/> instances.
    /// </value>
    public List<XamlResourceReference>? References => _validReferences;

    /// <summary>
    /// Gets the reference to the list of missing-resource occurrences.
    /// </summary>
    /// <value>
    /// A <see cref="List{T}"/> of <see cref="XamlMissingResource"/> instances.
    /// </value>
    public List<XamlMissingResource>? MissingResources => _missingResources;


    /// <summary>
    /// Gets the reference to the list of style dependencies listed by style key.
    /// </summary>
    /// <value>
    /// A <see cref="Dictionary{TKey, TValue}"/> of <see cref="string"/> key values and 
    /// <see cref="List{T}"/> of strings containing the depenency values.
    /// </value>
    public Dictionary<string, List<string>?>? StyleDependencies => _styleDependencies;
    #endregion

    #region Public Methods / Functions

    /// <summary>
    /// Adds a missing-resource occurrence to the list.
    /// </summary>
    /// <param name="key">
    /// A string specifying the key value for the missing resource.
    /// </param>
    /// <param name="file">
    /// A string specifying the source file in which the reference occurred.
    /// </param>
    /// <param name="line">
    /// The one-based line number of the XAML source where the reference occurred.
    /// </param>
    public void AddMissingResource(string key, string file, int line)
    {
        _missingResources?.Add(new XamlMissingResource { Key = key, File = file, Line = line });
    }

    /// <summary>
    /// Clears the content of the instance.
    /// </summary>
    public void Clear()
    {
        _resourceDefinitions?.Clear();
        _duplicateDefinitions?.Clear();
        _validReferences?.Clear();
        _styleDependencies?.Clear();
        _missingResources?.Clear();
    }

    /// <summary>
    /// Adds a style dependency to the dependency graph.
    /// </summary>
    /// <param name="styleKey">
    /// A string containing the style ID / key name value that has the dependency.
    /// </param>
    /// <param name="dependencyKey">
    /// A string containing the style ID / key name value on which the style depends.
    /// </param>
    public void AddStyleDependency(string styleKey, string dependencyKey)
    {
        if (string.IsNullOrWhiteSpace(styleKey) || string.IsNullOrWhiteSpace(dependencyKey))
        {
            return;
        }

        if (_styleDependencies is null)
        {
            _styleDependencies = new Dictionary<string, List<string>?>(StringComparer.OrdinalIgnoreCase);
        }

        if (!_styleDependencies.TryGetValue(styleKey, out List<string>? dependencies) || dependencies is null)
        {
            dependencies = [];
            _styleDependencies[styleKey] = dependencies;
        }

        if (!dependencies.Contains(dependencyKey, StringComparer.OrdinalIgnoreCase))
        {
            dependencies.Add(dependencyKey);
        }
    }

    /// <summary>
    /// Ensures that a style dependency graph node exists.
    /// </summary>
    /// <param name="keyValue">
    /// A string containing the style ID / key name value.
    /// </param>
    public void UpdateStyleDependencies(string keyValue)
    {
        if (string.IsNullOrWhiteSpace(keyValue))
        {
            return;
        }

        _styleDependencies ??= new Dictionary<string, List<string>?>(StringComparer.OrdinalIgnoreCase);
        _styleDependencies.TryAdd(keyValue, []);
    }
    #endregion
}
