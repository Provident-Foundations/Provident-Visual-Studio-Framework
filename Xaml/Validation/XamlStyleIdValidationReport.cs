using Provident.Foundations.Abstractions;

namespace Provident.Foundations.VisualStudio.Xaml.Validation;

/// <summary>
/// Provides the container for the results of the style ID/key validation for an XAML project.
/// </summary>
public sealed class XamlStyleIdValidationReport : DisposableObjectBase
{
    #region Private Member Declarations
    /// <summary>
    /// The summary data.
    /// </summary>
    private XamlValidationSummary? _summary;

    /// <summary>
    /// The list of duplicated style ID values.
    /// </summary>
    private List<XamlDuplicateResource>? _duplicates;

    /// <summary>
    /// The list of missing style ID values.
    /// </summary>
    private List<XamlMissingResource>? _missing;

    /// <summary>
    /// The list of circulare dependencies.
    /// </summary>
    private List<string>? _circularDependencies;

    /// <summary>
    /// The pass/fail flag.
    /// </summary>
    private bool _passed;

    /// <summary>
    /// The total number of errors.
    /// </summary>
    private int _errorCount;
    #endregion

    #region Public Properties   
    /// <summary>
    /// Gets the reference to the list of circulare dependencies.
    /// </summary>
    /// <value>
    /// A <see cref="List{T}"/> of <see cref="string"/> values.
    /// </value>
    public List<string>? CircularDependencies => _circularDependencies;

    /// <summary>
    /// Gets the reference to the list of duplicated XAML style ID items.
    /// </summary>
    /// <value>
    /// A <see cref="List{T}"/> of <see cref="XamlDuplicateResource"/> values.
    /// </value>
    public List<XamlDuplicateResource>? Duplicates => _duplicates;

    /// <summary>
    /// Gets the number of errors that were found.
    /// </summary>
    /// <value>
    /// An integer specifying the number of errors.
    /// </value>
    public int ErrorCount => _errorCount;

    /// <summary>
    /// Gets the reference to the list of missing resources.
    /// </summary>
    /// <value>
    /// A <see cref="List{T}"/> of <see cref="XamlMissingResource"/> instances.
    /// </value>
    public List<XamlMissingResource>? Missing => _missing;

    /// <summary>
    /// Gets a value indicating whether the validation passed.
    /// </summary>
    /// <value>
    /// <b>true</b> if no errors were found; otherwise, returns <b>false</b>.
    /// </value>
    public bool Passed => _passed;

    /// <summary>
    /// Gets the reference to the summary information object.
    /// </summary>
    /// <value>
    /// An <see cref="XamlValidationSummary"/> containing the summary data.
    /// </value>
    public XamlValidationSummary? Summary => _summary;
    #endregion

    #region Public Methods / Functions
    /// <summary>
    /// Generates the validation summary and detailed findings.
    /// </summary>
    /// <param name="container">
    /// The <see cref="XamlDefinitionsContainer"/> instance containing the collected definitions and references.
    /// </param>
    /// <param name="info">
    /// The <see cref="XamlCircularReferenceInfo"/> instance containing the circular dependency information.
    /// </param>
    public void Generate(XamlDefinitionsContainer? container, XamlCircularReferenceInfo? info)
    {
        _summary = new XamlValidationSummary(container, info);
        _duplicates = [];
        _errorCount = 0;

        if (container is not null)
        {
            if (container.DuplicateDefinitions is { Count: > 0 } duplicates)
            {
                foreach (XamlDuplicateResource duplicate in duplicates)
                {
                    _duplicates.Add(duplicate);
                }
                _errorCount += container.DuplicateDefinitions.Count;
            }


            _missing = [];
            if (container.MissingResources is { Count: > 0 } missingResources)
            {
                foreach (XamlMissingResource missing in missingResources)
                {
                    _missing.Add(missing);
                }
                _errorCount += container.MissingResources.Count;
            }
        }

        _circularDependencies = [];
        if (info is not null)
        {
            if (info.Cycles is { Count: > 0 } cycles)
            {
                foreach (List<string> cycle in cycles)
                {
                    _circularDependencies.Add(string.Join(" -> ", cycle));
                }
            }
            _errorCount += info.CycleCount;
        }

        _passed = (_errorCount == 0);
    }
    #endregion
}
